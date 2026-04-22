using FaturamentoService.DTOs;
using FaturamentoService.Models;

namespace FaturamentoService.Services.Interfaces;

public interface IInvoiceService
{
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(int id);
    Task<Invoice> IssueAsync(IssueInvoiceRequest request);
    Task PrintAsync(int id);
    Task CancelAsync(int id);
}
