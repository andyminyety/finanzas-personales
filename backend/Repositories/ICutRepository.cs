using FinanzasApi.Models;

namespace FinanzasApi.Repositories;

public interface ICutRepository
{
    Task EnsureIndexesAsync();
    // The user's latest cut (most recent period), or null if there is none.
    Task<MonthlyCut?> GetLastAsync(string userId);
    Task<List<MonthlyCut>> SearchAsync(string? userId, int? year);
    Task CreateAsync(MonthlyCut cut);
}
