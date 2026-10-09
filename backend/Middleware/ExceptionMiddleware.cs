namespace FinanzasApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (AppException ex)
        {
            await Write(ctx, ex.StatusCode, ex.Message);
        }
        catch (FormatException)
        {
            // Raised when an id does not have a valid MongoDB ObjectId format.
            await Write(ctx, 400, "Uno de los identificadores enviados no tiene un formato válido.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await Write(ctx, 500, "Ocurrió un error inesperado en el servidor.");
        }
    }

    private static Task Write(HttpContext ctx, int status, string message)
    {
        ctx.Response.StatusCode = status;
        return ctx.Response.WriteAsJsonAsync(new { message });
    }
}
