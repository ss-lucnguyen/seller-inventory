using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SellerInventory.Application.Interfaces;

namespace SellerInventory.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailySales([FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        // Convert to UTC if Kind is Unspecified
        DateTime reportDate;
        if (date.HasValue)
        {
            reportDate = date.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc)
                : date.Value.ToUniversalTime().Date;
        }
        else
        {
            reportDate = DateTime.UtcNow.Date;
        }

        var report = await _reportService.GetDailySalesAsync(reportDate, cancellationToken);
        return Ok(report);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSalesSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        // Convert to UTC if Kind is Unspecified
        var utcStartDate = startDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc)
            : startDate.ToUniversalTime().Date;

        var utcEndDate = endDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
            : endDate.ToUniversalTime().Date.AddDays(1).AddTicks(-1);

        if (utcStartDate > utcEndDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }

        var summary = await _reportService.GetSalesSummaryAsync(utcStartDate, utcEndDate, cancellationToken);
        return Ok(summary);
    }
}
