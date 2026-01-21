using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SellerInventer.Application.DTOs.Product;
using SellerInventer.Application.Interfaces;

namespace SellerInventer.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var products = await _productService.GetByCategoryAsync(categoryId, cancellationToken);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, dto, cancellationToken);
            return Ok(product);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _productService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int quantity, CancellationToken cancellationToken)
    {
        try
        {
            await _productService.UpdateStockAsync(id, quantity, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
