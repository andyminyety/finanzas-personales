using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Middleware;
using FinanzasApi.Models;
using FinanzasApi.Repositories;

namespace FinanzasApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly ICatalogRepository _catalogs;

    public UserService(IUserRepository repo, ICatalogRepository catalogs)
    {
        _repo = repo;
        _catalogs = catalogs;
    }

    public async Task<List<UserResponseDto>> GetAllAsync() =>
        (await _repo.GetAllAsync()).Select(u => u.ToDto()).ToList();

    public async Task<UserResponseDto> GetByIdAsync(string id) => (await Find(id)).ToDto();

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    {
        var cedula = CedulaRncValidator.Normalize(dto.CedulaRnc);
        var email = dto.Email.Trim().ToLowerInvariant();

        var error = CedulaRncValidator.Validate(dto.PersonType, cedula);
        if (error is not null) throw new AppException(error);

        if (await _repo.GetByCedulaAsync(cedula) is not null)
            throw new AppException("Ya existe un usuario con esa cédula/RNC.", 409);
        if (await _repo.GetByEmailAsync(email) is not null)
            throw new AppException("Ya existe un usuario con ese correo.", 409);

        var user = new User
        {
            Name = dto.Name.Trim(),
            CedulaRnc = cedula,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            PersonType = dto.PersonType,
            ExpenseLimit = dto.ExpenseLimit,
            CutDay = dto.CutDay
        };
        await _repo.CreateAsync(user);

        // Cada usuario nuevo recibe sus propios catálogos iniciales (que luego puede personalizar).
        await _catalogs.CreateManyAsync(CatalogSeeder.StarterFor(user.Id));
        return user.ToDto();
    }

    public async Task<UserResponseDto> UpdateAsync(string id, UpdateUserDto dto)
    {
        var user = await Find(id);
        var cedula = CedulaRncValidator.Normalize(dto.CedulaRnc);
        var email = dto.Email.Trim().ToLowerInvariant();

        var error = CedulaRncValidator.Validate(dto.PersonType, cedula);
        if (error is not null) throw new AppException(error);

        var byCedula = await _repo.GetByCedulaAsync(cedula);
        if (byCedula is not null && byCedula.Id != id)
            throw new AppException("Ya existe otro usuario con esa cédula/RNC.", 409);
        var byEmail = await _repo.GetByEmailAsync(email);
        if (byEmail is not null && byEmail.Id != id)
            throw new AppException("Ya existe otro usuario con ese correo.", 409);

        user.Name = dto.Name.Trim();
        user.CedulaRnc = cedula;
        user.Email = email;
        user.ExpenseLimit = dto.ExpenseLimit;
        user.PersonType = dto.PersonType;
        user.CutDay = dto.CutDay;
        user.Role = dto.Role;
        await _repo.ReplaceAsync(user);
        return user.ToDto();
    }

    public async Task<UserResponseDto> SetStatusAsync(string id, bool isActive, string currentUserId)
    {
        if (id == currentUserId && !isActive)
            throw new AppException("No puedes inhabilitar tu propio usuario.");
        var user = await Find(id);
        user.IsActive = isActive;
        await _repo.ReplaceAsync(user);
        return user.ToDto();
    }

    public async Task<UserResponseDto> UpdateProfileAsync(string id, UpdateProfileDto dto, bool isAdmin)
    {
        var user = await Find(id);
        var email = dto.Email.Trim().ToLowerInvariant();

        var byEmail = await _repo.GetByEmailAsync(email);
        if (byEmail is not null && byEmail.Id != id)
            throw new AppException("Ese correo ya está en uso.", 409);

        // El límite y el día de corte los ajusta cada usuario (son personales).
        if (dto.ExpenseLimit.HasValue) user.ExpenseLimit = dto.ExpenseLimit.Value;
        if (dto.CutDay.HasValue) user.CutDay = dto.CutDay.Value;

        // La identidad (cédula/RNC y tipo de persona) solo la cambia un administrador; para los demás se ignora.
        if (isAdmin)
        {
            var personType = dto.PersonType ?? user.PersonType;
            var cedula = dto.CedulaRnc is null ? user.CedulaRnc : CedulaRncValidator.Normalize(dto.CedulaRnc);

            if (cedula != user.CedulaRnc || personType != user.PersonType)
            {
                var error = CedulaRncValidator.Validate(personType, cedula);
                if (error is not null) throw new AppException(error);

                var byCedula = await _repo.GetByCedulaAsync(cedula);
                if (byCedula is not null && byCedula.Id != id)
                    throw new AppException("Ya existe otro usuario con esa cédula/RNC.", 409);
            }

            user.PersonType = personType;
            user.CedulaRnc = cedula;
        }

        user.Name = dto.Name.Trim();
        user.Email = email;
        await _repo.ReplaceAsync(user);
        return user.ToDto();
    }

    public async Task ChangePasswordAsync(string id, ChangePasswordDto dto)
    {
        var user = await Find(id);
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new AppException("La contraseña actual es incorrecta.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _repo.ReplaceAsync(user);
    }

    private async Task<User> Find(string id) =>
        await _repo.GetByIdAsync(id) ?? throw new AppException("Usuario no encontrado.", 404);
}
