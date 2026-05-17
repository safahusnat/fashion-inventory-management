using FashionInventoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionInventoryManagement.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ClothingItem> ClothingItems => Set<ClothingItem>();
}
