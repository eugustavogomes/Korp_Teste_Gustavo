namespace FaturamentoService.DTOs;

public record StockReservationRequest
{
    public List<StockReservationItem> Items { get; init; } = [];
}
