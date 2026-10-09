using FinanzasApi.Config;
using FinanzasApi.Models;
using MongoDB.Driver;

namespace FinanzasApi.Repositories;

public class CutRepository : ICutRepository
{
    private readonly IMongoCollection<MonthlyCut> _col;
    public CutRepository(MongoContext ctx) => _col = ctx.Database.GetCollection<MonthlyCut>("monthly_cuts");

    public Task EnsureIndexesAsync() =>
        _col.Indexes.CreateOneAsync(new CreateIndexModel<MonthlyCut>(
            Builders<MonthlyCut>.IndexKeys.Ascending(c => c.UserId).Ascending(c => c.Year).Ascending(c => c.Month),
            new CreateIndexOptions { Unique = true }));

    public async Task<MonthlyCut?> GetLastAsync(string userId) =>
        await _col.Find(c => c.UserId == userId).SortByDescending(c => c.PeriodEnd).FirstOrDefaultAsync();

    public Task<List<MonthlyCut>> SearchAsync(string? userId, int? year)
    {
        var b = Builders<MonthlyCut>.Filter;
        var filter = b.And(
            string.IsNullOrEmpty(userId) ? b.Empty : b.Eq(c => c.UserId, userId),
            year.HasValue ? b.Eq(c => c.Year, year.Value) : b.Empty);
        return _col.Find(filter).SortByDescending(c => c.CutDate).ThenBy(c => c.UserName).ToListAsync();
    }

    public Task CreateAsync(MonthlyCut cut) => _col.InsertOneAsync(cut);
}
