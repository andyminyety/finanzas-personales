using FinanzasApi.DTOs;
using FinanzasApi.Middleware;
using FinanzasApi.Repositories;

namespace FinanzasApi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly JwtService _jwt;

    public AuthService(IUserRepository users, JwtService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByLoginAsync(dto.Identifier);

        // Same message for unknown user and wrong password (does not reveal which one failed).
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new AppException("Credenciales incorrectas.", 401);

        if (!user.IsActive)
            throw new AppException("Tu usuario está inhabilitado. Contacta al administrador.", 403);

        var (token, expires) = _jwt.Generate(user);
        return new AuthResponseDto { Token = token, ExpiresAt = expires, User = user.ToDto() };
    }
}
