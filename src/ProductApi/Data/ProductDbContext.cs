using Microsoft.EntityFrameworkCore;
using ProductApi.Products;

namespace ProductApi.Data;

public sealed class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.ToTable("products");
            product.Property(p => p.Name).HasMaxLength(ProductValidator.NameMaxLength).IsRequired();
            product.Property(p => p.Price).HasPrecision(10, 2);
        });
    }
}
