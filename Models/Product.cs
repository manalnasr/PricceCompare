using System.ComponentModel.DataAnnotations;

namespace PricceCompare.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم الصنف مطلوب")]
    [Display(Name = "الصنف")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "الكمية")]
    public string? Quantity { get; set; }

    public List<SellerPrice> SellerPrices { get; set; } = new();
}