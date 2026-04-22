namespace FaturamentoService.Models;

public class Invoice
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; }
    public DateTime IssueDate { get; set; }
    public decimal Total { get; set; }
    public List<InvoiceItem> Items { get; set; } = new();
}

public enum InvoiceStatus
{
    Open = 1,
    Closed = 2,
    Cancelled = 3
}
