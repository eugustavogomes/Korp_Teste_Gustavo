using Microsoft.EntityFrameworkCore;
using EstoqueService.Models;

namespace EstoqueService.Data;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(p => p.Code).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Description).IsRequired().HasMaxLength(200);
            entity.Ignore(p => p.AvailableBalance);
            entity.UseXminAsConcurrencyToken();
        });
    }
}
