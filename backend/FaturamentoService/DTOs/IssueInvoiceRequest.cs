namespace FaturamentoService.DTOs;

public record IssueInvoiceRequest
{
    public List<InvoiceItemRequest> Items { get; init; } = [];
}
