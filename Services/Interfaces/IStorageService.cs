namespace PromptMarketPlace.Services.Interfaces;

public interface IStorageService
{
    Task<string> SaveFromUrlAsync(string url, string folder);
    string GetPublicUrl(string relativePath);
    Task DeleteAsync(string relativePath);
}
