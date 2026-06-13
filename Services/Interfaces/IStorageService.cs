using Microsoft.AspNetCore.Http;

namespace PromptMarketPlace.Services.Interfaces;

public interface IStorageService
{
    Task<string> SaveFromUrlAsync(string url, string folder, string? bearerToken = null);
    Task<string> SaveBytesAsync(byte[] bytes, string folder, string extension);
    Task<string> SaveUploadAsync(IFormFile file, string folder);
    string GetPublicUrl(string relativePath);
    Task DeleteAsync(string relativePath);
}
