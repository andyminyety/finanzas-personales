using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasApi.Controllers;

/// <summary>
/// {type}: expense-types | income-types | categories | payment-types.
/// Cada usuario gestiona los SUYOS. Un administrador puede indicar ?userId= para gestionar los de otro usuario.
/// </summary>
[ApiController]
[Route("api/catalogs/{type}")]
[Authorize]
public class CatalogsController : ControllerBase
{
    private readonly ICatalogService _service;
    public CatalogsController(ICatalogService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<CatalogItemResponseDto>>> GetAll(
        string type, [FromQuery] bool activeOnly = false, [FromQuery] string? userId = null) =>
        Ok(await _service.GetAllAsync(User.ResolveOwnerId(userId), type, activeOnly));

    [HttpPost]
    public async Task<ActionResult<CatalogItemResponseDto>> Create(
        string type, CatalogItemDto dto, [FromQuery] string? userId = null) =>
        Ok(await _service.CreateAsync(User.ResolveOwnerId(userId), type, dto));

    [HttpPut("{id}")]
    public async Task<ActionResult<CatalogItemResponseDto>> Update(
        string type, string id, CatalogItemDto dto, [FromQuery] string? userId = null) =>
        Ok(await _service.UpdateAsync(User.ResolveOwnerId(userId), type, id, dto));

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<CatalogItemResponseDto>> SetStatus(
        string type, string id, StatusDto dto, [FromQuery] string? userId = null) =>
        Ok(await _service.SetStatusAsync(User.ResolveOwnerId(userId), type, id, dto.IsActive));
}
