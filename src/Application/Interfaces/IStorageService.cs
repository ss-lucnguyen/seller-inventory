namespace SellerInventory.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default);
    string GetPublicUrl(string objectName);
}
