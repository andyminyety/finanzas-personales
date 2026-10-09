using System.ComponentModel.DataAnnotations;

namespace FinanzasApi.DTOs;

public class UserResponseDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string CedulaRnc { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string PersonType { get; set; } = "";
    public decimal ExpenseLimit { get; set; }
    public int CutDay { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUserDto
{
    [Required, StringLength(120, MinimumLength = 2)] public string Name { get; set; } = "";
    [Required] public string CedulaRnc { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6)] public string Password { get; set; } = "";
    [Range(0, double.MaxValue)] public decimal ExpenseLimit { get; set; }
    [Required, RegularExpression("Fisica|Juridica")] public string PersonType { get; set; } = "Fisica";
    [Range(1, 28)] public int CutDay { get; set; } = 1;
    [Required, RegularExpression("Admin|User")] public string Role { get; set; } = "User";
}

public class UpdateUserDto
{
    [Required, StringLength(120, MinimumLength = 2)] public string Name { get; set; } = "";
    [Required] public string CedulaRnc { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Range(0, double.MaxValue)] public decimal ExpenseLimit { get; set; }
    [Required, RegularExpression("Fisica|Juridica")] public string PersonType { get; set; } = "Fisica";
    [Range(1, 28)] public int CutDay { get; set; } = 1;
    [Required, RegularExpression("Admin|User")] public string Role { get; set; } = "User";
}

public class StatusDto
{
    public bool IsActive { get; set; }
}

public class UpdateProfileDto
{
    [Required, StringLength(120, MinimumLength = 2)] public string Name { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";

    // Personal: any user can apply these to their own profile.
    [Range(0, double.MaxValue)] public decimal? ExpenseLimit { get; set; }
    [Range(1, 28)] public int? CutDay { get; set; }

    // Identity: only applied for administrators; ignored for regular users.
    [RegularExpression("Fisica|Juridica")] public string? PersonType { get; set; }
    public string? CedulaRnc { get; set; }
}

public class ChangePasswordDto
{
    [Required] public string CurrentPassword { get; set; } = "";
    [Required, MinLength(6)] public string NewPassword { get; set; } = "";
}
