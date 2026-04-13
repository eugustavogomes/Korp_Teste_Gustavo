namespace FaturamentoService.DTOs;

public record InterpretarPedidoRequest
{
    public string Texto { get; init; } = string.Empty;
    public List<ProdutoCatalogItem> Produtos { get; init; } = [];
}

public record ProdutoCatalogItem
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public int SaldoDisponivel { get; init; }
}
