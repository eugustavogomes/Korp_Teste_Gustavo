namespace FaturamentoService.Models;

public class NotaFiscal
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public StatusNotaFiscal Status { get; set; }
    public DateTime DataEmissao { get; set; }
    public decimal Total { get; set; }
    public List<ItemNotaFiscal> Itens { get; set; } = new();
}

public enum StatusNotaFiscal
{
    Aberta = 1,
    Fechada = 2
}
