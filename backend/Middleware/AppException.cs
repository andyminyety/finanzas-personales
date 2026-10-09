namespace FinanzasApi.Middleware;

/// <summary>Error de negocio controlado: se convierte en una respuesta HTTP con mensaje.</summary>
public class AppException : Exception
{
    public int StatusCode { get; }
    public AppException(string message, int statusCode = 400) : base(message) => StatusCode = statusCode;
}
