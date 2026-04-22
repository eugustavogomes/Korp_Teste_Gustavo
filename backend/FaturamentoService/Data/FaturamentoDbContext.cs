namespace FaturamentoService.Data;

public class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options)
        : base(options) { }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Number).IsRequired().HasMaxLength(50);
            entity.Property(i => i.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.Property(i => i.Subtotal).HasPrecision(18, 2);
            entity.HasOne(i => i.Invoice)
                  .WithMany(inv => inv.Items)
                  .HasForeignKey(i => i.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
