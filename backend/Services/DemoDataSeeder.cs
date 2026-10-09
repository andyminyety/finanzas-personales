using FinanzasApi.Helpers;
using FinanzasApi.Models;
using FinanzasApi.Repositories;

namespace FinanzasApi.Services;

// Sample data for development and testing (only when Seed:DemoData=true). Creates a user with
// concepts and transactions from the last 40 days. Idempotent: if the user already exists, it does nothing.
// Credentials: maria@demo.local / Demo123!
public static class DemoDataSeeder
{
    public static async Task RunAsync(IServiceProvider sp)
    {
        var users = sp.GetRequiredService<IUserRepository>();
        var catalogs = sp.GetRequiredService<ICatalogRepository>();
        var expenseItems = sp.GetRequiredService<IExpenseItemRepository>();
        var incomeItems = sp.GetRequiredService<IIncomeItemRepository>();
        var transactions = sp.GetRequiredService<ITransactionRepository>();
        var counters = sp.GetRequiredService<ICounterRepository>();

        const string email = "maria@demo.local";
        // María may have changed her email from the profile, so the cédula (unique) is checked too.
        const string cedula = "00100000017";
        if (await users.GetByEmailAsync(email) is not null || await users.GetByCedulaAsync(cedula) is not null) return;

        var user = new User
        {
            Name = "María Pérez",
            CedulaRnc = cedula,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
            Role = Roles.User,
            PersonType = PersonTypes.Fisica,
            ExpenseLimit = 30000,
            CutDay = 1,
            CreatedAt = DateTime.UtcNow.AddMonths(-3) // created months ago so it can already have cuts
        };
        await users.CreateAsync(user);

        // María's catalogs belong to her (the same starter set any new user gets).
        var all = CatalogSeeder.StarterFor(user.Id);
        await catalogs.CreateManyAsync(all);
        CatalogItem Get(string type, string description) =>
            all.First(c => c.Type == type && c.Description == description);

        var fijo = Get(CatalogTypes.ExpenseType, "Fijo");
        var variable = Get(CatalogTypes.ExpenseType, "Variable");
        var salario = Get(CatalogTypes.IncomeType, "Salario");
        var extra = Get(CatalogTypes.IncomeType, "Ingreso extra");
        var alimentacion = Get(CatalogTypes.ExpenseCategory, "Alimentación");
        var vivienda = Get(CatalogTypes.ExpenseCategory, "Vivienda");
        var transporte = Get(CatalogTypes.ExpenseCategory, "Transporte");
        var entretenimiento = Get(CatalogTypes.ExpenseCategory, "Entretenimiento");
        var efectivo = Get(CatalogTypes.PaymentType, "Efectivo");
        var transferencia = Get(CatalogTypes.PaymentType, "Transferencia");
        var debito = Get(CatalogTypes.PaymentType, "Tarjeta de débito");
        var credito = Get(CatalogTypes.PaymentType, "Tarjeta de crédito");

        ExpenseItem Exp(string d, CatalogItem type, CatalogItem cat, CatalogItem pay) => new()
            { UserId = user.Id, Description = d, ExpenseTypeId = type.Id, CategoryId = cat.Id, DefaultPaymentTypeId = pay.Id };
        IncomeItem Inc(string d, CatalogItem type, string source) => new()
            { UserId = user.Id, Description = d, IncomeTypeId = type.Id, Source = source };

        var supermercado = Exp("Supermercado", variable, alimentacion, credito);
        var alquiler = Exp("Alquiler", fijo, vivienda, transferencia);
        var gasolina = Exp("Gasolina", variable, transporte, efectivo);
        var cine = Exp("Cine y salidas", variable, entretenimiento, debito);
        var sueldo = Inc("Salario mensual", salario, "Empresa Demo SRL");
        var freelance = Inc("Trabajo freelance", extra, "Cliente Demo");
        foreach (var e in new[] { supermercado, alquiler, gasolina, cine }) await expenseItems.CreateAsync(e);
        foreach (var i in new[] { sueldo, freelance }) await incomeItems.CreateAsync(i);

        // (days ago, concept, amount, payment type, card)
        var today = AppClock.Today;
        async Task AddExpense(int daysAgo, ExpenseItem item, decimal amount, CatalogItem pay, string? card = null)
        {
            var date = today.AddDays(-daysAgo);
            await transactions.CreateAsync(new Transaction
            {
                Number = await counters.NextAsync("transactions"), Type = TransactionTypes.Expense,
                UserId = user.Id, UserName = user.Name,
                ItemId = item.Id, ItemDescription = item.Description,
                ExpenseTypeId = item.ExpenseTypeId, ExpenseTypeName = all.First(c => c.Id == item.ExpenseTypeId).Description,
                CategoryId = item.CategoryId, CategoryName = all.First(c => c.Id == item.CategoryId).Description,
                PaymentTypeId = pay.Id, PaymentTypeName = pay.Description,
                TransactionDate = date, RegisteredAt = date, Amount = amount, CardLast4 = card
            });
        }
        async Task AddIncome(int daysAgo, IncomeItem item, decimal amount, CatalogItem pay)
        {
            var date = today.AddDays(-daysAgo);
            await transactions.CreateAsync(new Transaction
            {
                Number = await counters.NextAsync("transactions"), Type = TransactionTypes.Income,
                UserId = user.Id, UserName = user.Name,
                ItemId = item.Id, ItemDescription = item.Description,
                IncomeTypeName = all.First(c => c.Id == item.IncomeTypeId).Description, Source = item.Source,
                PaymentTypeId = pay.Id, PaymentTypeName = pay.Description,
                TransactionDate = date, RegisteredAt = date, Amount = amount
            });
        }

        await AddIncome(40, sueldo, 60000, transferencia);
        await AddExpense(38, alquiler, 18000, transferencia);
        await AddExpense(36, supermercado, 6500.50m, credito, "4242");
        await AddExpense(30, gasolina, 2000, efectivo);
        await AddIncome(27, freelance, 12000, transferencia);
        await AddIncome(14, sueldo, 60000, transferencia);
        await AddExpense(12, alquiler, 18000, transferencia);
        await AddExpense(9, supermercado, 7200, credito, "4242");
        await AddExpense(5, cine, 1500, debito, "1111");
        await AddExpense(2, gasolina, 1800, efectivo);
    }
}
