namespace FaturamentoService.DTOs;

public record StockReservationItem
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}
