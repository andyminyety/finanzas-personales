using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

// Expense item: a reusable expense concept (e.g. Supermarket).
public class ExpenseItem
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // Record owner: each user has their own catalogs, expense items and income items.
    [BsonRepresentation(BsonType.ObjectId)] public string UserId { get; set; } = "";

    [BsonRepresentation(BsonType.ObjectId)] public string ExpenseTypeId { get; set; } = "";
    [BsonRepresentation(BsonType.ObjectId)] public string CategoryId { get; set; } = "";
    [BsonRepresentation(BsonType.ObjectId)] public string DefaultPaymentTypeId { get; set; } = "";

    public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
