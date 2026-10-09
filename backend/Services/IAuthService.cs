using FinanzasApi.DTOs;

namespace FinanzasApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
