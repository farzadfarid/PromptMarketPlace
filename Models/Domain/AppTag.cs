namespace PromptMarketPlace.Models.Domain;

public class AppTag
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public string TagName { get; set; } = string.Empty;

    public AiApp App { get; set; } = null!;
}
