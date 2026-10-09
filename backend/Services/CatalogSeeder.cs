using FinanzasApi.Models;

namespace FinanzasApi.Services;

/// <summary>
/// Catálogos de arranque que recibe cada usuario nuevo para no empezar con todo vacío.
/// Son suyos: puede editarlos, inhabilitarlos o agregar más.
/// </summary>
public static class CatalogSeeder
{
    public static List<CatalogItem> StarterFor(string userId)
    {
        var items = new List<CatalogItem>();
        void Add(string type, string description, bool requiresCard = false) =>
            items.Add(new CatalogItem { UserId = userId, Type = type, Description = description, RequiresCard = requiresCard });

        Add(CatalogTypes.ExpenseType, "Fijo");
        Add(CatalogTypes.ExpenseType, "Variable");
        Add(CatalogTypes.IncomeType, "Salario");
        Add(CatalogTypes.IncomeType, "Ingreso extra");
        Add(CatalogTypes.ExpenseCategory, "Alimentación");
        Add(CatalogTypes.ExpenseCategory, "Transporte");
        Add(CatalogTypes.ExpenseCategory, "Vivienda");
        Add(CatalogTypes.ExpenseCategory, "Salud");
        Add(CatalogTypes.ExpenseCategory, "Entretenimiento");
        Add(CatalogTypes.PaymentType, "Efectivo");
        Add(CatalogTypes.PaymentType, "Transferencia");
        Add(CatalogTypes.PaymentType, "Tarjeta de débito", true);
        Add(CatalogTypes.PaymentType, "Tarjeta de crédito", true);
        return items;
    }
}
