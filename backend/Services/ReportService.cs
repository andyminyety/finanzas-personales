using FinanzasApi.DTOs;
using FinanzasApi.Middleware;
using FinanzasApi.Models;
using FinanzasApi.Repositories;

namespace FinanzasApi.Services;

public interface IReportService
{
    Task<ReportDto> GetAsync(string kind, ReportQuery query, string currentUserId, bool isAdmin);
}

public class ReportService : IReportService
{
    private static readonly string[] MonthNames =
        { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

    private readonly ITransactionRepository _tx;
    private readonly IUserRepository _users;

    public ReportService(ITransactionRepository tx, IUserRepository users)
    {
        _tx = tx;
        _users = users;
    }

    public async Task<ReportDto> GetAsync(string kind, ReportQuery q, string currentUserId, bool isAdmin)
    {
        if (kind is not ("range" or "by-user" or "by-type" or "by-category" or "monthly"))
            throw new AppException("Reporte no encontrado.", 404);

        // Regla clave: un usuario normal SIEMPRE obtiene solo sus datos, aunque mande otro userId.
        var userId = isAdmin ? (string.IsNullOrEmpty(q.UserId) ? null : q.UserId) : currentUserId;

        var filter = new TransactionFilter { UserId = userId, IsActive = true };
        int? year = null;
        if (kind == "monthly")
        {
            year = q.Year ?? DateTime.UtcNow.Year;
            filter.From = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            filter.To = new DateTime(year.Value, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        }
        else
        {
            if (q.From.HasValue) filter.From = DateTime.SpecifyKind(q.From.Value.Date, DateTimeKind.Utc);
            if (q.To.HasValue) filter.To = DateTime.SpecifyKind(q.To.Value.Date, DateTimeKind.Utc);
        }

        string scope = "Todos los usuarios";
        if (userId is not null)
            scope = (await _users.GetByIdAsync(userId))?.Name ?? throw new AppException("Usuario no encontrado.", 404);

        var txs = await _tx.ListAsync(filter);
        var income = txs.Where(t => t.Type == TransactionTypes.Income).Sum(t => t.Amount);
        var expense = txs.Where(t => t.Type == TransactionTypes.Expense).Sum(t => t.Amount);

        var report = new ReportDto
        {
            Kind = kind,
            Scope = scope,
            From = filter.From,
            To = filter.To,
            Year = year,
            Totals = new ReportTotalsDto { Income = income, Expense = expense, Balance = income - expense, Count = txs.Count }
        };

        switch (kind)
        {
            case "range":
                report.Title = "Reporte por rango de fechas";
                report.Transactions = txs.Select(TransactionService.ToDto).ToList();
                break;

            case "by-user":
                report.Title = "Reporte por usuario";
                report.Rows = txs.GroupBy(t => t.UserName)
                    .Select(g => Row(g.Key, g.ToList()))
                    .OrderByDescending(r => r.Expense).ThenBy(r => r.Label).ToList();
                break;

            case "by-type":
                report.Title = "Reporte por tipo de transacción";
                report.Rows = new List<ReportRowDto>
                {
                    Row("Ingresos", txs.Where(t => t.Type == TransactionTypes.Income).ToList()),
                    Row("Egresos", txs.Where(t => t.Type == TransactionTypes.Expense).ToList())
                };
                break;

            case "by-category":
                report.Title = "Reporte por categoría de egreso";
                report.Rows = txs.Where(t => t.Type == TransactionTypes.Expense)
                    .GroupBy(t => t.CategoryName ?? "Sin clasificar")
                    .Select(g => Row(g.Key, g.ToList()))
                    .OrderByDescending(r => r.Expense).ToList();
                foreach (var r in report.Rows)
                    r.Percent = expense == 0 ? 0 : Math.Round(r.Expense / expense * 100, 1);
                break;

            case "monthly":
                report.Title = $"Resumen mensual {year}";
                report.Rows = Enumerable.Range(1, 12).Select(m =>
                    Row(MonthNames[m - 1], txs.Where(t => t.TransactionDate.Month == m).ToList())).ToList();
                break;
        }
        return report;
    }

    private static ReportRowDto Row(string label, List<Transaction> txs)
    {
        var income = txs.Where(t => t.Type == TransactionTypes.Income).Sum(t => t.Amount);
        var expense = txs.Where(t => t.Type == TransactionTypes.Expense).Sum(t => t.Amount);
        return new ReportRowDto { Label = label, Income = income, Expense = expense, Balance = income - expense, Count = txs.Count };
    }
}
