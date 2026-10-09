using FinanzasApi.DTOs;
using FinanzasApi.Middleware;
using FinanzasApi.Models;
using FinanzasApi.Repositories;

namespace FinanzasApi.Services;

/// <summary>Todos los métodos reciben el ownerId: cada usuario solo ve y modifica sus propios catálogos.</summary>
public interface ICatalogService
{
    Task<List<CatalogItemResponseDto>> GetAllAsync(string ownerId, string slug, bool activeOnly);
    Task<CatalogItemResponseDto> CreateAsync(string ownerId, string slug, CatalogItemDto dto);
    Task<CatalogItemResponseDto> UpdateAsync(string ownerId, string slug, string id, CatalogItemDto dto);
    Task<CatalogItemResponseDto> SetStatusAsync(string ownerId, string slug, string id, bool isActive);
    /// <summary>Exige que el elemento exista, sea del tipo correcto, sea DEL DUEÑO y esté activo.</summary>
    Task<CatalogItem> RequireAsync(string ownerId, string id, string type, string label, string? currentId = null);
}

public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _repo;
    private readonly IUserRepository _users;

    public CatalogService(ICatalogRepository repo, IUserRepository users)
    {
        _repo = repo;
        _users = users;
    }

    public async Task<List<CatalogItemResponseDto>> GetAllAsync(string ownerId, string slug, bool activeOnly) =>
        (await _repo.GetByTypeAsync(ownerId, ResolveType(slug), activeOnly)).Select(ToDto).ToList();

    public async Task<CatalogItemResponseDto> CreateAsync(string ownerId, string slug, CatalogItemDto dto)
    {
        var type = ResolveType(slug);
        if (await _users.GetByIdAsync(ownerId) is null) throw new AppException("Usuario no encontrado.", 404);
        await EnsureUniqueAsync(ownerId, type, dto.Description, null);

        var item = new CatalogItem
        {
            UserId = ownerId,
            Type = type,
            Description = dto.Description.Trim(),
            RequiresCard = type == CatalogTypes.PaymentType && dto.RequiresCard
        };
        await _repo.CreateAsync(item);
        return ToDto(item);
    }

    public async Task<CatalogItemResponseDto> UpdateAsync(string ownerId, string slug, string id, CatalogItemDto dto)
    {
        var type = ResolveType(slug);
        var item = await Find(ownerId, id, type);
        await EnsureUniqueAsync(ownerId, type, dto.Description, id);

        item.Description = dto.Description.Trim();
        item.RequiresCard = type == CatalogTypes.PaymentType && dto.RequiresCard;
        await _repo.ReplaceAsync(item);
        return ToDto(item);
    }

    public async Task<CatalogItemResponseDto> SetStatusAsync(string ownerId, string slug, string id, bool isActive)
    {
        var item = await Find(ownerId, id, ResolveType(slug));
        item.IsActive = isActive;
        await _repo.ReplaceAsync(item);
        return ToDto(item);
    }

    public async Task<CatalogItem> RequireAsync(string ownerId, string id, string type, string label, string? currentId = null)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item is null || item.Type != type || item.UserId != ownerId)
            throw new AppException($"{label} no es válido.");
        if (!item.IsActive && id != currentId)
            throw new AppException($"{label} está inhabilitado.");
        return item;
    }

    private static string ResolveType(string slug) =>
        CatalogTypes.BySlug.TryGetValue(slug, out var t) ? t : throw new AppException("Catálogo no encontrado.", 404);

    /// <summary>Si el elemento es de otro usuario responde 404, como si no existiera.</summary>
    private async Task<CatalogItem> Find(string ownerId, string id, string type)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is not null && item.Type == type && item.UserId == ownerId
            ? item
            : throw new AppException("Elemento no encontrado.", 404);
    }

    private async Task EnsureUniqueAsync(string ownerId, string type, string description, string? exceptId)
    {
        var existing = await _repo.GetByTypeAsync(ownerId, type, false);
        if (existing.Any(c => c.Id != exceptId &&
                string.Equals(c.Description, description.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new AppException("Ya existe un elemento con esa descripción.", 409);
    }

    private static CatalogItemResponseDto ToDto(CatalogItem c) => new()
    {
        Id = c.Id, Description = c.Description, RequiresCard = c.RequiresCard, IsActive = c.IsActive
    };
}
