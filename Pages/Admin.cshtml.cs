using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PricceCompare.Data;
using PricceCompare.Models;

namespace PricceCompare.Pages;

public class AdminModel : PageModel
{
    private readonly AppDbContext _db;
    public AdminModel(AppDbContext db) => _db = db;

    [BindProperty] public string ProductName { get; set; } = "";
    [BindProperty] public string? ProductQty { get; set; }
    [BindProperty] public int SellerProductId { get; set; }
    [BindProperty] public string SellerName { get; set; } = "";
    [BindProperty] public decimal SellerPrice { get; set; }

    public List<Product> Products { get; set; } = new();

    public async Task OnGetAsync()
    {
        Products = await _db.Products
            .Include(p => p.SellerPrices)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAddProductAsync()
    {
        if (string.IsNullOrWhiteSpace(ProductName))
        {
            TempData["Error"] = "اسم الصنف مطلوب";
            return RedirectToPage();
        }
        bool exists = await _db.Products
            .AnyAsync(p => p.Name == ProductName.Trim());
        if (exists)
        {
            TempData["Error"] = "هذا الصنف موجود مسبقاً";
            return RedirectToPage();
        }
        _db.Products.Add(new Product
        {
            Name = ProductName.Trim(),
            Quantity = ProductQty?.Trim()
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"تم إضافة الصنف \"{ProductName}\" بنجاح";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddSellerAsync()
    {
        if (SellerProductId == 0 ||
            string.IsNullOrWhiteSpace(SellerName) ||
            SellerPrice <= 0)
        {
            TempData["Error"] = "يرجى تعبئة جميع الحقول بشكل صحيح";
            return RedirectToPage();
        }
        bool exists = await _db.SellerPrices.AnyAsync(
            s => s.ProductId == SellerProductId &&
                 s.SellerName == SellerName.Trim());
        if (exists)
        {
            TempData["Error"] = "هذا البائع مضاف مسبقاً لهذا الصنف";
            return RedirectToPage();
        }
        _db.SellerPrices.Add(new SellerPrice
        {
            ProductId = SellerProductId,
            SellerName = SellerName.Trim(),
            Price = SellerPrice
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إضافة سعر البائع بنجاح";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteProductAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"تم حذف الصنف \"{product.Name}\"";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteSellerAsync(int id)
    {
        var sp = await _db.SellerPrices.FindAsync(id);
        if (sp != null)
        {
            _db.SellerPrices.Remove(sp);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم حذف سعر البائع";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditSellerAsync(int id, decimal price)
    {
        var sp = await _db.SellerPrices.FindAsync(id);
        if (sp != null && price > 0)
        {
            sp.Price = price;
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم تحديث السعر بنجاح";
        }
        return RedirectToPage();
    }
}