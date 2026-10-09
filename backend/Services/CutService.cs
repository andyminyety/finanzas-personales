using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Middleware;
using FinanzasApi.Models;
using FinanzasApi.Repositories;
using MongoDB.Driver;

namespace FinanzasApi.Services;

public interface ICutService
{
    Task<List<CutResponseDto>> SearchAsync(CutQuery query, string currentUserId, bool isAdmin);
    Task<RunCutsResultDto> RunManualAsync(RunCutsDto dto, string currentUserId, bool isAdmin);
    /// <summary>Lo llama el proceso diario: corta a todos los usuarios activos. Devuelve cuántos cortes creó.</summary>
    Task<int> RunAutomaticAsync();
}

public class CutService : ICutService
{
    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly ICutRepository _cuts;
    private readonly IUserRepository _users;
    private readonly ITransactionRepository _transactions;
    private readonly ILogger<CutService> _logger;

    public CutService(ICutRepository cuts, IUserRepository users, ITransactionRepository transactions, ILogger<CutService> logger)
    {
        _cuts = cuts;
        _users = users;
        _transactions = transactions;
        _logger = logger;
    }

    public async Task<List<CutResponseDto>> SearchAsync(CutQuery q, string currentUserId, bool isAdmin) =>
        (await _cuts.SearchAsync(isAdmin ? q.UserId : currentUserId, q.Year)).Select(ToDto).ToList();

    public async Task<RunCutsResultDto> RunManualAsync(RunCutsDto dto, string currentUserId, bool isAdmin)
    {
        List<User> targets;
        if (!isAdmin)
        {
            if (!string.IsNullOrEmpty(dto.UserId) && dto.UserId != currentUserId)
                throw new AppException("Solo puedes generar tu propio corte.", 403);
            targets = new() { await _users.GetByIdAsync(currentUserId) ?? throw new AppException("Usuario no encontrado.", 404) };
        }
        else if (!string.IsNullOrEmpty(dto.UserId))
        {
            targets = new() { await _users.GetByIdAsync(dto.UserId) ?? throw new AppException("Usuario no encontrado.", 404) };
        }
        else
        {
            targets = (await _users.GetAllAsync()).Where(u => u.IsActive).ToList();
        }

        var created = new List<MonthlyCut>();
        foreach (var u in targets.Where(u => u.IsActive))
            created.AddRange(await CatchUpAsync(u, "Manual"));

        return new RunCutsResultDto
        {
            Created = created.Select(ToDto).ToList(),
            UsersProcessed = targets.Count,
            Message = created.Count == 0
                ? "Todo al día: no hay períodos terminados pendientes de cortar."
                : $"Se generó {created.Count} corte{(created.Count == 1 ? "" : "s")}."
        };
    }

    public async Task<int> RunAutomaticAsync()
    {
        var total = 0;
        foreach (var u in (await _users.GetAllAsync()).Where(u => u.IsActive))
        {
            try
            {
                total += (await CatchUpAsync(u, "Auto")).Count;
            }
            catch (Exception ex)
            {
                // Un usuario con problemas no debe impedir el corte de los demás.
                _logger.LogError(ex, "Falló el corte automático del usuario {UserId}", u.Id);
            }
        }
        return total;
    }

    /// <summary>
    /// Genera todos los cortes pendientes de un usuario, en orden, hasta el último período YA TERMINADO
    /// (el que termina ayer o antes). Es idempotente: si no hay nada pendiente, no hace nada.
    /// El primer corte de un usuario incluye todas sus transacciones anteriores (balance inicial 0).
    /// </summary>
    private async Task<List<MonthlyCut>> CatchUpAsync(User user, string source)
    {
        var created = new List<MonthlyCut>();
        var cutoff = AppClock.Today.AddDays(-1);

        for (var i = 0; i < 36; i++) // tope de seguridad
        {
            var last = await _cuts.GetLastAsync(user.Id);
            var first = last is null;
            DateTime start, end;

            if (first)
            {
                var p = BillingPeriod.For(cutoff, user.CutDay);
                if (p.End > cutoff) p = BillingPeriod.For(p.Start.AddDays(-1), user.CutDay);
                (start, end) = (p.Start, p.End);
                if (user.CreatedAt.Date > end) break; // usuario creado después: aún no hay nada que cortar
            }
            else
            {
                start = last!.PeriodEnd.AddDays(1);
                end = BillingPeriod.For(start, user.CutDay).End;
                if (end > cutoff) break; // ese período todavía no termina
            }

            var from = first ? Epoch : start;
            var income = await _transactions.SumAsync(user.Id, TransactionTypes.Income, from, end);
            var expense = await _transactions.SumAsync(user.Id, TransactionTypes.Expense, from, end);
            var initial = last?.FinalBalance ?? 0m;

            var cut = new MonthlyCut
            {
                UserId = user.Id,
                UserName = user.Name,
                Year = end.Year,
                Month = end.Month,
                CutDate = end,
                PeriodStart = start,
                PeriodEnd = end,
                InitialBalance = initial,
                TotalIncome = income,
                TotalExpense = expense,
                FinalBalance = initial + income - expense,
                Source = source
            };

            try
            {
                await _cuts.CreateAsync(cut);
                created.Add(cut);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                break; // otro proceso (auto/manual) ya lo creó: el índice único evita duplicados
            }
        }
        return created;
    }

    private static CutResponseDto ToDto(MonthlyCut c) => new()
    {
        Id = c.Id, UserId = c.UserId, UserName = c.UserName, Year = c.Year, Month = c.Month,
        CutDate = c.CutDate, PeriodStart = c.PeriodStart, PeriodEnd = c.PeriodEnd,
        InitialBalance = c.InitialBalance, TotalIncome = c.TotalIncome, TotalExpense = c.TotalExpense,
        FinalBalance = c.FinalBalance, Source = c.Source
    };
}
