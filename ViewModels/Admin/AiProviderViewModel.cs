using System.ComponentModel.DataAnnotations;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.ViewModels.Admin;

public class AiProviderFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "نام الزامی است")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "آدرس پایه الزامی است")]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? ApiKey { get; set; }
    public bool HasApiKey { get; set; }

    public ProviderType ProviderType { get; set; } = ProviderType.OpenAiCompatible;

    // پیکربندی موجودی
    [MaxLength(500)]
    public string? BalanceUrl { get; set; }

    [MaxLength(200)]
    public string? BalanceJsonPath { get; set; }

    [MaxLength(20)]
    public string? BalanceCurrency { get; set; }
}
