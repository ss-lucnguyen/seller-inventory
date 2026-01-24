using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using SellerInventory.Application.Interfaces;

namespace SellerInventory.Infrastructure.Storage;

public class GoogleCloudStorageService : IStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    private readonly string _baseUrl;

    public GoogleCloudStorageService(IConfiguration configuration)
    {
        var credentialPath = configuration["GoogleCloud:CredentialPath"];

        if (!string.IsNullOrEmpty(credentialPath) && File.Exists(credentialPath))
        {
            var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(credentialPath);
            _storageClient = StorageClient.Create(credential);
        }
        else
        {
            // Use default credentials (Application Default Credentials)
            // This works automatically in Cloud Run with the service account
            _storageClient = StorageClient.Create();
        }

        _bucketName = configuration["GoogleCloud:BucketName"] ?? "sellerinventory-images";
        _baseUrl = $"https://storage.googleapis.com/{_bucketName}";
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var objectName = $"products/{DateTime.UtcNow:yyyy/MM}/{fileName}";

        await _storageClient.UploadObjectAsync(
            _bucketName,
            objectName,
            contentType,
            imageStream,
            cancellationToken: cancellationToken
        );

        return GetPublicUrl(objectName);
    }

    public async Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        var objectName = ExtractObjectName(imageUrl);
        if (!string.IsNullOrEmpty(objectName))
        {
            try
            {
                await _storageClient.DeleteObjectAsync(_bucketName, objectName, cancellationToken: cancellationToken);
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Object doesn't exist, ignore
            }
        }
    }

    public string GetPublicUrl(string objectName)
    {
        return $"{_baseUrl}/{objectName}";
    }

    private string ExtractObjectName(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return string.Empty;
        return imageUrl.Replace($"{_baseUrl}/", "");
    }
}
