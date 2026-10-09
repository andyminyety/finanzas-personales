using FinanzasApi.Models;

namespace FinanzasApi.Repositories;

public class TransactionFilter
{
    public string? UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Type { get; set; }
    public string? ExpenseTypeId { get; set; }
    public string? CategoryId { get; set; }
    public string? PaymentTypeId { get; set; }
    public bool? IsActive { get; set; }
}

public interface ITransactionRepository
{
    Task EnsureIndexesAsync();
    Task<(List<Transaction> Items, long Total)> SearchAsync(TransactionFilter filter, int page, int pageSize);
    // All transactions matching the filter (unpaged), newest first.
    Task<List<Transaction>> ListAsync(TransactionFilter filter);
    Task<(decimal Income, decimal Expense)> TotalsAsync(TransactionFilter filter);
    Task<Transaction?> GetByIdAsync(string id);
    Task CreateAsync(Transaction t);
    Task ReplaceAsync(Transaction t);
    // Sum of a user's active transactions by type and date range (inclusive).
    Task<decimal> SumAsync(string userId, string type, DateTime from, DateTime to);
}
