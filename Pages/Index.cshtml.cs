using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PricceCompare.Data;
using PricceCompare.Models;
using ClosedXML.Excel;

namespace PricceCompare.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<Product> Results { get; set; } = new();
    public int TotalProducts { get; set; }
    public int TotalSellers { get; set; }

    public async Task OnGetAsync()
    {
        TotalProducts = await _db.Products.CountAsync();
        TotalSellers = await _db.SellerPrices
            .Select(s => s.SellerName).Distinct().CountAsync();

        if (!string.IsNullOrWhiteSpace(Query))
        {
            Results = await _db.Products
                .Include(p => p.SellerPrices)
                .Where(p => p.Name.Contains(Query))
                .ToListAsync();
        }
    }

    public async Task<IActionResult> OnGetExportAsync(string? query)
    {
        var products = string.IsNullOrWhiteSpace(query)
            ? await _db.Products.Include(p => p.SellerPrices).ToListAsync()
            : await _db.Products.Include(p => p.SellerPrices)
                .Where(p => p.Name.Contains(query)).ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("مقارنة الأسعار");

        var sellers = products
            .SelectMany(p => p.SellerPrices)
            .Select(s => s.SellerName)
            .Distinct().OrderBy(s => s).ToList();

        // Header
        int col = 1;
        ws.Cell(1, col++).Value = "الصنف";
        ws.Cell(1, col++).Value = "الكمية";
        foreach (var seller in sellers)
            ws.Cell(1, col++).Value = seller;
        ws.Cell(1, col++).Value = "أفضل سعر";
        ws.Cell(1, col++).Value = "الفرق";

        var headerRow = ws.Range(1, 1, 1, col - 1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Font.FontColor = XLColor.White;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3a5c");
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Data
        int row = 2;
        foreach (var p in products)
        {
            int c = 1;
            ws.Cell(row, c++).Value = p.Name;
            ws.Cell(row, c++).Value = p.Quantity ?? "";

            var prices = new List<decimal>();
            foreach (var seller in sellers)
            {
                var sp = p.SellerPrices.FirstOrDefault(x => x.SellerName == seller);
                if (sp != null)
                {
                    ws.Cell(row, c).Value = (double)sp.Price;
                    ws.Cell(row, c).Style.NumberFormat.Format = "#,##0.00";
                    prices.Add(sp.Price);
                }
                c++;
            }

            if (prices.Count > 0)
            {
                decimal best = prices.Min();
                decimal worst = prices.Max();
                ws.Cell(row, c).Value = (double)best;
                ws.Cell(row, c).Style.Font.FontColor = XLColor.FromHtml("#16a34a");
                ws.Cell(row, c).Style.Font.Bold = true;
                c++;
                ws.Cell(row, c).Value = (double)(worst - best);
                ws.Cell(row, c).Style.Font.FontColor = XLColor.FromHtml("#dc2626");
            }

            if (row % 2 == 0)
                ws.Range(row, 1, row, col - 1)
                  .Style.Fill.BackgroundColor = XLColor.FromHtml("#f4f7fb");
            row++;
        }

        ws.Columns().AdjustToContents();
       

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        string fileName = $"مقارنة_الأسعار_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}