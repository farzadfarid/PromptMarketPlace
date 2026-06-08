using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Areas.Admin.Pages.Credits;

public class PackagesModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public PackagesModel(ApplicationDbContext db) => _db = db;

    public List<CreditPackage> Packages { get; set; } = new();

    [BindProperty] public PackageForm Form { get; set; } = new();

    public async Task OnGetAsync()
        => Packages = await _db.CreditPackages.OrderBy(p => p.SortOrder).ToListAsync();

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }

        if (Form.Id == 0)
        {
            _db.CreditPackages.Add(new CreditPackage
            {
                Name = Form.Name, CreditAmount = Form.CreditAmount,
                PriceRial = Form.PriceRial, IsActive = Form.IsActive,
                IsBestValue = Form.IsBestValue, SortOrder = Form.SortOrder
            });
        }
        else
        {
            var pkg = await _db.CreditPackages.FindAsync(Form.Id);
            if (pkg != null)
            {
                pkg.Name = Form.Name; pkg.CreditAmount = Form.CreditAmount;
                pkg.PriceRial = Form.PriceRial; pkg.IsActive = Form.IsActive;
                pkg.IsBestValue = Form.IsBestValue; pkg.SortOrder = Form.SortOrder;
            }
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "بسته ذخیره شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var pkg = await _db.CreditPackages.FindAsync(id);
        if (pkg != null) { pkg.IsActive = !pkg.IsActive; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public class PackageForm
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        [Range(1, 100000)] public int CreditAmount { get; set; }
        [Range(1000, 100000000)] public decimal PriceRial { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsBestValue { get; set; }
        public int SortOrder { get; set; }
    }
}
