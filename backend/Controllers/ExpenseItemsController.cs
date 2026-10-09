using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasApi.Controllers;

/// <summary>Cada usuario gestiona sus egresos. Un administrador puede indicar ?userId= para gestionar los de otro.</summary>
[ApiController]
[Route("api/expense-items")]
[Authorize]
public class ExpenseItemsController : ControllerBase
{
    private readonly IExpenseItemService _service;
    public ExpenseItemsController(IExpenseItemService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<ExpenseItemResponseDto>>> GetAll(
        [FromQuery] bool activeOnly = false, [FromQuery] string? userId = null) =>
        Ok(await _service.GetAllAsync(User.ResolveOwnerId(userId), activeOnly));

    [HttpPost]
    public async Task<ActionResult<ExpenseItemResponseDto>> Create(ExpenseItemDto dto, [FromQuery] string? userId = null) =>
        Ok(await _service.CreateAsync(User.ResolveOwnerId(userId), dto));

    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseItemResponseDto>> Update(string id, ExpenseItemDto dto, [FromQuery] string? userId = null) =>
        Ok(await _service.UpdateAsync(User.ResolveOwnerId(userId), id, dto));

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ExpenseItemResponseDto>> SetStatus(string id, StatusDto dto, [FromQuery] string? userId = null) =>
        Ok(await _service.SetStatusAsync(User.ResolveOwnerId(userId), id, dto.IsActive));
}
