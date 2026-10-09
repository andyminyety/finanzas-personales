using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasApi.Models;

public class User
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = "";
    public string CedulaRnc { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";

    // "Admin" or "User"
    public string Role { get; set; } = Roles.User;

    // "Fisica" (individual) or "Juridica" (legal entity)
    public string PersonType { get; set; } = PersonTypes.Fisica;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal ExpenseLimit { get; set; }

    // Day of the month (1-28) on which the cut is made.
    public int CutDay { get; set; } = 1;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public static class PersonTypes
{
    public const string Fisica = "Fisica";
    public const string Juridica = "Juridica";
}
