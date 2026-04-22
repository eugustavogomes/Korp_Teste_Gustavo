using FaturamentoService.DTOs;

namespace FaturamentoService.Services.Interfaces;

public interface IEstoqueClient
{
    Task ReserveStockAsync(StockReservationRequest request);
    Task ReleaseReservationAsync(StockReservationRequest request);
    Task WithdrawStockAsync(StockWithdrawalRequest request);
}
