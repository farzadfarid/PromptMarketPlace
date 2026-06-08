using System.ComponentModel.DataAnnotations;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.ViewModels.Admin;

public class AiModelFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "سرویس‌دهنده الزامی است")]
    public int AiProviderId { get; set; }

    [Required(ErrorMessage = "نام الزامی است")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "شناسه مدل الزامی است")]
    [MaxLength(200)]
    public string ModelId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public List<AiCapability> SelectedCapabilities { get; set; } = new();

    public decimal? CostPer1KTokens { get; set; }
    public decimal? CostPerImage { get; set; }
    public decimal? CostPerSecondVideo { get; set; }
    public int? MaxTokens { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}
