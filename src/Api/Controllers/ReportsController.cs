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
        var reportDate = date ?? DateTime.UtcNow.Date;
        var report = await _reportService.GetDailySalesAsync(reportDate, cancellationToken);
        return Ok(report);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSalesSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        if (startDate > endDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }

        var summary = await _reportService.GetSalesSummaryAsync(startDate, endDate, cancellationToken);
        return Ok(summary);
    }
}
