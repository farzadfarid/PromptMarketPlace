using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class LocalStorageService : IStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IWebHostEnvironment env, IHttpClientFactory httpFactory,
        ILogger<LocalStorageService> logger)
    {
        _env = env;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<string> SaveFromUrlAsync(string url, string folder)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadDir);

        var ext = Path.GetExtension(new Uri(url).AbsolutePath);
        if (string.IsNullOrEmpty(ext)) ext = folder switch
        {
            "images" => ".jpg",
            "audio"  => ".mp3",
            "video"  => ".mp4",
            _        => ".bin"
        };

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        var client = _httpFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(60);

        var bytes = await client.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(filePath, bytes);

        return $"/uploads/{folder}/{fileName}";
    }

    public string GetPublicUrl(string relativePath) => relativePath;

    public async Task DeleteAsync(string relativePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            await Task.Run(() => File.Delete(fullPath));
            _logger.LogInformation("Deleted file: {Path}", fullPath);
        }
    }
}
