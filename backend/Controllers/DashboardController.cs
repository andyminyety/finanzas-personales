using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasApi.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    public DashboardController(IDashboardService service) => _service = service;

    /// <summary>userId vacío = el usuario actual; "all" = todos (solo admin); un id = ese usuario (solo admin).</summary>
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get([FromQuery] string? userId) =>
        Ok(await _service.GetAsync(userId, User.GetUserId(), User.IsAdmin()));
}
