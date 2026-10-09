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
    // Called by the background process: cuts every active user. Returns how many cuts were created.
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
                // One failing user must not prevent the others from being cut.
                _logger.LogError(ex, "Falló el corte automático del usuario {UserId}", u.Id);
            }
        }
        return total;
    }

    // Generates all of a user's pending cuts, in order, up to the last period that has ALREADY ENDED
    // (the one ending yesterday or earlier). Idempotent: if nothing is pending, it does nothing.
    // A user's first cut includes all earlier transactions (initial balance 0).
    private async Task<List<MonthlyCut>> CatchUpAsync(User user, string source)
    {
        var created = new List<MonthlyCut>();
        var cutoff = AppClock.Today.AddDays(-1);

        for (var i = 0; i < 36; i++) // safety limit
        {
            var last = await _cuts.GetLastAsync(user.Id);
            var first = last is null;
            DateTime start, end;

            if (first)
            {
                var p = BillingPeriod.For(cutoff, user.CutDay);
                if (p.End > cutoff) p = BillingPeriod.For(p.Start.AddDays(-1), user.CutDay);
                (start, end) = (p.Start, p.End);
                if (user.CreatedAt.Date > end) break; // user created later: nothing to cut yet
            }
            else
            {
                start = last!.PeriodEnd.AddDays(1);
                end = BillingPeriod.For(start, user.CutDay).End;
                if (end > cutoff) break; // that period has not ended yet
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
                break; // another process (auto/manual) already created it: the unique index prevents duplicates
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
