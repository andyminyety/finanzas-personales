using FinanzasApi.Config;
using FinanzasApi.Models;
using MongoDB.Driver;

namespace FinanzasApi.Repositories;

public class CounterRepository : ICounterRepository
{
    private readonly IMongoCollection<Counter> _col;
    public CounterRepository(MongoContext ctx) => _col = ctx.Database.GetCollection<Counter>("counters");

    // Atomically increments and returns the next number.
    public async Task<long> NextAsync(string name)
    {
        var counter = await _col.FindOneAndUpdateAsync<Counter>(
            c => c.Id == name,
            Builders<Counter>.Update.Inc(c => c.Seq, 1),
            new FindOneAndUpdateOptions<Counter> { IsUpsert = true, ReturnDocument = ReturnDocument.After });
        return counter.Seq;
    }
}
