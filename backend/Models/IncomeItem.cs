using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

/// <summary>"Ingreso" del enunciado: un concepto de ingreso reutilizable (ej. Salario).</summary>
public class IncomeItem
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>Dueño del registro: cada usuario tiene sus propios catálogos, egresos e ingresos.</summary>
    [BsonRepresentation(BsonType.ObjectId)] public string UserId { get; set; } = "";

    [BsonRepresentation(BsonType.ObjectId)] public string IncomeTypeId { get; set; } = "";

    public string Description { get; set; } = "";

    /// <summary>Institución / Empleador / Cliente</summary>
    public string Source { get; set; } = "";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
