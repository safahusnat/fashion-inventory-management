using System.ComponentModel.DataAnnotations;

namespace FashionInventoryManagement.Models;

public class ClothingItem
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Item Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Size { get; set; } = string.Empty;

    [Range(0, 99999)]
    public decimal Price { get; set; }

    [Range(0, 10000)]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }

    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.Now;

    public bool IsLowStock => StockQuantity <= 5;
}
