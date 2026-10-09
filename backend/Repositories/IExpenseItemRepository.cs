using FinanzasApi.Models;

namespace FinanzasApi.Repositories;

public interface IExpenseItemRepository
{
    Task<List<ExpenseItem>> GetAllAsync(string userId, bool activeOnly);
    Task<ExpenseItem?> GetByIdAsync(string id);
    Task CreateAsync(ExpenseItem item);
    Task ReplaceAsync(ExpenseItem item);
}
