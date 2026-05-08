using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace EventEase_WebApp.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly IConfiguration _configuration;

    public BlobStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("The selected file is empty.");
        }

        var containerClient = await GetContainerClientAsync(cancellationToken);
        var extension = Path.GetExtension(file.FileName);
        var blobName = $"{folder}/{Guid.NewGuid():N}{extension}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(
            stream,
            new BlobHttpHeaders { ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType },
            cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteImageIfExistsAsync(string? imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || !Uri.TryCreate(imageUrl, UriKind.Absolute, out var imageUri))
        {
            return;
        }

        var connectionString = _configuration["AzureBlob:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var containerClient = await GetContainerClientAsync(cancellationToken, connectionString);
        if (!string.Equals(imageUri.Host, containerClient.Uri.Host, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var blobName = Uri.UnescapeDataString(imageUri.AbsolutePath.TrimStart('/'));
        if (blobName.StartsWith($"{containerClient.Name}/", StringComparison.OrdinalIgnoreCase))
        {
            blobName = blobName[(containerClient.Name.Length + 1)..];
        }

        if (string.IsNullOrWhiteSpace(blobName))
        {
            return;
        }

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
    {
        var connectionString = _configuration["AzureBlob:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Azure Blob Storage is not configured. Set AzureBlob:ConnectionString.");
        }
        return await GetContainerClientAsync(cancellationToken, connectionString);
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken, string connectionString)
    {
        var containerName = _configuration["AzureBlob:ContainerName"];
        if (string.IsNullOrWhiteSpace(containerName))
        {
            containerName = "eventease-images";
        }

        var containerClient = new BlobContainerClient(connectionString, containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        return containerClient;
    }
}
