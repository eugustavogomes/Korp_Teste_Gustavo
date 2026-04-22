using EstoqueService.Data;
using EstoqueService.Models;
using EstoqueService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EstoqueService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly EstoqueDbContext _context;

    public ProductRepository(EstoqueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
        => await _context.Products.ToListAsync();

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products.FindAsync(id);

    public async Task AddAsync(Product product)
        => await _context.Products.AddAsync(product);

    public void Update(Product product)
    {
        _context.Entry(product).State = EntityState.Modified;
        _context.Entry(product).Property(p => p.CreatedAt).IsModified = false;
    }

    public void Remove(Product product)
        => _context.Products.Remove(product);

    public async Task<bool> ExistsAsync(int id)
        => await _context.Products.AnyAsync(p => p.Id == id);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _context.Database.BeginTransactionAsync();
}
