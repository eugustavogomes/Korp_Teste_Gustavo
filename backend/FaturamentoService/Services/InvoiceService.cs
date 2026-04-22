using FaturamentoService.DTOs;
using FaturamentoService.Exceptions;
using FaturamentoService.Models;
using FaturamentoService.Repositories.Interfaces;
using FaturamentoService.Services.Interfaces;

namespace FaturamentoService.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repository;
    private readonly IEstoqueClient _estoqueClient;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IInvoiceRepository repository,
        IEstoqueClient estoqueClient,
        ILogger<InvoiceService> logger)
    {
        _repository = repository;
        _estoqueClient = estoqueClient;
        _logger = logger;
    }

    public Task<IEnumerable<Invoice>> GetAllAsync()
        => _repository.GetAllAsync();

    public Task<Invoice?> GetByIdAsync(int id)
        => _repository.GetByIdWithItemsAsync(id);

    public async Task<Invoice> IssueAsync(IssueInvoiceRequest request)
    {
        var items = request.Items.Select(i => new InvoiceItem
        {
            ProductId = i.ProductId,
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Subtotal = i.Quantity * i.UnitPrice
        }).ToList();

        var reservationRequest = new StockReservationRequest
        {
            Items = items.Select(i => new StockReservationItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        await _estoqueClient.ReserveStockAsync(reservationRequest);

        var invoice = new Invoice
        {
            Number = string.Empty,
            IssueDate = DateTime.UtcNow,
            Status = InvoiceStatus.Open,
            Total = items.Sum(i => i.Subtotal),
            Items = items
        };

        await _repository.AddAsync(invoice);
        await _repository.SaveChangesAsync();

        invoice.Number = $"INV-{invoice.Id:D6}";
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Invoice {Number} created with status Open. Total: {Total}", invoice.Number, invoice.Total);

        return invoice;
    }

    public async Task PrintAsync(int id)
    {
        var invoice = await _repository.GetByIdWithItemsAsync(id);
        if (invoice == null)
            throw new InvoiceNotFoundException(id);

        if (invoice.Status != InvoiceStatus.Open)
            throw new InvalidStatusException("Only invoices with status Open can be printed.");

        var withdrawalRequest = new StockWithdrawalRequest
        {
            Items = invoice.Items.Select(i => new StockWithdrawalItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        await _estoqueClient.WithdrawStockAsync(withdrawalRequest);

        invoice.Status = InvoiceStatus.Closed;
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Invoice {Number} printed. Status updated to Closed.", invoice.Number);
    }

    public async Task CancelAsync(int id)
    {
        var invoice = await _repository.GetByIdWithItemsAsync(id);
        if (invoice == null)
            throw new InvoiceNotFoundException(id);

        if (invoice.Status != InvoiceStatus.Open)
            throw new InvalidStatusException("Only invoices with status Open can be cancelled.");

        var releaseRequest = new StockReservationRequest
        {
            Items = invoice.Items.Select(i => new StockReservationItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        await _estoqueClient.ReleaseReservationAsync(releaseRequest);

        invoice.Status = InvoiceStatus.Cancelled;
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Invoice {Id} cancelled. Stock reservation released.", id);
    }
}
