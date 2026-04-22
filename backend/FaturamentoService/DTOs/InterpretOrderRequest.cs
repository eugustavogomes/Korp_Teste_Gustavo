namespace FaturamentoService.DTOs;

public record InterpretOrderRequest
{
    public string Text { get; init; } = string.Empty;
    public List<ProductCatalogItem> Products { get; init; } = [];
}

public record ProductCatalogItem
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int AvailableBalance { get; init; }
}
