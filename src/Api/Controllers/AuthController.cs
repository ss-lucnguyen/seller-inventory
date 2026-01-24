using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SellerInventory.Application.DTOs.Auth;
using SellerInventory.Application.Interfaces;

namespace SellerInventory.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITenantContext _tenantContext;

    public AuthController(IAuthService authService, ITenantContext tenantContext)
    {
        _authService = authService;
        _tenantContext = tenantContext;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(new { success = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Register a new store with owner account (public endpoint)
    /// </summary>
    [HttpPost("register-store")]
    public async Task<IActionResult> RegisterStore([FromBody] RegisterStoreRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RegisterStoreAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Invite a user to join the current store (Manager only)
    /// </summary>
    [HttpPost("invite")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> InviteUser([FromBody] InviteUserRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.CurrentStoreId.HasValue)
            {
                return BadRequest(new { message = "No store context available" });
            }

            if (!_tenantContext.CurrentUserId.HasValue)
            {
                return Unauthorized();
            }

            var result = await _authService.InviteUserAsync(
                _tenantContext.CurrentStoreId.Value,
                request,
                _tenantContext.CurrentUserId.Value,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Accept an invitation and create account (public endpoint)
    /// </summary>
    [HttpPost("accept-invitation")]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.AcceptInvitationAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Validate an invitation token (public endpoint)
    /// </summary>
    [HttpGet("validate-invitation/{token}")]
    public async Task<IActionResult> ValidateInvitation(string token, CancellationToken cancellationToken)
    {
        var result = await _authService.ValidateInvitationTokenAsync(token, cancellationToken);

        if (result == null)
        {
            return NotFound(new { message = "Invalid or expired invitation token" });
        }

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
