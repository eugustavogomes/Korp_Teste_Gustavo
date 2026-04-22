namespace FaturamentoService.DTOs;

public record InvoiceItemRequest
{
    public int ProductId { get; init; }
    public string Description { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
