using System.Text;
using FashionInventoryManagement.Data;
using FashionInventoryManagement.Models;
using FashionInventoryManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionInventoryManagement.Controllers;

public class InventoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? category, string? stockStatus)
    {
        var query = _context.ClothingItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Name.Contains(search) || i.Brand.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(i => i.Category == category);
        }

        if (stockStatus == "low")
        {
            query = query.Where(i => i.StockQuantity <= 5);
        }
        else if (stockStatus == "healthy")
        {
            query = query.Where(i => i.StockQuantity > 5);
        }

        var allItems = await _context.ClothingItems.ToListAsync();

        var viewModel = new InventoryDashboardViewModel
        {
            Items = await query.OrderBy(i => i.Category).ThenBy(i => i.Name).ToListAsync(),
            Search = search,
            Category = category,
            StockStatus = stockStatus,
            TotalItems = allItems.Count,
            LowStockItems = allItems.Count(i => i.StockQuantity <= 5),
            TotalStockUnits = allItems.Sum(i => i.StockQuantity),
            TotalStockValue = allItems.Sum(i => i.Price * i.StockQuantity),
            Categories = allItems.Select(i => i.Category).Distinct().OrderBy(c => c).ToList(),
            CategoryBreakdown = allItems
                .GroupBy(i => i.Category)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.StockQuantity))
        };

        return View(viewModel);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClothingItem item)
    {
        if (!ModelState.IsValid) return View(item);
        item.DateAdded = DateTime.Now;
        _context.Add(item);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Inventory item added successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.ClothingItems.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClothingItem item)
    {
        if (id != item.Id) return NotFound();
        if (!ModelState.IsValid) return View(item);
        _context.Update(item);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Inventory item updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.ClothingItems.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.ClothingItems.FindAsync(id);
        if (item != null)
        {
            _context.ClothingItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Inventory item deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportCsv()
    {
        var items = await _context.ClothingItems.OrderBy(i => i.Category).ThenBy(i => i.Name).ToListAsync();
        var csv = new StringBuilder();
        csv.AppendLine("Name,Category,Brand,Size,Price,StockQuantity,DateAdded");

        foreach (var item in items)
        {
            csv.AppendLine($"\"{item.Name}\",\"{item.Category}\",\"{item.Brand}\",\"{item.Size}\",{item.Price},{item.StockQuantity},{item.DateAdded:yyyy-MM-dd}");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "fashion-inventory-export.csv");
    }
}
