using FinanzasApi.Models;

namespace FinanzasApi.Repositories;

public interface ICatalogRepository
{
    Task EnsureIndexesAsync();
    Task<List<CatalogItem>> GetByTypeAsync(string userId, string type, bool activeOnly);
    // All of a user's catalogs (used to resolve names).
    Task<List<CatalogItem>> GetByUserAsync(string userId);
    Task<CatalogItem?> GetByIdAsync(string id);
    Task<bool> AnyForUserAsync(string userId);
    Task CreateAsync(CatalogItem item);
    Task CreateManyAsync(IEnumerable<CatalogItem> items);
    Task ReplaceAsync(CatalogItem item);
}
