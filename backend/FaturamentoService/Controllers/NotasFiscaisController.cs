namespace FaturamentoService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotasFiscaisController : ControllerBase
{
    private readonly FaturamentoDbContext _context;
    private readonly IEstoqueClient _estoqueClient;
    private readonly ILogger<NotasFiscaisController> _logger;

    public NotasFiscaisController(
        FaturamentoDbContext context,
        IEstoqueClient estoqueClient,
        ILogger<NotasFiscaisController> logger)
    {
        _context = context;
        _estoqueClient = estoqueClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotaFiscal>>> GetNotasFiscais()
    {
        return await _context.NotasFiscais
            .Include(n => n.Itens)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NotaFiscal>> GetNotaFiscal(int id)
    {
        var notaFiscal = await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notaFiscal == null)
            return NotFound();

        return notaFiscal;
    }

    [HttpPost]
    public async Task<ActionResult<NotaFiscal>> EmitirNotaFiscal([FromBody] EmitirNotaFiscalRequest request)
    {
        try
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
                Numero = $"NF-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                DataEmissao = DateTime.UtcNow,
                Status = StatusNotaFiscal.Aberta,
                Total = itens.Sum(i => i.Subtotal),
                Itens = itens
            };

            _context.NotasFiscais.Add(notaFiscal);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nota fiscal {Numero} criada com status Aberta. Total: {Total}", notaFiscal.Numero, notaFiscal.Total);

            return CreatedAtAction(nameof(GetNotaFiscal), new { id = notaFiscal.Id }, notaFiscal);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha na comunicação com EstoqueService");
            return StatusCode(503, "EstoqueService indisponível. Tente novamente em instantes.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir nota fiscal");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPost("{id}/imprimir")]
    public async Task<IActionResult> ImprimirNotaFiscal(int id)
    {
        try
        {
            var notaFiscal = await _context.NotasFiscais
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notaFiscal == null)
                return NotFound();

            if (notaFiscal.Status != StatusNotaFiscal.Aberta)
                return BadRequest(new { mensagem = "Apenas notas com status Aberta podem ser impressas." });

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
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nota fiscal {Numero} impressa. Status atualizado para Fechada.", notaFiscal.Numero);

            return NoContent();
        }
        catch (EstoqueException ex) when (ex.StatusCode == 400)
        {
            _logger.LogWarning("EstoqueService rejeitou a baixa para nota {Id}: {Mensagem}", id, ex.Message);
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (EstoqueException ex)
        {
            _logger.LogError("EstoqueService retornou {Status} para nota {Id}: {Mensagem}", ex.StatusCode, id, ex.Message);
            return StatusCode(503, new { mensagem = "EstoqueService indisponível. Tente novamente em instantes." });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha de conexão com EstoqueService ao imprimir nota {Id}", id);
            return StatusCode(503, new { mensagem = "EstoqueService indisponível. Tente novamente em instantes." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao imprimir nota fiscal {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor." });
        }
    }

    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> CancelarNotaFiscal(int id)
    {
        try
        {
            var notaFiscal = await _context.NotasFiscais.FindAsync(id);
            if (notaFiscal == null)
                return NotFound();

            if (notaFiscal.Status == StatusNotaFiscal.Fechada)
                return BadRequest("Nota fiscal já está fechada");

            notaFiscal.Status = StatusNotaFiscal.Fechada;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nota fiscal {Numero} cancelada", notaFiscal.Numero);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar nota fiscal {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
