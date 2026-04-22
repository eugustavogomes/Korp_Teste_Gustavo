using EstoqueService.DTOs;
using EstoqueService.Models;

namespace EstoqueService.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(int id, Product product);
    Task DeleteAsync(int id);
    Task ReserveStockAsync(StockReservationRequest request);
    Task ReleaseReservationAsync(StockReservationRequest request);
    Task WithdrawStockAsync(StockWithdrawalRequest request);
}
