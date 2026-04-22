using FaturamentoService.Data;
using FaturamentoService.Models;
using FaturamentoService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoService.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly FaturamentoDbContext _context;

    public InvoiceRepository(FaturamentoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
        => await _context.Invoices
            .Include(i => i.Items)
            .ToListAsync();

    public async Task<Invoice?> GetByIdAsync(int id)
        => await _context.Invoices.FindAsync(id);

    public async Task<Invoice?> GetByIdWithItemsAsync(int id)
        => await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task AddAsync(Invoice invoice)
        => await _context.Invoices.AddAsync(invoice);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
