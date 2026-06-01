using System.ComponentModel.DataAnnotations;

namespace PricceCompare.Models;

public class SellerPrice
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم البائع مطلوب")]
    [Display(Name = "البائع")]
    public string SellerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "السعر مطلوب")]
    [Display(Name = "السعر")]
    [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون موجباً")]
    public decimal Price { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}