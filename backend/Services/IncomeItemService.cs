using FinanzasApi.DTOs;
using FinanzasApi.Middleware;
using FinanzasApi.Models;
using FinanzasApi.Repositories;

namespace FinanzasApi.Services;

public interface IIncomeItemService
{
    Task<List<IncomeItemResponseDto>> GetAllAsync(string ownerId, bool activeOnly);
    Task<IncomeItemResponseDto> CreateAsync(string ownerId, IncomeItemDto dto);
    Task<IncomeItemResponseDto> UpdateAsync(string ownerId, string id, IncomeItemDto dto);
    Task<IncomeItemResponseDto> SetStatusAsync(string ownerId, string id, bool isActive);
}

public class IncomeItemService : IIncomeItemService
{
    private readonly IIncomeItemRepository _repo;
    private readonly ICatalogRepository _catalogs;
    private readonly ICatalogService _catalogService;
    private readonly IUserRepository _users;

    public IncomeItemService(IIncomeItemRepository repo, ICatalogRepository catalogs,
        ICatalogService catalogService, IUserRepository users)
    {
        _repo = repo;
        _catalogs = catalogs;
        _catalogService = catalogService;
        _users = users;
    }

    public async Task<List<IncomeItemResponseDto>> GetAllAsync(string ownerId, bool activeOnly)
    {
        var names = await NamesAsync(ownerId);
        return (await _repo.GetAllAsync(ownerId, activeOnly)).Select(i => ToDto(i, names)).ToList();
    }

    public async Task<IncomeItemResponseDto> CreateAsync(string ownerId, IncomeItemDto dto)
    {
        if (await _users.GetByIdAsync(ownerId) is null) throw new AppException("Usuario no encontrado.", 404);
        await ValidateAsync(ownerId, dto, null);
        var item = new IncomeItem
        {
            UserId = ownerId,
            IncomeTypeId = dto.IncomeTypeId,
            Description = dto.Description.Trim(),
            Source = dto.Source.Trim()
        };
        await _repo.CreateAsync(item);
        return ToDto(item, await NamesAsync(ownerId));
    }

    public async Task<IncomeItemResponseDto> UpdateAsync(string ownerId, string id, IncomeItemDto dto)
    {
        var item = await Find(ownerId, id);
        await ValidateAsync(ownerId, dto, item);

        item.IncomeTypeId = dto.IncomeTypeId;
        item.Description = dto.Description.Trim();
        item.Source = dto.Source.Trim();
        await _repo.ReplaceAsync(item);
        return ToDto(item, await NamesAsync(ownerId));
    }

    public async Task<IncomeItemResponseDto> SetStatusAsync(string ownerId, string id, bool isActive)
    {
        var item = await Find(ownerId, id);
        item.IsActive = isActive;
        await _repo.ReplaceAsync(item);
        return ToDto(item, await NamesAsync(ownerId));
    }

    private async Task ValidateAsync(string ownerId, IncomeItemDto dto, IncomeItem? current)
    {
        await _catalogService.RequireAsync(ownerId, dto.IncomeTypeId, CatalogTypes.IncomeType, "El tipo de ingreso", current?.IncomeTypeId);

        var all = await _repo.GetAllAsync(ownerId, false);
        if (all.Any(i => i.Id != current?.Id &&
                string.Equals(i.Description, dto.Description.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new AppException("Ya existe un ingreso con esa descripción.", 409);
    }

    private async Task<IncomeItem> Find(string ownerId, string id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is not null && item.UserId == ownerId ? item : throw new AppException("Ingreso no encontrado.", 404);
    }

    private async Task<Dictionary<string, string>> NamesAsync(string ownerId) =>
        (await _catalogs.GetByUserAsync(ownerId)).ToDictionary(c => c.Id, c => c.Description);

    private static IncomeItemResponseDto ToDto(IncomeItem i, Dictionary<string, string> names) => new()
    {
        Id = i.Id,
        IncomeTypeId = i.IncomeTypeId,
        IncomeTypeName = names.GetValueOrDefault(i.IncomeTypeId, "—"),
        Description = i.Description,
        Source = i.Source,
        IsActive = i.IsActive
    };
}
