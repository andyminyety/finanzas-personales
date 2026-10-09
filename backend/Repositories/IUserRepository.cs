using FinanzasApi.Models;

namespace FinanzasApi.Repositories;

public interface IUserRepository
{
    Task EnsureIndexesAsync();
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByLoginAsync(string identifier);
    Task<User?> GetByCedulaAsync(string cedulaRnc);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> AnyAsync();
    Task CreateAsync(User user);
    Task ReplaceAsync(User user);
}
