using System.Globalization;
using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Middleware;
using FinanzasApi.Models;
using FinanzasApi.Repositories;

namespace FinanzasApi.Services;

public interface ITransactionService
{
    Task<TransactionPageDto> SearchAsync(TransactionQuery query, string currentUserId, bool isAdmin);
    Task<TransactionResponseDto> GetByIdAsync(string id, string currentUserId, bool isAdmin);
    Task<TransactionResultDto> CreateAsync(CreateTransactionDto dto, string currentUserId, bool isAdmin);
    Task<TransactionResultDto> UpdateAsync(string id, UpdateTransactionDto dto, string currentUserId, bool isAdmin);
    Task<TransactionResponseDto> SetStatusAsync(string id, bool isActive, string currentUserId, bool isAdmin);
}

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repo;
    private readonly ICounterRepository _counters;
    private readonly IUserRepository _users;
    private readonly IExpenseItemRepository _expenseItems;
    private readonly IIncomeItemRepository _incomeItems;
    private readonly ICatalogRepository _catalogs;
    private readonly ICatalogService _catalogService;
    private readonly ICutRepository _cuts;

    public TransactionService(
        ITransactionRepository repo, ICounterRepository counters, IUserRepository users,
        IExpenseItemRepository expenseItems, IIncomeItemRepository incomeItems,
        ICatalogRepository catalogs, ICatalogService catalogService, ICutRepository cuts)
    {
        _repo = repo;
        _counters = counters;
        _users = users;
        _expenseItems = expenseItems;
        _incomeItems = incomeItems;
        _catalogs = catalogs;
        _catalogService = catalogService;
        _cuts = cuts;
    }

    // ------------------------------------------------------------ consultas

    public async Task<TransactionPageDto> SearchAsync(TransactionQuery q, string currentUserId, bool isAdmin)
    {
        var page = Math.Max(1, q.Page);
        var size = Math.Clamp(q.PageSize, 1, 100);

        var filter = new TransactionFilter
        {
            // Regla clave: un usuario normal SIEMPRE consulta solo lo suyo, aunque mande otro userId.
            UserId = isAdmin ? q.UserId : currentUserId,
            From = q.From?.Date is { } f ? AsUtc(f) : null,
            To = q.To?.Date is { } t ? AsUtc(t) : null,
            Type = q.Type,
            ExpenseTypeId = q.ExpenseTypeId,
            CategoryId = q.CategoryId,
            PaymentTypeId = q.PaymentTypeId,
            IsActive = q.IsActive
        };

        var (items, total) = await _repo.SearchAsync(filter, page, size);
        var (income, expense) = await _repo.TotalsAsync(filter);

        return new TransactionPageDto
        {
            Items = items.Select(ToDto).ToList(),
            Total = total, Page = page, PageSize = size,
            TotalIncome = income, TotalExpense = expense
        };
    }

    public async Task<TransactionResponseDto> GetByIdAsync(string id, string currentUserId, bool isAdmin) =>
        ToDto(await FindAuthorized(id, currentUserId, isAdmin));

    // ------------------------------------------------------------ escritura

    public async Task<TransactionResultDto> CreateAsync(CreateTransactionDto dto, string currentUserId, bool isAdmin)
    {
        // El usuario sale del token. Solo un admin puede registrar a nombre de otro.
        var targetId = currentUserId;
        if (!string.IsNullOrEmpty(dto.UserId) && dto.UserId != currentUserId)
        {
            if (!isAdmin) throw new AppException("Solo puedes registrar transacciones a tu nombre.", 403);
            targetId = dto.UserId;
        }

        var user = await _users.GetByIdAsync(targetId) ?? throw new AppException("Usuario no encontrado.", 404);
        if (!user.IsActive) throw new AppException("El usuario está inhabilitado.");

        var date = ValidateDate(dto.TransactionDate);
        await EnsureOpenPeriodAsync(user.Id, date);

        var t = new Transaction
        {
            Type = dto.Type,
            UserId = user.Id,
            UserName = user.Name,
            TransactionDate = date,
            Amount = dto.Amount,
            Comment = Clean(dto.Comment)
        };
        await ApplyConceptAndPayment(t, dto.ItemId, dto.PaymentTypeId, dto.CardNumber);
        t.Number = await _counters.NextAsync("transactions");

        await _repo.CreateAsync(t);
        return new TransactionResultDto { Transaction = ToDto(t), Warning = await CheckLimitAsync(user, t) };
    }

    public async Task<TransactionResultDto> UpdateAsync(string id, UpdateTransactionDto dto, string currentUserId, bool isAdmin)
    {
        var t = await FindAuthorized(id, currentUserId, isAdmin);
        if (!t.IsActive) throw new AppException("No puedes editar una transacción inhabilitada. Habilítala primero.");

        var newDate = ValidateDate(dto.TransactionDate);
        await EnsureOpenPeriodAsync(t.UserId, t.TransactionDate); // la fecha actual no puede estar en un período cortado
        await EnsureOpenPeriodAsync(t.UserId, newDate);           // ni la nueva

        t.TransactionDate = newDate;
        t.Amount = dto.Amount;
        t.Comment = Clean(dto.Comment);
        await ApplyConceptAndPayment(t, dto.ItemId, dto.PaymentTypeId, dto.CardNumber);

        await _repo.ReplaceAsync(t);
        var user = await _users.GetByIdAsync(t.UserId);
        return new TransactionResultDto { Transaction = ToDto(t), Warning = user is null ? null : await CheckLimitAsync(user, t) };
    }

    public async Task<TransactionResponseDto> SetStatusAsync(string id, bool isActive, string currentUserId, bool isAdmin)
    {
        var t = await FindAuthorized(id, currentUserId, isAdmin);
        await EnsureOpenPeriodAsync(t.UserId, t.TransactionDate);
        t.IsActive = isActive;
        await _repo.ReplaceAsync(t);
        return ToDto(t);
    }

    // ------------------------------------------------------------ reglas

    /// <summary>Valida concepto y tipo de pago, y copia los nombres (snapshot) dentro de la transacción.</summary>
    private async Task ApplyConceptAndPayment(Transaction t, string itemId, string paymentTypeId, string? cardNumber)
    {
        var names = (await _catalogs.GetByUserAsync(t.UserId)).ToDictionary(c => c.Id, c => c.Description);
        string Name(string? id) => id is not null && names.TryGetValue(id, out var n) ? n : "—";

        if (itemId != t.ItemId) // concepto nuevo o cambiado → se vuelve a copiar
        {
            if (t.Type == TransactionTypes.Expense)
            {
                var item = await _expenseItems.GetByIdAsync(itemId);
                if (item is null || item.UserId != t.UserId) throw new AppException("El egreso seleccionado no es válido.");
                if (!item.IsActive) throw new AppException("El egreso seleccionado está inhabilitado.");
                t.ItemId = item.Id;
                t.ItemDescription = item.Description;
                t.ExpenseTypeId = item.ExpenseTypeId;
                t.ExpenseTypeName = Name(item.ExpenseTypeId);
                t.CategoryId = item.CategoryId;
                t.CategoryName = Name(item.CategoryId);
            }
            else
            {
                var item = await _incomeItems.GetByIdAsync(itemId);
                if (item is null || item.UserId != t.UserId) throw new AppException("El ingreso seleccionado no es válido.");
                if (!item.IsActive) throw new AppException("El ingreso seleccionado está inhabilitado.");
                t.ItemId = item.Id;
                t.ItemDescription = item.Description;
                t.IncomeTypeName = Name(item.IncomeTypeId);
                t.Source = item.Source;
            }
        }

        var payment = await _catalogService.RequireAsync(t.UserId, paymentTypeId, CatalogTypes.PaymentType, "El tipo de pago", t.PaymentTypeId);
        t.PaymentTypeId = payment.Id;
        t.PaymentTypeName = payment.Description;

        if (!payment.RequiresCard)
        {
            t.CardLast4 = null;
            return;
        }

        var digits = CedulaRncValidator.Normalize(cardNumber);
        if (digits.Length == 0 && t.CardLast4 is not null) return; // edición: se conserva la tarjeta guardada
        if (digits.Length < 4 || digits.Length > 19)
            throw new AppException("Ingresa el número de tarjeta (mínimo los últimos 4 dígitos, máximo 19).");
        t.CardLast4 = digits[^4..];
    }

    /// <summary>Si es un egreso y el total del período supera el límite del usuario, devuelve una advertencia.</summary>
    private async Task<LimitWarningDto?> CheckLimitAsync(User user, Transaction t)
    {
        if (t.Type != TransactionTypes.Expense || !t.IsActive || user.ExpenseLimit <= 0) return null;

        var period = BillingPeriod.For(t.TransactionDate, user.CutDay);

        // Si el usuario cambió su día de corte, el período no puede empezar antes del último corte ya hecho.
        var start = period.Start;
        var lastCut = await _cuts.GetLastAsync(user.Id);
        if (lastCut is not null && lastCut.PeriodEnd.AddDays(1) > start) start = lastCut.PeriodEnd.AddDays(1);

        var spent = await _repo.SumAsync(user.Id, TransactionTypes.Expense, start, period.End);
        if (spent <= user.ExpenseLimit) return null;

        var inv = CultureInfo.InvariantCulture;
        return new LimitWarningDto
        {
            Message = $"{user.Name} superó el límite de egresos del período: RD$ {spent.ToString("N2", inv)} de RD$ {user.ExpenseLimit.ToString("N2", inv)}.",
            Limit = user.ExpenseLimit,
            Spent = spent,
            Percent = Math.Round(spent / user.ExpenseLimit * 100, 1),
            PeriodStart = start,
            PeriodEnd = period.End
        };
    }

    /// <summary>Un período ya cortado queda congelado: no se crean, editan ni inhabilitan transacciones en él.</summary>
    private async Task EnsureOpenPeriodAsync(string userId, DateTime date)
    {
        var last = await _cuts.GetLastAsync(userId);
        if (last is not null && date.Date <= last.PeriodEnd.Date)
            throw new AppException(
                $"El período hasta el {last.PeriodEnd:dd/MM/yyyy} ya fue cortado. No se pueden registrar ni modificar transacciones con fecha en ese período.");
    }

    private static DateTime ValidateDate(DateTime date)
    {
        var d = date.Date;
        if (d.Year < 2000)
            throw new AppException("La fecha de la transacción no es válida.");
        if (d > DateTime.UtcNow.Date.AddDays(1))
            throw new AppException("La fecha de la transacción no puede ser futura.");
        return AsUtc(d);
    }

    private static DateTime AsUtc(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>Busca la transacción; si no es del usuario (y no es admin) responde 404 para no revelar que existe.</summary>
    private async Task<Transaction> FindAuthorized(string id, string currentUserId, bool isAdmin)
    {
        var t = await _repo.GetByIdAsync(id);
        if (t is null || (!isAdmin && t.UserId != currentUserId))
            throw new AppException("Transacción no encontrada.", 404);
        return t;
    }

    internal static TransactionResponseDto ToDto(Transaction t) => new()
    {
        Id = t.Id, Number = t.Number, Type = t.Type, UserId = t.UserId, UserName = t.UserName,
        ItemId = t.ItemId, ItemDescription = t.ItemDescription,
        ExpenseTypeId = t.ExpenseTypeId, ExpenseTypeName = t.ExpenseTypeName,
        CategoryId = t.CategoryId, CategoryName = t.CategoryName,
        IncomeTypeName = t.IncomeTypeName, Source = t.Source,
        PaymentTypeId = t.PaymentTypeId, PaymentTypeName = t.PaymentTypeName,
        TransactionDate = t.TransactionDate, RegisteredAt = t.RegisteredAt,
        Amount = t.Amount, CardLast4 = t.CardLast4, Comment = t.Comment, IsActive = t.IsActive
    };
}
