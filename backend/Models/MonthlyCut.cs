using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

// Closing of a user's period. It is a "frozen" document: it stores the calculated totals,
// so reading it later does not require summing the transactions again.
// Unique index (UserId, Year, Month): a user cannot have two cuts for the same month.
public class MonthlyCut
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)] public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";

    public int Year { get; set; }
    public int Month { get; set; }

    // Cut date = last day of the period.
    public DateTime CutDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    [BsonRepresentation(BsonType.Decimal128)] public decimal InitialBalance { get; set; }
    [BsonRepresentation(BsonType.Decimal128)] public decimal TotalIncome { get; set; }
    [BsonRepresentation(BsonType.Decimal128)] public decimal TotalExpense { get; set; }
    [BsonRepresentation(BsonType.Decimal128)] public decimal FinalBalance { get; set; }

    // "Auto" (background process) or "Manual" (button).
    public string Source { get; set; } = "Auto";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
