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

    /// <summary>Historial de cortes. Usuario normal: los suyos. Admin: todos (o filtrados).</summary>
    [HttpGet]
    public async Task<ActionResult<List<CutResponseDto>>> Search([FromQuery] CutQuery query) =>
        Ok(await _service.SearchAsync(query, User.GetUserId(), User.IsAdmin()));

    /// <summary>Corte manual: genera los cortes pendientes (períodos ya terminados).</summary>
    [HttpPost("run")]
    public async Task<ActionResult<RunCutsResultDto>> Run(RunCutsDto dto) =>
        Ok(await _service.RunManualAsync(dto, User.GetUserId(), User.IsAdmin()));
}
