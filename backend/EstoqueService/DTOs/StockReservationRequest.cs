namespace EstoqueService.DTOs;

public record StockReservationRequest
{
    public List<StockReservationItem> Items { get; init; } = [];
}
