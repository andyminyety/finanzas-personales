namespace FinanzasApi.Middleware;

// Handled business error: converted into an HTTP response with a message.
public class AppException : Exception
{
    public int StatusCode { get; }
    public AppException(string message, int statusCode = 400) : base(message) => StatusCode = statusCode;
}
