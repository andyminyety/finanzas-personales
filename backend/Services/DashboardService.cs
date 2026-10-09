using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Middleware;
using FinanzasApi.Models;
using FinanzasApi.Repositories;

namespace FinanzasApi.Services;

public interface IDashboardService
{
    /// <param name="userId">null = el usuario actual · "all" = todos (solo admin) · un id = ese usuario (solo admin)</param>
    Task<DashboardDto> GetAsync(string? userId, string currentUserId, bool isAdmin);
}

public class DashboardService : IDashboardService
{
    private const int EvolutionPeriods = 6;

    private readonly ITransactionRepository _tx;
    private readonly IUserRepository _users;
    private readonly ICutRepository _cuts;

    public DashboardService(ITransactionRepository tx, IUserRepository users, ICutRepository cuts)
    {
        _tx = tx;
        _users = users;
        _cuts = cuts;
    }

    public async Task<DashboardDto> GetAsync(string? userId, string currentUserId, bool isAdmin)
    {
        var all = userId == "all";
        if (all && !isAdmin)
            throw new AppException("Solo un administrador puede ver el resumen de todos los usuarios.", 403);

        User? user = null;
        if (!all)
        {
            // Un usuario normal siempre ve lo suyo, aunque mande otro userId.
            var id = isAdmin && !string.IsNullOrEmpty(userId) ? userId : currentUserId;
            user = await _users.GetByIdAsync(id) ?? throw new AppException("Usuario no encontrado.", 404);
        }

        var periods = BuildPeriods(AppClock.Today, user?.CutDay);
        var current = periods[^1];

        // Si el usuario cambió su día de corte, el período actual no puede empezar antes del último corte hecho.
        if (user is not null)
        {
            var lastCut = await _cuts.GetLastAsync(user.Id);
            if (lastCut is not null && lastCut.PeriodEnd.AddDays(1) > current.Start)
            {
                current = current with { Start = lastCut.PeriodEnd.AddDays(1) };
                periods[^1] = current;
            }
        }

        // 1) Balance actual: todo lo histórico (los cortes se calculan con estos mismos datos).
        var (totalIncome, totalExpense) = await _tx.TotalsAsync(new TransactionFilter { UserId = user?.Id });

        // 2) Una sola consulta con los últimos 6 períodos; el resto se arma en memoria.
        var txs = await _tx.ListAsync(new TransactionFilter
        {
            UserId = user?.Id, From = periods[0].Start, To = current.End, IsActive = true
        });
        var inCurrent = txs.Where(t => t.TransactionDate >= current.Start && t.TransactionDate <= current.End).ToList();

        var periodIncome = inCurrent.Where(t => t.Type == TransactionTypes.Income).Sum(t => t.Amount);
        var periodExpense = inCurrent.Where(t => t.Type == TransactionTypes.Expense).Sum(t => t.Amount);

        // 3) Últimas transacciones (de cualquier fecha).
        var (last, _) = await _tx.SearchAsync(new TransactionFilter { UserId = user?.Id, IsActive = true }, 1, 8);

        var dto = new DashboardDto
        {
            Scope = user?.Name ?? "Todos los usuarios",
            IsAllUsers = all,
            PeriodStart = current.Start,
            PeriodEnd = current.End,
            CurrentBalance = totalIncome - totalExpense,
            PeriodIncome = periodIncome,
            PeriodExpense = periodExpense,
            ExpensesByCategory = Group(inCurrent.Where(t => t.Type == TransactionTypes.Expense), t => t.CategoryName),
            IncomesByType = Group(inCurrent.Where(t => t.Type == TransactionTypes.Income), t => t.IncomeTypeName),
            Evolution = periods.Select(p => new EvolutionPointDto
            {
                Year = p.Year,
                Month = p.Month,
                Income = txs.Where(t => t.Type == TransactionTypes.Income && t.TransactionDate >= p.Start && t.TransactionDate <= p.End).Sum(t => t.Amount),
                Expense = txs.Where(t => t.Type == TransactionTypes.Expense && t.TransactionDate >= p.Start && t.TransactionDate <= p.End).Sum(t => t.Amount)
            }).ToList(),
            LastTransactions = last.Select(TransactionService.ToDto).ToList()
        };

        if (user is not null && user.ExpenseLimit > 0)
        {
            dto.HasLimit = true;
            dto.ExpenseLimit = user.ExpenseLimit;
            dto.LimitUsedPercent = Math.Round(periodExpense / user.ExpenseLimit * 100, 1);
        }
        return dto;
    }

    /// <summary>
    /// Los últimos 6 períodos (el actual al final). Para un usuario, períodos de su día de corte;
    /// para "todos", meses calendario (cada usuario tiene su propio corte, no se pueden mezclar).
    /// </summary>
    private static List<BillingPeriod> BuildPeriods(DateTime today, int? cutDay)
    {
        var list = new List<BillingPeriod>();
        if (cutDay is int c)
        {
            var p = BillingPeriod.For(today, c);
            list.Add(p);
            for (var i = 1; i < EvolutionPeriods; i++)
            {
                p = BillingPeriod.For(p.Start.AddDays(-1), c);
                list.Insert(0, p);
            }
        }
        else
        {
            for (var i = EvolutionPeriods - 1; i >= 0; i--)
            {
                var start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
                var end = start.AddMonths(1).AddDays(-1);
                list.Add(new BillingPeriod(start, end, start.Year, start.Month));
            }
        }
        return list;
    }

    private static List<NameAmountDto> Group(IEnumerable<Transaction> txs, Func<Transaction, string?> key)
    {
        var groups = txs
            .GroupBy(t => key(t) ?? "Sin clasificar")
            .Select(g => new { Name = g.Key, Amount = g.Sum(t => t.Amount) })
            .OrderByDescending(g => g.Amount)
            .ToList();
        var total = groups.Sum(g => g.Amount);
        return groups.Select(g => new NameAmountDto
        {
            Name = g.Name,
            Amount = g.Amount,
            Percent = total == 0 ? 0 : Math.Round(g.Amount / total * 100, 1)
        }).ToList();
    }
}
