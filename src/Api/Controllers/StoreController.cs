using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SellerInventory.Application.DTOs.Store;
using SellerInventory.Application.Interfaces;
using SellerInventory.Shared.Contracts.Store;

namespace SellerInventory.Api.Controllers;

[Route("api/v1/store")]
[ApiController]
[Authorize]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoreController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet]
    [Authorize(Roles = "Manager,SystemAdmin")]
    public async Task<ActionResult<StoreResponse>> GetCurrentStore(CancellationToken cancellationToken)
    {
        try
        {
            var store = await _storeService.GetCurrentStoreAsync(cancellationToken);
            if (store is null)
                return NotFound("Store not found");

            return Ok(MapToResponse(store));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPut]
    [Authorize(Roles = "Manager,SystemAdmin")]
    public async Task<ActionResult<StoreResponse>> UpdateStore([FromBody] UpdateStoreRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = new UpdateStoreDto(
                request.Name,
                request.Location,
                request.Address,
                request.Industry,
                request.Description,
                request.ContactEmail,
                request.ContactPhone,
                request.Currency
            );

            var store = await _storeService.UpdateStoreAsync(dto, cancellationToken);
            return Ok(MapToResponse(store));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("logo")]
    [Authorize(Roles = "Manager,SystemAdmin")]
    public async Task<ActionResult<string>> UpdateLogo(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest("Invalid file type. Allowed: jpg, jpeg, png, webp");

        if (file.Length > 5 * 1024 * 1024) // 5MB
            return BadRequest("File size must be less than 5MB");

        try
        {
            using var stream = file.OpenReadStream();
            var logoUrl = await _storeService.UpdateLogoAsync(stream, file.FileName, cancellationToken);
            return Ok(new { logoUrl });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    private static StoreResponse MapToResponse(StoreDto dto) =>
        new(
            dto.Id,
            dto.Name,
            dto.Slug,
            dto.Location,
            dto.Address,
            dto.Industry,
            dto.LogoUrl,
            dto.Description,
            dto.ContactEmail,
            dto.ContactPhone,
            dto.Currency,
            dto.IsActive,
            dto.CreatedAt,
            dto.UpdatedAt
        );
}
