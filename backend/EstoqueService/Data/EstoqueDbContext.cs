using Microsoft.EntityFrameworkCore;
using EstoqueService.Models;

namespace EstoqueService.Data;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) 
        : base(options) { }
    
    public DbSet<Produto> Produtos { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.Property(p => p.Codigo).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Descricao).IsRequired().HasMaxLength(200);
            entity.UseXminAsConcurrencyToken();
        });
    }
}