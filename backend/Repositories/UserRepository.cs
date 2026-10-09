using FinanzasApi.Config;
using FinanzasApi.Helpers;
using FinanzasApi.Models;
using MongoDB.Driver;

namespace FinanzasApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoContext ctx) => _users = ctx.Database.GetCollection<User>("users");

    public async Task EnsureIndexesAsync()
    {
        var keys = Builders<User>.IndexKeys;
        await _users.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<User>(keys.Ascending(u => u.CedulaRnc), new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<User>(keys.Ascending(u => u.Email), new CreateIndexOptions { Unique = true }),
        });
    }

    public Task<List<User>> GetAllAsync() =>
        _users.Find(_ => true).SortBy(u => u.Name).ToListAsync();

    public async Task<User?> GetByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByLoginAsync(string identifier)
    {
        identifier = identifier.Trim();
        if (identifier.Contains('@'))
            return await GetByEmailAsync(identifier);
        return await GetByCedulaAsync(identifier);
    }

    public async Task<User?> GetByCedulaAsync(string cedulaRnc)
    {
        var normalized = CedulaRncValidator.Normalize(cedulaRnc);
        return await _users.Find(u => u.CedulaRnc == normalized).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _users.Find(u => u.Email == normalized).FirstOrDefaultAsync();
    }

    public async Task<bool> AnyAsync() => await _users.Find(_ => true).Limit(1).AnyAsync();

    public Task CreateAsync(User user) => _users.InsertOneAsync(user);

    public Task ReplaceAsync(User user) => _users.ReplaceOneAsync(u => u.Id == user.Id, user);
}
