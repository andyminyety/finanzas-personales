using FinanzasApi.DTOs;
using FinanzasApi.Middleware;
using FinanzasApi.Models;
using FinanzasApi.Repositories;

namespace FinanzasApi.Services;

public interface IExpenseItemService
{
    Task<List<ExpenseItemResponseDto>> GetAllAsync(string ownerId, bool activeOnly);
    Task<ExpenseItemResponseDto> CreateAsync(string ownerId, ExpenseItemDto dto);
    Task<ExpenseItemResponseDto> UpdateAsync(string ownerId, string id, ExpenseItemDto dto);
    Task<ExpenseItemResponseDto> SetStatusAsync(string ownerId, string id, bool isActive);
}

public class ExpenseItemService : IExpenseItemService
{
    private readonly IExpenseItemRepository _repo;
    private readonly ICatalogRepository _catalogs;
    private readonly ICatalogService _catalogService;
    private readonly IUserRepository _users;

    public ExpenseItemService(IExpenseItemRepository repo, ICatalogRepository catalogs,
        ICatalogService catalogService, IUserRepository users)
    {
        _repo = repo;
        _catalogs = catalogs;
        _catalogService = catalogService;
        _users = users;
    }

    public async Task<List<ExpenseItemResponseDto>> GetAllAsync(string ownerId, bool activeOnly)
    {
        var names = await NamesAsync(ownerId);
        return (await _repo.GetAllAsync(ownerId, activeOnly)).Select(i => ToDto(i, names)).ToList();
    }

    public async Task<ExpenseItemResponseDto> CreateAsync(string ownerId, ExpenseItemDto dto)
    {
        if (await _users.GetByIdAsync(ownerId) is null) throw new AppException("Usuario no encontrado.", 404);
        await ValidateAsync(ownerId, dto, null);
        var item = new ExpenseItem
        {
            UserId = ownerId,
            ExpenseTypeId = dto.ExpenseTypeId,
            CategoryId = dto.CategoryId,
            DefaultPaymentTypeId = dto.DefaultPaymentTypeId,
            Description = dto.Description.Trim()
        };
        await _repo.CreateAsync(item);
        return ToDto(item, await NamesAsync(ownerId));
    }

    public async Task<ExpenseItemResponseDto> UpdateAsync(string ownerId, string id, ExpenseItemDto dto)
    {
        var item = await Find(ownerId, id);
        await ValidateAsync(ownerId, dto, item);

        item.ExpenseTypeId = dto.ExpenseTypeId;
        item.CategoryId = dto.CategoryId;
        item.DefaultPaymentTypeId = dto.DefaultPaymentTypeId;
        item.Description = dto.Description.Trim();
        await _repo.ReplaceAsync(item);
        return ToDto(item, await NamesAsync(ownerId));
    }

    public async Task<ExpenseItemResponseDto> SetStatusAsync(string ownerId, string id, bool isActive)
    {
        var item = await Find(ownerId, id);
        item.IsActive = isActive;
        await _repo.ReplaceAsync(item);
        return ToDto(item, await NamesAsync(ownerId));
    }

    private async Task ValidateAsync(string ownerId, ExpenseItemDto dto, ExpenseItem? current)
    {
        // RequireAsync also checks that each chosen catalog belongs to this same user.
        await _catalogService.RequireAsync(ownerId, dto.ExpenseTypeId, CatalogTypes.ExpenseType, "El tipo de egreso", current?.ExpenseTypeId);
        await _catalogService.RequireAsync(ownerId, dto.CategoryId, CatalogTypes.ExpenseCategory, "El renglón", current?.CategoryId);
        await _catalogService.RequireAsync(ownerId, dto.DefaultPaymentTypeId, CatalogTypes.PaymentType, "El tipo de pago", current?.DefaultPaymentTypeId);

        var all = await _repo.GetAllAsync(ownerId, false);
        if (all.Any(i => i.Id != current?.Id &&
                string.Equals(i.Description, dto.Description.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new AppException("Ya existe un egreso con esa descripción.", 409);
    }

    private async Task<ExpenseItem> Find(string ownerId, string id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is not null && item.UserId == ownerId ? item : throw new AppException("Egreso no encontrado.", 404);
    }

    private async Task<Dictionary<string, string>> NamesAsync(string ownerId) =>
        (await _catalogs.GetByUserAsync(ownerId)).ToDictionary(c => c.Id, c => c.Description);

    private static ExpenseItemResponseDto ToDto(ExpenseItem i, Dictionary<string, string> names) => new()
    {
        Id = i.Id,
        ExpenseTypeId = i.ExpenseTypeId,
        ExpenseTypeName = names.GetValueOrDefault(i.ExpenseTypeId, "—"),
        CategoryId = i.CategoryId,
        CategoryName = names.GetValueOrDefault(i.CategoryId, "—"),
        DefaultPaymentTypeId = i.DefaultPaymentTypeId,
        DefaultPaymentTypeName = names.GetValueOrDefault(i.DefaultPaymentTypeId, "—"),
        Description = i.Description,
        IsActive = i.IsActive
    };
}
