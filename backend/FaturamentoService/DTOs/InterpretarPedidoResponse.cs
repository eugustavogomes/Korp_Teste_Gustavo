namespace FaturamentoService.DTOs;

public record InterpretarPedidoResponse
{
    public List<ItemInterpretado> Itens { get; init; } = [];
    public List<string> NaoEncontrados { get; init; } = [];
}

public record ItemInterpretado
{
    public int ProdutoId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public int Quantidade { get; init; }
    public decimal PrecoUnitario { get; init; }
}
