using FinanzasApi.DTOs;
using FinanzasApi.Models;

namespace FinanzasApi.Services;

public static class UserMapper
{
    public static UserResponseDto ToDto(this User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        CedulaRnc = u.CedulaRnc,
        Email = u.Email,
        Role = u.Role,
        PersonType = u.PersonType,
        ExpenseLimit = u.ExpenseLimit,
        CutDay = u.CutDay,
        IsActive = u.IsActive
    };
}
