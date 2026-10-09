using FinanzasApi.Config;
using FinanzasApi.Models;
using MongoDB.Driver;

namespace FinanzasApi.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IMongoCollection<Transaction> _col;
    public TransactionRepository(MongoContext ctx) => _col = ctx.Database.GetCollection<Transaction>("transactions");

    public async Task EnsureIndexesAsync()
    {
        var k = Builders<Transaction>.IndexKeys;
        await _col.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<Transaction>(k.Ascending(t => t.UserId).Descending(t => t.TransactionDate)),
            new CreateIndexModel<Transaction>(k.Ascending(t => t.Number), new CreateIndexOptions { Unique = true }),
        });
    }

    public async Task<(List<Transaction> Items, long Total)> SearchAsync(TransactionFilter filter, int page, int pageSize)
    {
        var f = Build(filter);
        var total = await _col.CountDocumentsAsync(f);
        var items = await _col.Find(f)
            .SortByDescending(t => t.TransactionDate).ThenByDescending(t => t.Number)
            .Skip((page - 1) * pageSize).Limit(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public Task<List<Transaction>> ListAsync(TransactionFilter filter) =>
        _col.Find(Build(filter)).SortByDescending(t => t.TransactionDate).ThenByDescending(t => t.Number).ToListAsync();

    public async Task<(decimal Income, decimal Expense)> TotalsAsync(TransactionFilter filter)
    {
        var f = Builders<Transaction>.Filter.And(Build(filter), Builders<Transaction>.Filter.Eq(t => t.IsActive, true));
        var rows = await _col.Find(f).Project(t => new { t.Type, t.Amount }).ToListAsync();
        return (
            rows.Where(r => r.Type == TransactionTypes.Income).Sum(r => r.Amount),
            rows.Where(r => r.Type == TransactionTypes.Expense).Sum(r => r.Amount));
    }

    public async Task<Transaction?> GetByIdAsync(string id) =>
        await _col.Find(t => t.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(Transaction t) => _col.InsertOneAsync(t);
    public Task ReplaceAsync(Transaction t) => _col.ReplaceOneAsync(x => x.Id == t.Id, t);

    public async Task<decimal> SumAsync(string userId, string type, DateTime from, DateTime to)
    {
        var amounts = await _col
            .Find(t => t.UserId == userId && t.Type == type && t.IsActive &&
                       t.TransactionDate >= from && t.TransactionDate <= to)
            .Project(t => t.Amount)
            .ToListAsync();
        return amounts.Sum();
    }

    private static FilterDefinition<Transaction> Build(TransactionFilter f)
    {
        var b = Builders<Transaction>.Filter;
        var list = new List<FilterDefinition<Transaction>> { b.Empty };

        if (!string.IsNullOrEmpty(f.UserId)) list.Add(b.Eq(t => t.UserId, f.UserId));
        if (f.From.HasValue) list.Add(b.Gte(t => t.TransactionDate, f.From.Value));
        if (f.To.HasValue) list.Add(b.Lte(t => t.TransactionDate, f.To.Value));
        if (!string.IsNullOrEmpty(f.Type)) list.Add(b.Eq(t => t.Type, f.Type));
        if (!string.IsNullOrEmpty(f.ExpenseTypeId)) list.Add(b.Eq(t => t.ExpenseTypeId, f.ExpenseTypeId));
        if (!string.IsNullOrEmpty(f.CategoryId)) list.Add(b.Eq(t => t.CategoryId, f.CategoryId));
        if (!string.IsNullOrEmpty(f.PaymentTypeId)) list.Add(b.Eq(t => t.PaymentTypeId, f.PaymentTypeId));
        if (f.IsActive.HasValue) list.Add(b.Eq(t => t.IsActive, f.IsActive.Value));

        return b.And(list);
    }
}
