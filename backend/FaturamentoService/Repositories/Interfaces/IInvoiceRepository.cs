using FaturamentoService.Models;

namespace FaturamentoService.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(int id);
    Task<Invoice?> GetByIdWithItemsAsync(int id);
    Task AddAsync(Invoice invoice);
    Task SaveChangesAsync();
}
