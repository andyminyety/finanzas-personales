using FinanzasApi.Models;

namespace FinanzasApi.Repositories;

public interface IIncomeItemRepository
{
    Task<List<IncomeItem>> GetAllAsync(string userId, bool activeOnly);
    Task<IncomeItem?> GetByIdAsync(string id);
    Task CreateAsync(IncomeItem item);
    Task ReplaceAsync(IncomeItem item);
}
