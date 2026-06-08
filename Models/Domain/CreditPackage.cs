namespace PromptMarketPlace.Models.Domain;

public class CreditPackage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CreditAmount { get; set; }
    public decimal PriceRial { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsBestValue { get; set; }
    public int SortOrder { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
