namespace FaturamentoService.Data;

public class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options)
        : base(options) { }

    public DbSet<NotaFiscal> NotasFiscais { get; set; }
    public DbSet<ItemNotaFiscal> ItensNotaFiscal { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscal>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Numero).IsRequired().HasMaxLength(50);
            entity.Property(n => n.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ItemNotaFiscal>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.PrecoUnitario).HasPrecision(18, 2);
            entity.Property(i => i.Subtotal).HasPrecision(18, 2);
            entity.HasOne(i => i.NotaFiscal)
                  .WithMany(n => n.Itens)
                  .HasForeignKey(i => i.NotaFiscalId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
