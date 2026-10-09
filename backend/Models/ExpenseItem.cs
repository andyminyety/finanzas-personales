using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

/// <summary>"Egreso" del enunciado: un concepto de gasto reutilizable (ej. Supermercado).</summary>
public class ExpenseItem
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>Dueño del registro: cada usuario tiene sus propios catálogos, egresos e ingresos.</summary>
    [BsonRepresentation(BsonType.ObjectId)] public string UserId { get; set; } = "";

    [BsonRepresentation(BsonType.ObjectId)] public string ExpenseTypeId { get; set; } = "";
    [BsonRepresentation(BsonType.ObjectId)] public string CategoryId { get; set; } = "";
    [BsonRepresentation(BsonType.ObjectId)] public string DefaultPaymentTypeId { get; set; } = "";

    public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
