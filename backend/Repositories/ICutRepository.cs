using FinanzasApi.Models;

namespace FinanzasApi.Repositories;

public interface ICutRepository
{
    Task EnsureIndexesAsync();
    /// <summary>Último corte del usuario (el de período más reciente), o null si no tiene.</summary>
    Task<MonthlyCut?> GetLastAsync(string userId);
    Task<List<MonthlyCut>> SearchAsync(string? userId, int? year);
    Task CreateAsync(MonthlyCut cut);
}
