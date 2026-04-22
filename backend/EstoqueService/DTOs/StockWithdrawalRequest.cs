namespace EstoqueService.DTOs;

public record StockWithdrawalRequest
{
    public List<StockWithdrawalItem> Items { get; init; } = [];
}
