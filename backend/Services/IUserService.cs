using FinanzasApi.DTOs;

namespace FinanzasApi.Services;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto> GetByIdAsync(string id);
    Task<UserResponseDto> CreateAsync(CreateUserDto dto);
    Task<UserResponseDto> UpdateAsync(string id, UpdateUserDto dto);
    Task<UserResponseDto> SetStatusAsync(string id, bool isActive, string currentUserId);
    Task<UserResponseDto> UpdateProfileAsync(string id, UpdateProfileDto dto, bool isAdmin);
    Task ChangePasswordAsync(string id, ChangePasswordDto dto);
}
