using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.ErrorLogs;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IAiProviderService _providers;
    private readonly IAiService _ai;

    public IndexModel(ApplicationDbContext db, IAiProviderService providers, IAiService ai)
    {
        _db = db;
        _providers = providers;
        _ai = ai;
    }

    [BindProperty(SupportsGet = true)] public string? Search        { get; set; }
    [BindProperty(SupportsGet = true)] public string? FilterLevel   { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom    { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo      { get; set; }
    [BindProperty(SupportsGet = true)] public string? FilterResolved { get; set; }
    [BindProperty(SupportsGet = true)] public string? FilterAi      { get; set; }
    [BindProperty(SupportsGet = true)] public int Page              { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public long? OpenId          { get; set; }

    private const int PageSize = 20;

    public List<ErrorLog> Logs      { get; set; } = new();
    public int TotalCount           { get; set; }
    public int TotalPages           => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public ErrorLogStats Stats      { get; set; } = null!;
    public bool AiConfigured        { get; set; }

    public record ErrorLogStats(int Total, int Critical, int Error, int Warning, int Today, int Resolved);

    public async Task OnGetAsync()
    {
        var (_, textModel, _) = await _providers.GetActiveSetupForOutputTypeAsync(OutputType.Text);
        AiConfigured = textModel != null;

        var today = DateTime.UtcNow.Date;
        var all = _db.ErrorLogs.AsNoTracking();

        Stats = new ErrorLogStats(
            Total:    await all.CountAsync(),
            Critical: await all.CountAsync(e => e.Level == "Critical"),
            Error:    await all.CountAsync(e => e.Level == "Error"),
            Warning:  await all.CountAsync(e => e.Level == "Warning"),
            Today:    await all.CountAsync(e => e.CreatedAt >= today),
            Resolved: await all.CountAsync(e => e.IsResolved)
        );

        var q = all.AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
            q = q.Where(e => e.Message.Contains(Search)
                           || (e.RequestPath != null && e.RequestPath.Contains(Search))
                           || e.Category.Contains(Search));

        if (!string.IsNullOrWhiteSpace(FilterLevel))
            q = q.Where(e => e.Level == FilterLevel);

        if (DateFrom.HasValue)
            q = q.Where(e => e.CreatedAt >= DateFrom.Value);

        if (DateTo.HasValue)
            q = q.Where(e => e.CreatedAt < DateTo.Value.AddDays(1));

        if (FilterResolved == "true")       q = q.Where(e => e.IsResolved);
        else if (FilterResolved == "false") q = q.Where(e => !e.IsResolved);

        if (FilterAi == "true")       q = q.Where(e => e.AiAnalysis != null);
        else if (FilterAi == "false") q = q.Where(e => e.AiAnalysis == null);

        TotalCount = await q.CountAsync();
        Logs = await q
            .OrderByDescending(e => e.CreatedAt)
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAnalyzeAsync(long id)
    {
        var entry = await _db.ErrorLogs.FindAsync(id);
        if (entry == null) return NotFound();

        var (_, model, apiKey) = await _providers.GetActiveSetupForOutputTypeAsync(OutputType.Text);
        if (model != null)
        {
            var prompt = $"""
                خطای زیر را بررسی کن و پاسخ را کوتاه، فارسی و فقط در ۳ بخش بنویس:

                Level: {entry.Level}
                Category: {entry.Category}
                Message: {entry.Message}
                Path: {entry.RequestPath ?? "N/A"}
                Exception: {entry.ExceptionType ?? "none"}
                StackTrace: {(entry.StackTrace != null ? entry.StackTrace[..Math.Min(800, entry.StackTrace.Length)] : "none")}

                پاسخ:
                🔍 علت: [یک جمله]
                🔧 راه‌حل: [حداکثر ۲ جمله]
                ⚡ اقدام فوری: [یک جمله]
                """;

            var result = await _ai.RunAsync(model, apiKey, null, prompt, OutputType.Text);
            if (result.IsSuccess && !string.IsNullOrEmpty(result.Text))
            {
                entry.AiAnalysis   = result.Text;
                entry.AiAnalyzedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        return RedirectToPage("/ErrorLogs/Index", new { Page, Search, FilterLevel, DateFrom = DateFrom?.ToString("yyyy-MM-dd"), DateTo = DateTo?.ToString("yyyy-MM-dd"), FilterResolved, FilterAi, OpenId = id });
    }

    public async Task<IActionResult> OnPostResolveAsync(long id, bool isResolved)
    {
        var entry = await _db.ErrorLogs.FindAsync(id);
        if (entry == null) return NotFound();
        entry.IsResolved = isResolved;
        await _db.SaveChangesAsync();
        return RedirectToPage("/ErrorLogs/Index", new { Page, Search, FilterLevel, DateFrom = DateFrom?.ToString("yyyy-MM-dd"), DateTo = DateTo?.ToString("yyyy-MM-dd"), FilterResolved, FilterAi });
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id)
    {
        var entry = await _db.ErrorLogs.FindAsync(id);
        if (entry != null) { _db.ErrorLogs.Remove(entry); await _db.SaveChangesAsync(); }
        return RedirectToPage("/ErrorLogs/Index", new { Page, Search, FilterLevel, DateFrom = DateFrom?.ToString("yyyy-MM-dd"), DateTo = DateTo?.ToString("yyyy-MM-dd"), FilterResolved, FilterAi });
    }

    public async Task<IActionResult> OnPostClearResolvedAsync()
    {
        var resolved = await _db.ErrorLogs.Where(e => e.IsResolved).ToListAsync();
        _db.ErrorLogs.RemoveRange(resolved);
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostClearAllAsync()
    {
        await _db.ErrorLogs.ExecuteDeleteAsync();
        return RedirectToPage();
    }

    private Dictionary<string, string> CurrentFilters() => new()
    {
        ["Page"]           = Page.ToString(),
        ["Search"]         = Search ?? "",
        ["FilterLevel"]    = FilterLevel ?? "",
        ["FilterResolved"] = FilterResolved ?? "",
        ["FilterAi"]       = FilterAi ?? ""
    };
}
