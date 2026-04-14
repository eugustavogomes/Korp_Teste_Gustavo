using EstoqueService.DTOs;
using EstoqueService.Exceptions;
using EstoqueService.Models;
using EstoqueService.Repositories.Interfaces;
using EstoqueService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repository;
    private readonly ILogger<ProdutoService> _logger;

    public ProdutoService(IProdutoRepository repository, ILogger<ProdutoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<IEnumerable<Produto>> GetAllAsync()
        => _repository.GetAllAsync();

    public Task<Produto?> GetByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    public async Task<Produto> CreateAsync(Produto produto)
    {
        produto.DataCriacao = DateTime.UtcNow;
        await _repository.AddAsync(produto);
        await _repository.SaveChangesAsync();
        return produto;
    }

    public async Task UpdateAsync(int id, Produto produto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new ProdutoNotFoundException(id);

        existing.Codigo = produto.Codigo;
        existing.Descricao = produto.Descricao;
        existing.Saldo = produto.Saldo;
        existing.DataAtualizacao = DateTime.UtcNow;

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
        var produto = await _repository.GetByIdAsync(id);
        if (produto == null)
            throw new ProdutoNotFoundException(id);

        if (produto.SaldoReservado > 0)
            throw new ProdutoComReservaAtivaException(produto.Descricao, produto.SaldoReservado);

        _repository.Remove(produto);
        await _repository.SaveChangesAsync();
    }

    public async Task ReservarEstoqueAsync(ReservaEstoqueRequest request)
    {
        await using var transaction = await _repository.BeginTransactionAsync();

        try
        {
            foreach (var item in request.Itens)
            {
                var produto = await _repository.GetByIdAsync(item.ProdutoId);

                if (produto == null)
                    throw new ProdutoNotFoundException(item.ProdutoId);

                if (produto.SaldoDisponivel < item.Quantidade)
                    throw new SaldoInsuficienteException(produto.Descricao, produto.SaldoDisponivel, item.Quantidade);

                produto.SaldoReservado += item.Quantidade;
                produto.DataAtualizacao = DateTime.UtcNow;
            }

            await _repository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogError("Erro de concorrência ao reservar estoque");
            throw new ConcurrencyException();
        }
    }

    public async Task LiberarReservaAsync(ReservaEstoqueRequest request)
    {
        await using var transaction = await _repository.BeginTransactionAsync();

        try
        {
            foreach (var item in request.Itens)
            {
                var produto = await _repository.GetByIdAsync(item.ProdutoId);

                if (produto == null)
                {
                    _logger.LogWarning("Produto {ProdutoId} não encontrado ao liberar reserva — pode ter sido excluído. Ignorando item.", item.ProdutoId);
                    continue;
                }

                if (produto.SaldoReservado < item.Quantidade)
                    throw new ReservaInsuficienteException(produto.Descricao, produto.SaldoReservado, item.Quantidade);

                produto.SaldoReservado -= item.Quantidade;
                produto.DataAtualizacao = DateTime.UtcNow;
            }

            await _repository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogError("Erro de concorrência ao liberar reserva");
            throw new ConcurrencyException();
        }
    }

    public async Task BaixarEstoqueAsync(BaixaEstoqueRequest request)
    {
        await using var transaction = await _repository.BeginTransactionAsync();

        try
        {
            foreach (var item in request.Itens)
            {
                var produto = await _repository.GetByIdAsync(item.ProdutoId);

                if (produto == null)
                    throw new ProdutoNotFoundException(item.ProdutoId);

                if (produto.Saldo < item.Quantidade)
                    throw new SaldoInsuficienteException(produto.Descricao, produto.Saldo, item.Quantidade);

                // Converte a reserva em baixa física
                produto.Saldo -= item.Quantidade;
                produto.SaldoReservado = Math.Max(0, produto.SaldoReservado - item.Quantidade);
                produto.DataAtualizacao = DateTime.UtcNow;
            }

            await _repository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogError("Erro de concorrência ao baixar estoque");
            throw new ConcurrencyException();
        }
    }
}
