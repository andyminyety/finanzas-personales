using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

/// <summary>
/// Cierre de un período de un usuario. Es un documento "congelado": guarda los totales calculados,
/// por lo que consultarlo después no requiere volver a sumar las transacciones.
/// Índice único (UserId, Year, Month) → un usuario no puede tener dos cortes del mismo mes.
/// </summary>
public class MonthlyCut
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)] public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";

    public int Year { get; set; }
    public int Month { get; set; }

    /// <summary>Fecha de corte = último día del período.</summary>
    public DateTime CutDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    [BsonRepresentation(BsonType.Decimal128)] public decimal InitialBalance { get; set; }
    [BsonRepresentation(BsonType.Decimal128)] public decimal TotalIncome { get; set; }
    [BsonRepresentation(BsonType.Decimal128)] public decimal TotalExpense { get; set; }
    [BsonRepresentation(BsonType.Decimal128)] public decimal FinalBalance { get; set; }

    /// <summary>"Auto" (proceso diario) o "Manual" (botón).</summary>
    public string Source { get; set; } = "Auto";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
