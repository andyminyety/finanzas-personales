using System.Security.Claims;
using FinanzasApi.Models;

namespace FinanzasApi.Helpers;

public static class ClaimsExtensions
{
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Token inválido.");

    // Data owner: the token's user. An Admin can specify another with ?userId=.
    public static string ResolveOwnerId(this ClaimsPrincipal user, string? requested) =>
        user.IsAdmin() && !string.IsNullOrEmpty(requested) ? requested : user.GetUserId();

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.Admin);
}
