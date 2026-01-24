using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SellerInventory.Application.DTOs.Product;
using SellerInventory.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace SellerInventory.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IStorageService _storageService;

    public ProductsController(IProductService productService, IStorageService storageService)
    {
        _productService = productService;
        _storageService = storageService;
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
    [Authorize(Roles = "Manager")]
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
    [Authorize(Roles = "Manager")]
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
    [Authorize(Roles = "Manager")]
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
    [Authorize(Roles = "Manager")]
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

    [HttpPost("import")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Import([FromBody] IEnumerable<ImportProductDto> products, CancellationToken cancellationToken)
    {
        try
        {
            var results = await _productService.ImportAsync(products, cancellationToken);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("upload-image")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Invalid file type. Allowed: jpg, jpeg, png, webp");

        if (file.Length > 10 * 1024 * 1024) // 10MB max
            return BadRequest("File size exceeds maximum of 10MB");

        try
        {
            var fileName = $"{Guid.NewGuid()}.jpg";
            var contentType = "image/jpeg";

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                using (var image = await Image.LoadAsync(memoryStream, cancellationToken))
                {
                    // Resize to max 1200x1200
                    if (image.Width > 1200 || image.Height > 1200)
                    {
                        image.Mutate(x => x.Resize(
                            new ResizeOptions
                            {
                                Size = new Size(1200, 1200),
                                Mode = ResizeMode.Max
                            }));
                    }

                    // Compress by adjusting JPEG quality
                    using (var outputStream = new MemoryStream())
                    {
                        for (int quality = 85; quality >= 10; quality -= 5)
                        {
                            outputStream.SetLength(0);
                            outputStream.Position = 0;

                            var encoder = new JpegEncoder { Quality = quality };
                            await image.SaveAsync(outputStream, encoder, cancellationToken);

                            if (outputStream.Length >= 300 * 1024 && outputStream.Length <= 500 * 1024)
                                break;
                        }

                        // Upload to Google Cloud Storage (or Local based on configuration)
                        outputStream.Position = 0;
                        var imageUrl = await _storageService.UploadImageAsync(outputStream, fileName, contentType, cancellationToken);
                        return Ok(new { imageUrl });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("import-excel")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> ImportExcel([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Invalid file type. Allowed: xlsx, xls");

        try
        {
            var products = new List<ImportProductDto>();

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                using (var workbook = new ClosedXML.Excel.XLWorkbook(memoryStream))
                {
                    var worksheet = workbook.Worksheets.First();
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

                    // Excel columns: No, ProductName, Category, SKU, SellPrice, CostPrice, InStock
                    foreach (var row in rows)
                    {
                        try
                        {
                            var product = new ImportProductDto(
                                Name: row.Cell(2).Value.ToString() ?? string.Empty,
                                Description: null,
                                SKU: row.Cell(4).Value.ToString(),
                                CostPrice: Convert.ToDecimal(row.Cell(6).Value),
                                SellPrice: Convert.ToDecimal(row.Cell(5).Value),
                                StockQuantity: Convert.ToInt32(row.Cell(7).Value),
                                CategoryName: row.Cell(3).Value.ToString() ?? string.Empty
                            );
                            products.Add(product);
                        }
                        catch
                        {
                            // Skip invalid rows
                            continue;
                        }
                    }
                }
            }

            var results = await _productService.ImportAsync(products, cancellationToken);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
