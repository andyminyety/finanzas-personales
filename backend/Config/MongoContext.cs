using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FinanzasApi.Config;

/// <summary>Punto único de acceso a la base de datos MongoDB.</summary>
public class MongoContext
{
    public IMongoDatabase Database { get; }
    public IMongoClient Client { get; }

    public MongoContext(IOptions<MongoSettings> options)
    {
        var s = options.Value;
        if (string.IsNullOrWhiteSpace(s.ConnectionString))
            throw new InvalidOperationException(
                "Falta Mongo:ConnectionString. Configúrala con: dotnet user-secrets set \"Mongo:ConnectionString\" \"...\"");
        Client = new MongoClient(s.ConnectionString);
        Database = Client.GetDatabase(s.Database);
    }
}
