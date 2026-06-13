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

    public async Task<string> SaveFromUrlAsync(string url, string folder, string? bearerToken = null)
    {
        // تصاویر base64 را مستقیم ذخیره کن
        if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return await SaveFromBase64Async(url, folder);

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

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(filePath, bytes);

        _logger.LogInformation("Image saved locally: {Path}", filePath);
        return $"/uploads/{folder}/{fileName}";
    }

    private async Task<string> SaveFromBase64Async(string dataUrl, string folder)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadDir);

        // data:image/png;base64,xxxx
        var commaIdx = dataUrl.IndexOf(',');
        if (commaIdx < 0) throw new ArgumentException("Invalid base64 data URL");

        var header = dataUrl[..commaIdx];  // data:image/png;base64
        var ext = header.Contains("png") ? ".png"
                : header.Contains("webp") ? ".webp"
                : header.Contains("gif") ? ".gif"
                : ".jpg";

        var base64 = dataUrl[(commaIdx + 1)..];
        var bytes = Convert.FromBase64String(base64);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);
        await File.WriteAllBytesAsync(filePath, bytes);

        _logger.LogInformation("Base64 image saved locally: {Path}", filePath);
        return $"/uploads/{folder}/{fileName}";
    }

    public async Task<string> SaveBytesAsync(byte[] bytes, string folder, string extension)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadDir);
        if (string.IsNullOrEmpty(extension)) extension = ".bin";
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDir, fileName);
        await File.WriteAllBytesAsync(filePath, bytes);
        _logger.LogInformation("Bytes saved locally: {Path}", filePath);
        return $"/uploads/{folder}/{fileName}";
    }

    public async Task<string> SaveUploadAsync(IFormFile file, string folder)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadDir);

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";

        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        _logger.LogInformation("Uploaded file saved: {Path}", filePath);
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
