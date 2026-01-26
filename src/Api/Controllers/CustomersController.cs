using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SellerInventory.Application.DTOs.Customer;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Enums;
using SellerInventory.Shared.Contracts.Customer;

namespace SellerInventory.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(cancellationToken);
        return Ok(customers.Select(MapToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }
        return Ok(MapToResponse(customer));
    }

    [HttpGet("default")]
    public async Task<IActionResult> GetDefault(CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetOrCreateDefaultAsync(cancellationToken);
        return Ok(MapToResponse(customer));
    }

    [HttpPost]
    [Authorize(Roles = "Manager,SystemAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var dto = new CreateCustomerDto(
            request.Name,
            Enum.TryParse<Gender>(request.Gender, true, out var gender) ? gender : Gender.Unknown,
            request.Mobile,
            request.Address
        );

        var customer = await _customerService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, MapToResponse(customer));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Manager,SystemAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = new UpdateCustomerDto(
                request.Name,
                Enum.TryParse<Gender>(request.Gender, true, out var gender) ? gender : Gender.Unknown,
                request.Mobile,
                request.Address,
                request.IsActive
            );

            var customer = await _customerService.UpdateAsync(id, dto, cancellationToken);
            return Ok(MapToResponse(customer));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Manager,SystemAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _customerService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static CustomerResponse MapToResponse(CustomerDto dto) =>
        new(
            dto.Id,
            dto.Name,
            dto.Gender.ToString(),
            dto.Mobile,
            dto.AccountNumber,
            dto.Address,
            dto.IsDefault,
            dto.IsActive,
            dto.CreatedAt,
            dto.UpdatedAt
        );
}
