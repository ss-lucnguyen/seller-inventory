using Microsoft.AspNetCore.Mvc;
using SellerInventory.Infrastructure.Data;

namespace SellerInventory.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Check database connectivity
            await _context.Database.CanConnectAsync();
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return Ok(new { status = "ready" });
        }
        catch
        {
            return StatusCode(503, new { status = "not ready" });
        }
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { status = "alive" });
    }
}
