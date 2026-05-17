using FashionInventoryManagement.Data;
using FashionInventoryManagement.Models;
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

    public async Task<IActionResult> Index(string? search, string? category)
    {
        var items = _context.ClothingItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            items = items.Where(i => i.Name.Contains(search) || i.Brand.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            items = items.Where(i => i.Category == category);
        }

        ViewBag.Search = search;
        ViewBag.Category = category;
        ViewBag.TotalItems = await _context.ClothingItems.CountAsync();
        ViewBag.LowStockItems = await _context.ClothingItems.CountAsync(i => i.StockQuantity <= 5);
        ViewBag.TotalStockValue = await _context.ClothingItems.SumAsync(i => i.Price * i.StockQuantity);

        return View(await items.OrderBy(i => i.Category).ThenBy(i => i.Name).ToListAsync());
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClothingItem item)
    {
        if (!ModelState.IsValid) return View(item);
        _context.Add(item);
        await _context.SaveChangesAsync();
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
        }
        return RedirectToAction(nameof(Index));
    }
}
