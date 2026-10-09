using System.ComponentModel.DataAnnotations;

namespace FinanzasApi.DTOs;

public class LoginDto
{
    [Required] public string Identifier { get; set; } = "";   // correo o cédula/RNC
    [Required] public string Password { get; set; } = "";
}

public class AuthResponseDto
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public UserResponseDto User { get; set; } = new();
}
