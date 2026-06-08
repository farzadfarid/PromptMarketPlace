using System.ComponentModel.DataAnnotations;

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
}
