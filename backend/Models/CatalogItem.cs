using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

// A single collection ("catalogs") for the four simple catalogs.
// The Type field says which catalog an item belongs to and UserId says who owns it.
public class CatalogItem
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // Record owner: each user has their own catalogs, expense items and income items.
    [BsonRepresentation(BsonType.ObjectId)] public string UserId { get; set; } = "";

    public string Type { get; set; } = "";
    public string Description { get; set; } = "";

    // Only applies to PaymentType: whether a card number is required.
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

    // URL segment → internal type.
    public static readonly Dictionary<string, string> BySlug = new()
    {
        ["expense-types"] = ExpenseType,
        ["income-types"] = IncomeType,
        ["categories"] = ExpenseCategory,
        ["payment-types"] = PaymentType,
    };
}
