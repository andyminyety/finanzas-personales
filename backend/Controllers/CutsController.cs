using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasApi.Controllers;

[ApiController]
[Route("api/cuts")]
[Authorize]
public class CutsController : ControllerBase
{
    private readonly ICutService _service;
    public CutsController(ICutService service) => _service = service;

    // Cut history. Regular user: their own. Admin: all (or filtered).
    [HttpGet]
    public async Task<ActionResult<List<CutResponseDto>>> Search([FromQuery] CutQuery query) =>
        Ok(await _service.SearchAsync(query, User.GetUserId(), User.IsAdmin()));

    // Manual cut: generates pending cuts (periods that have already ended).
    [HttpPost("run")]
    public async Task<ActionResult<RunCutsResultDto>> Run(RunCutsDto dto) =>
        Ok(await _service.RunManualAsync(dto, User.GetUserId(), User.IsAdmin()));
}
