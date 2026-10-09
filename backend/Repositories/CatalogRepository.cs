using FinanzasApi.Config;
using FinanzasApi.Models;
using MongoDB.Driver;

namespace FinanzasApi.Repositories;

public class CatalogRepository : ICatalogRepository
{
    private readonly IMongoCollection<CatalogItem> _col;

    public CatalogRepository(MongoContext ctx) => _col = ctx.Database.GetCollection<CatalogItem>("catalogs");

    public Task EnsureIndexesAsync() =>
        _col.Indexes.CreateOneAsync(new CreateIndexModel<CatalogItem>(
            Builders<CatalogItem>.IndexKeys.Ascending(c => c.UserId).Ascending(c => c.Type).Ascending(c => c.Description)));

    public Task<List<CatalogItem>> GetByTypeAsync(string userId, string type, bool activeOnly) =>
        _col.Find(c => c.UserId == userId && c.Type == type && (!activeOnly || c.IsActive))
            .SortBy(c => c.Description).ToListAsync();

    public Task<List<CatalogItem>> GetByUserAsync(string userId) =>
        _col.Find(c => c.UserId == userId).ToListAsync();

    public async Task<CatalogItem?> GetByIdAsync(string id) =>
        await _col.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task<bool> AnyForUserAsync(string userId) =>
        await _col.Find(c => c.UserId == userId).Limit(1).AnyAsync();

    public Task CreateAsync(CatalogItem item) => _col.InsertOneAsync(item);
    public Task CreateManyAsync(IEnumerable<CatalogItem> items) => _col.InsertManyAsync(items);
    public Task ReplaceAsync(CatalogItem item) => _col.ReplaceOneAsync(c => c.Id == item.Id, item);
}
