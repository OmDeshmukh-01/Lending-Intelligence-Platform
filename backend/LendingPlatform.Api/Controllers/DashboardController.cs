using Microsoft.AspNetCore.Mvc;
using LendingPlatform.Application.DTOs;
using LendingPlatform.Application.Services;

namespace LendingPlatform.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly LoanApplicationService _service;

    public DashboardController(LoanApplicationService service)
    {
        _service = service;
    }

    /// <summary>Retrieve aggregated portfolio statistics and recent applications.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var dashboard = await _service.GetDashboardAsync(ct);
        return Ok(dashboard);
    }
}
