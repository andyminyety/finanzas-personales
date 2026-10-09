using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

/// <summary>
/// Una sola colección ("catalogs") para los 4 catálogos simples del enunciado.
/// El campo Type indica a cuál pertenece y UserId a quién pertenece.
/// </summary>
public class CatalogItem
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>Dueño del registro: cada usuario tiene sus propios catálogos, egresos e ingresos.</summary>
    [BsonRepresentation(BsonType.ObjectId)] public string UserId { get; set; } = "";

    public string Type { get; set; } = "";
    public string Description { get; set; } = "";

    /// <summary>Solo aplica a PaymentType: indica si exige número de tarjeta (Fase 3).</summary>
    public bool RequiresCard { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class CatalogTypes
{
    public const string ExpenseType = "ExpenseType";
    public const string IncomeType = "IncomeType";
    public const string ExpenseCategory = "ExpenseCategory";
    public const string PaymentType = "PaymentType";

    /// <summary>Segmento de la URL → tipo interno.</summary>
    public static readonly Dictionary<string, string> BySlug = new()
    {
        ["expense-types"] = ExpenseType,
        ["income-types"] = IncomeType,
        ["categories"] = ExpenseCategory,
        ["payment-types"] = PaymentType,
    };
}
