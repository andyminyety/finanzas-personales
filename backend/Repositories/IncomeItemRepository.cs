using FinanzasApi.Config;
using FinanzasApi.Models;
using MongoDB.Driver;

namespace FinanzasApi.Repositories;

public class IncomeItemRepository : IIncomeItemRepository
{
    private readonly IMongoCollection<IncomeItem> _col;
    public IncomeItemRepository(MongoContext ctx) => _col = ctx.Database.GetCollection<IncomeItem>("income_items");

    public Task<List<IncomeItem>> GetAllAsync(string userId, bool activeOnly) =>
        _col.Find(i => i.UserId == userId && (!activeOnly || i.IsActive)).SortBy(i => i.Description).ToListAsync();

    public async Task<IncomeItem?> GetByIdAsync(string id) =>
        await _col.Find(i => i.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(IncomeItem item) => _col.InsertOneAsync(item);
    public Task ReplaceAsync(IncomeItem item) => _col.ReplaceOneAsync(i => i.Id == item.Id, item);
}
