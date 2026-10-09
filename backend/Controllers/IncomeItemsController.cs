using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasApi.Controllers;

// Each user manages their own income items. An administrator can pass ?userId= to manage another user's.
[ApiController]
[Route("api/income-items")]
[Authorize]
public class IncomeItemsController : ControllerBase
{
    private readonly IIncomeItemService _service;
    public IncomeItemsController(IIncomeItemService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<IncomeItemResponseDto>>> GetAll(
        [FromQuery] bool activeOnly = false, [FromQuery] string? userId = null) =>
        Ok(await _service.GetAllAsync(User.ResolveOwnerId(userId), activeOnly));

    [HttpPost]
    public async Task<ActionResult<IncomeItemResponseDto>> Create(IncomeItemDto dto, [FromQuery] string? userId = null) =>
        Ok(await _service.CreateAsync(User.ResolveOwnerId(userId), dto));

    [HttpPut("{id}")]
    public async Task<ActionResult<IncomeItemResponseDto>> Update(string id, IncomeItemDto dto, [FromQuery] string? userId = null) =>
        Ok(await _service.UpdateAsync(User.ResolveOwnerId(userId), id, dto));

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<IncomeItemResponseDto>> SetStatus(string id, StatusDto dto, [FromQuery] string? userId = null) =>
        Ok(await _service.SetStatusAsync(User.ResolveOwnerId(userId), id, dto.IsActive));
}
