using FinanzasApi.Config;
using FinanzasApi.Models;
using MongoDB.Driver;

namespace FinanzasApi.Repositories;

public class ExpenseItemRepository : IExpenseItemRepository
{
    private readonly IMongoCollection<ExpenseItem> _col;
    public ExpenseItemRepository(MongoContext ctx) => _col = ctx.Database.GetCollection<ExpenseItem>("expense_items");

    public Task<List<ExpenseItem>> GetAllAsync(string userId, bool activeOnly) =>
        _col.Find(i => i.UserId == userId && (!activeOnly || i.IsActive)).SortBy(i => i.Description).ToListAsync();

    public async Task<ExpenseItem?> GetByIdAsync(string id) =>
        await _col.Find(i => i.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(ExpenseItem item) => _col.InsertOneAsync(item);
    public Task ReplaceAsync(ExpenseItem item) => _col.ReplaceOneAsync(i => i.Id == item.Id, item);
}
