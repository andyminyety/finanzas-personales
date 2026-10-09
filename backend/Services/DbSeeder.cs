using FinanzasApi.Models;
using FinanzasApi.Repositories;

namespace FinanzasApi.Services;

/// <summary>Crea índices, el primer administrador y los catálogos iniciales de cada usuario que aún no tenga.</summary>
public static class DbSeeder
{
    public static async Task RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var catalogs = scope.ServiceProvider.GetRequiredService<ICatalogRepository>();
        var transactions = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var cuts = scope.ServiceProvider.GetRequiredService<ICutRepository>();
        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await users.EnsureIndexesAsync();
        await catalogs.EnsureIndexesAsync();
        await transactions.EnsureIndexesAsync();
        await cuts.EnsureIndexesAsync();

        if (!await users.AnyAsync())
        {
            await users.CreateAsync(new User
            {
                Name = cfg["Seed:AdminName"] ?? "Administrador",
                CedulaRnc = "00000000000",
                Email = (cfg["Seed:AdminEmail"] ?? "admin@finanzas.local").ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(cfg["Seed:AdminPassword"] ?? "Admin123!"),
                Role = Roles.Admin,
                PersonType = PersonTypes.Fisica,
                ExpenseLimit = 0,
                CutDay = 1
            });
        }

        // Cada usuario tiene sus propios catálogos: se crean los de arranque para quien no tenga ninguno.
        foreach (var u in await users.GetAllAsync())
        {
            if (!await catalogs.AnyForUserAsync(u.Id))
                await catalogs.CreateManyAsync(CatalogSeeder.StarterFor(u.Id));
        }

        if (cfg.GetValue("Seed:DemoData", false))
            await DemoDataSeeder.RunAsync(scope.ServiceProvider);
    }
}
