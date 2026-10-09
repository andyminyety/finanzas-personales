using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanzasApi.Config;
using FinanzasApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanzasApi.Services;

public class JwtService
{
    private readonly JwtSettings _settings;
    public JwtService(IOptions<JwtSettings> options) => _settings = options.Value;

    public (string Token, DateTime ExpiresAt) Generate(User user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpireMinutes);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
