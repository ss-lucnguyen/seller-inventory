using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SellerInventer.Application.DTOs.Order;
using SellerInventer.Application.Interfaces;

namespace SellerInventer.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var orders = await _orderService.GetByUserAsync(userGuid, cancellationToken);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        try
        {
            var order = await _orderService.CreateAsync(dto, userGuid, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.UpdateStatusAsync(id, dto, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{orderId:guid}/items")]
    public async Task<IActionResult> AddItem(Guid orderId, [FromBody] AddOrderItemDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.AddItemAsync(orderId, dto, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid itemId, CancellationToken cancellationToken)
    {
        try
        {
            await _orderService.RemoveItemAsync(orderId, itemId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
