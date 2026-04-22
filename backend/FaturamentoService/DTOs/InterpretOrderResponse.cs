namespace FaturamentoService.DTOs;

public record InterpretOrderResponse
{
    public List<InterpretedItem> Items { get; init; } = [];
    public List<string> NotFound { get; init; } = [];
}

public record InterpretedItem
{
    public int ProductId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
