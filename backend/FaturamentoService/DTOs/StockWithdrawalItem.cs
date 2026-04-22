namespace FaturamentoService.DTOs;

public record StockWithdrawalItem
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}
