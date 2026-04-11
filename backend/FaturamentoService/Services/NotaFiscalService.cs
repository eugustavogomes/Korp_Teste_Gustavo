using FaturamentoService.DTOs;
using FaturamentoService.Exceptions;
using FaturamentoService.Models;
using FaturamentoService.Repositories.Interfaces;
using FaturamentoService.Services.Interfaces;

namespace FaturamentoService.Services;

public class NotaFiscalService : INotaFiscalService
{
    private readonly INotaFiscalRepository _repository;
    private readonly IEstoqueClient _estoqueClient;
    private readonly ILogger<NotaFiscalService> _logger;

    public NotaFiscalService(
        INotaFiscalRepository repository,
        IEstoqueClient estoqueClient,
        ILogger<NotaFiscalService> logger)
    {
        _repository = repository;
        _estoqueClient = estoqueClient;
        _logger = logger;
    }

    public Task<IEnumerable<NotaFiscal>> GetAllAsync()
        => _repository.GetAllAsync();

    public Task<NotaFiscal?> GetByIdAsync(int id)
        => _repository.GetByIdWithItensAsync(id);

    public async Task<NotaFiscal> EmitirAsync(EmitirNotaFiscalRequest request)
    {
        var itens = request.Itens.Select(i => new ItemNotaFiscal
        {
            ProdutoId = i.ProdutoId,
            Descricao = i.Descricao,
            Quantidade = i.Quantidade,
            PrecoUnitario = i.PrecoUnitario,
            Subtotal = i.Quantidade * i.PrecoUnitario
        }).ToList();

        var notaFiscal = new NotaFiscal
        {
            Numero = string.Empty,
            DataEmissao = DateTime.UtcNow,
            Status = StatusNotaFiscal.Aberta,
            Total = itens.Sum(i => i.Subtotal),
            Itens = itens
        };

        await _repository.AddAsync(notaFiscal);
        await _repository.SaveChangesAsync();

        notaFiscal.Numero = $"NF-{notaFiscal.Id:D6}";
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Nota fiscal {Numero} criada com status Aberta. Total: {Total}", notaFiscal.Numero, notaFiscal.Total);

        return notaFiscal;
    }

    public async Task ImprimirAsync(int id)
    {
        var notaFiscal = await _repository.GetByIdWithItensAsync(id);
        if (notaFiscal == null)
            throw new NotaFiscalNotFoundException(id);

        if (notaFiscal.Status != StatusNotaFiscal.Aberta)
            throw new StatusInvalidoException("Apenas notas com status Aberta podem ser impressas.");

        var baixaRequest = new BaixaEstoqueRequest
        {
            Itens = notaFiscal.Itens.Select(i => new ItemBaixaEstoque
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade
            }).ToList()
        };

        await _estoqueClient.BaixarEstoqueAsync(baixaRequest);

        notaFiscal.Status = StatusNotaFiscal.Fechada;
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Nota fiscal {Numero} impressa. Status atualizado para Fechada.", notaFiscal.Numero);
    }

    public async Task CancelarAsync(int id)
    {
        var notaFiscal = await _repository.GetByIdAsync(id);
        if (notaFiscal == null)
            throw new NotaFiscalNotFoundException(id);

        if (notaFiscal.Status == StatusNotaFiscal.Fechada)
            throw new StatusInvalidoException("Nota fiscal já está fechada.");

        notaFiscal.Status = StatusNotaFiscal.Fechada;
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Nota fiscal {Id} cancelada.", id);
    }
}
