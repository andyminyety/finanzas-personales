using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

/// <summary>
/// Un solo documento por movimiento. Guarda "snapshots" (copias del nombre) del usuario,
/// el concepto, el renglón y el tipo de pago, para que reportes y consultas no necesiten
/// joins y el historial no cambie si luego se edita un catálogo.
/// </summary>
public class Transaction
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>Número secuencial (colección counters).</summary>
    public long Number { get; set; }

    /// <summary>"Income" o "Expense"</summary>
    public string Type { get; set; } = TransactionTypes.Expense;

    [BsonRepresentation(BsonType.ObjectId)] public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";

    // Concepto (expense_items / income_items)
    [BsonRepresentation(BsonType.ObjectId)] public string ItemId { get; set; } = "";
    public string ItemDescription { get; set; } = "";

    // Solo egresos
    [BsonRepresentation(BsonType.ObjectId)] public string? ExpenseTypeId { get; set; }
    public string? ExpenseTypeName { get; set; }
    [BsonRepresentation(BsonType.ObjectId)] public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // Solo ingresos
    public string? IncomeTypeName { get; set; }
    public string? Source { get; set; }

    [BsonRepresentation(BsonType.ObjectId)] public string PaymentTypeId { get; set; } = "";
    public string PaymentTypeName { get; set; } = "";

    public DateTime TransactionDate { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Amount { get; set; }

    /// <summary>Solo los últimos 4 dígitos; nunca se guarda el número completo.</summary>
    public string? CardLast4 { get; set; }

    public string? Comment { get; set; }
    public bool IsActive { get; set; } = true;
}

public static class TransactionTypes
{
    public const string Income = "Income";
    public const string Expense = "Expense";
}

/// <summary>Contador atómico para el número de transacción.</summary>
public class Counter
{
    [BsonId] public string Id { get; set; } = "";
    public long Seq { get; set; }
}
