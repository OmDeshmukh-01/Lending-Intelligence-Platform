using Microsoft.AspNetCore.Mvc;
using LendingPlatform.Application.DTOs;
using LendingPlatform.Application.Services;
using LendingPlatform.Domain.Enums;

namespace LendingPlatform.Api.Controllers;

/// <summary>
/// Thin controller — delegates all logic to LoanApplicationService.
/// No business rules live here.
/// </summary>
[ApiController]
[Route("api/loans")]
public sealed class LoansController : ControllerBase
{
    private readonly LoanApplicationService _service;

    public LoansController(LoanApplicationService service)
    {
        _service = service;
    }

    /// <summary>Submit a new loan application.</summary>
    [HttpPost("apply")]
    [ProducesResponseType(typeof(LoanApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Apply(
        [FromBody] LoanApplicationRequest request,
        CancellationToken ct)
    {
        var (response, validationError) = await _service.ApplyAsync(request, ct);

        if (validationError is not null)
            return BadRequest(validationError);

        return Ok(response);
    }

    /// <summary>Get paginated list of applications with optional filtering.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LoanApplicationSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? decision,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        LoanDecision? decisionFilter = null;
        if (!string.IsNullOrWhiteSpace(decision))
        {
            if (Enum.TryParse<LoanDecision>(decision, ignoreCase: true, out var parsed))
                decisionFilter = parsed;
            else
                return BadRequest(new { type = "ValidationError", message = $"Invalid decision value: '{decision}'. Use 'Approved' or 'Declined'." });
        }

        var filter = new LoanFilterParams
        {
            Decision = decisionFilter,
            From = from,
            To = to,
            Page = page,
            PageSize = pageSize
        };

        var result = await _service.GetPagedAsync(filter, ct);
        return Ok(result);
    }

    /// <summary>Get a single application by ID including full rule evaluations.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var application = await _service.GetByIdAsync(id, ct);
        if (application is null)
            return NotFound(new { type = "NotFound", message = "Application not found." });

        return Ok(application);
    }
}
