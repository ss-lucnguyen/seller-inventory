using Microsoft.Extensions.Configuration;
using SellerInventory.Application.Interfaces;

namespace SellerInventory.Infrastructure.Storage;

public class LocalStorageService : IStorageService
{
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public LocalStorageService(IConfiguration configuration)
    {
        _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5000";
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var folder = Path.Combine(_uploadsPath, "products");
        Directory.CreateDirectory(folder);

        var filePath = Path.Combine(folder, fileName);
        using var fileStream = File.Create(filePath);
        await imageStream.CopyToAsync(fileStream, cancellationToken);

        return $"/uploads/products/{fileName}";
    }

    public Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith("/uploads/"))
            return Task.CompletedTask;

        var relativePath = imageUrl.TrimStart('/');
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public string GetPublicUrl(string objectName)
    {
        return $"/uploads/{objectName}";
    }
}
