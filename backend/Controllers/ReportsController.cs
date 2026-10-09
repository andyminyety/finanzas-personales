using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasApi.Controllers;

/// <summary>kind: range | by-user | by-type | by-category | monthly. Usuario normal: solo lo suyo; admin: todo.</summary>
[ApiController]
[Route("api/reports/{kind}")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    public ReportsController(IReportService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ReportDto>> Get(string kind, [FromQuery] ReportQuery query) =>
        Ok(await _service.GetAsync(kind, query, User.GetUserId(), User.IsAdmin()));
}
