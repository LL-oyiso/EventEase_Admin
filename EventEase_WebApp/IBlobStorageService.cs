using Microsoft.AspNetCore.Http;

namespace EventEase_WebApp.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    Task DeleteImageIfExistsAsync(string? imageUrl, CancellationToken cancellationToken = default);
}
