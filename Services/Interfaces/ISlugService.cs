namespace PromptMarketPlace.Services.Interfaces;

public interface ISlugService
{
    string GenerateSlug(string title);
    Task<string> EnsureUniqueAsync(string slug, int? excludeAppId = null);
}
