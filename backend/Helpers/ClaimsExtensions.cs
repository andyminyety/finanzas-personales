using System.Security.Claims;
using FinanzasApi.Models;

namespace FinanzasApi.Helpers;

public static class ClaimsExtensions
{
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Token inválido.");

    /// <summary>Dueño de los datos: el usuario del token. Un Admin puede indicar otro con ?userId=.</summary>
    public static string ResolveOwnerId(this ClaimsPrincipal user, string? requested) =>
        user.IsAdmin() && !string.IsNullOrEmpty(requested) ? requested : user.GetUserId();

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.Admin);
}
