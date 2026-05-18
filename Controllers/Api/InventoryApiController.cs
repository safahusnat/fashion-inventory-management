using FashionInventoryManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionInventoryManagement.Controllers.Api;

[ApiController]
[Route("api/inventory")]
public class InventoryApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InventoryApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventory()
    {
        var items = await _context.ClothingItems
            .OrderBy(i => i.Category)
            .ThenBy(i => i.Name)
            .Select(i => new
            {
                i.Id,
                i.Name,
                i.Category,
                i.Brand,
                i.Size,
                i.Price,
                i.StockQuantity,
                i.IsLowStock
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var items = await _context.ClothingItems.ToListAsync();
        return Ok(new
        {
            totalItems = items.Count,
            lowStockItems = items.Count(i => i.StockQuantity <= 5),
            totalStockUnits = items.Sum(i => i.StockQuantity),
            totalStockValue = items.Sum(i => i.Price * i.StockQuantity)
        });
    }
}
