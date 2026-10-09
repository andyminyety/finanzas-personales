using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasApi.Controllers;

/// <summary>El usuario sale siempre del token: cada quien solo ve y registra lo suyo; el admin ve todo.</summary>
[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;
    public TransactionsController(ITransactionService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<TransactionPageDto>> Search([FromQuery] TransactionQuery query) =>
        Ok(await _service.SearchAsync(query, User.GetUserId(), User.IsAdmin()));

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionResponseDto>> Get(string id) =>
        Ok(await _service.GetByIdAsync(id, User.GetUserId(), User.IsAdmin()));

    [HttpPost]
    public async Task<ActionResult<TransactionResultDto>> Create(CreateTransactionDto dto) =>
        Ok(await _service.CreateAsync(dto, User.GetUserId(), User.IsAdmin()));

    [HttpPut("{id}")]
    public async Task<ActionResult<TransactionResultDto>> Update(string id, UpdateTransactionDto dto) =>
        Ok(await _service.UpdateAsync(id, dto, User.GetUserId(), User.IsAdmin()));

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TransactionResponseDto>> SetStatus(string id, StatusDto dto) =>
        Ok(await _service.SetStatusAsync(id, dto.IsActive, User.GetUserId(), User.IsAdmin()));
}
