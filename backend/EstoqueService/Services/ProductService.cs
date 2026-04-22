using EstoqueService.DTOs;
using EstoqueService.Exceptions;
using EstoqueService.Models;
using EstoqueService.Repositories.Interfaces;
using EstoqueService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<IEnumerable<Product>> GetAllAsync()
        => _repository.GetAllAsync();

    public Task<Product?> GetByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(int id, Product product)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new ProductNotFoundException(id);

        existing.Code = product.Code;
        existing.Description = product.Description;
        existing.Balance = product.Balance;
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _repository.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            throw new ProductNotFoundException(id);

        if (product.ReservedBalance > 0)
            throw new ProductWithActiveReservationException(product.Description, product.ReservedBalance);

        _repository.Remove(product);
        await _repository.SaveChangesAsync();
    }

    public async Task ReserveStockAsync(StockReservationRequest request)
    {
        await using var transaction = await _repository.BeginTransactionAsync();

        try
        {
            foreach (var item in request.Items)
            {
                var product = await _repository.GetByIdAsync(item.ProductId);

                if (product == null)
                    throw new ProductNotFoundException(item.ProductId);

                if (product.AvailableBalance < item.Quantity)
                    throw new InsufficientBalanceException(product.Description, product.AvailableBalance, item.Quantity);

                product.ReservedBalance += item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _repository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogError("Concurrency error when reserving stock");
            throw new ConcurrencyException();
        }
    }

    public async Task ReleaseReservationAsync(StockReservationRequest request)
    {
        await using var transaction = await _repository.BeginTransactionAsync();

        try
        {
            foreach (var item in request.Items)
            {
                var product = await _repository.GetByIdAsync(item.ProductId);

                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found when releasing reservation — it may have been deleted. Ignoring item.", item.ProductId);
                    continue;
                }

                if (product.ReservedBalance < item.Quantity)
                    throw new InsufficientReservationException(product.Description, product.ReservedBalance, item.Quantity);

                product.ReservedBalance -= item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _repository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogError("Concurrency error when releasing reservation");
            throw new ConcurrencyException();
        }
    }

    public async Task WithdrawStockAsync(StockWithdrawalRequest request)
    {
        await using var transaction = await _repository.BeginTransactionAsync();

        try
        {
            foreach (var item in request.Items)
            {
                var product = await _repository.GetByIdAsync(item.ProductId);

                if (product == null)
                    throw new ProductNotFoundException(item.ProductId);

                if (product.Balance < item.Quantity)
                    throw new InsufficientBalanceException(product.Description, product.Balance, item.Quantity);

                // Converts the reservation into physical withdrawal
                product.Balance -= item.Quantity;
                product.ReservedBalance = Math.Max(0, product.ReservedBalance - item.Quantity);
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _repository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogError("Concurrency error when withdrawing stock");
            throw new ConcurrencyException();
        }
    }
}
