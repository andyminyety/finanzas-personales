using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

// Income item: a reusable income concept (e.g. Salary).
public class IncomeItem
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // Record owner: each user has their own catalogs, expense items and income items.
    [BsonRepresentation(BsonType.ObjectId)] public string UserId { get; set; } = "";

    [BsonRepresentation(BsonType.ObjectId)] public string IncomeTypeId { get; set; } = "";

    public string Description { get; set; } = "";

    // Institution / Employer / Client
    public string Source { get; set; } = "";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
