using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

// One document per movement. It stores snapshots (name copies) of the user,
// the concept, the category and the payment type, so reports and queries need no
// joins and the history does not change if a catalog is edited later.
public class Transaction
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // Sequential number (counters collection).
    public long Number { get; set; }

    // "Income" or "Expense"
    public string Type { get; set; } = TransactionTypes.Expense;

    [BsonRepresentation(BsonType.ObjectId)] public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";

    // Concepto (expense_items / income_items)
    [BsonRepresentation(BsonType.ObjectId)] public string ItemId { get; set; } = "";
    public string ItemDescription { get; set; } = "";

    // Expenses only
    [BsonRepresentation(BsonType.ObjectId)] public string? ExpenseTypeId { get; set; }
    public string? ExpenseTypeName { get; set; }
    [BsonRepresentation(BsonType.ObjectId)] public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // Income only
    public string? IncomeTypeName { get; set; }
    public string? Source { get; set; }

    [BsonRepresentation(BsonType.ObjectId)] public string PaymentTypeId { get; set; } = "";
    public string PaymentTypeName { get; set; } = "";

    public DateTime TransactionDate { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Amount { get; set; }

    // Last 4 digits only; the full number is never stored.
    public string? CardLast4 { get; set; }

    public string? Comment { get; set; }
    public bool IsActive { get; set; } = true;
}

public static class TransactionTypes
{
    public const string Income = "Income";
    public const string Expense = "Expense";
}

// Atomic counter for the transaction number.
public class Counter
{
    [BsonId] public string Id { get; set; } = "";
    public long Seq { get; set; }
}
