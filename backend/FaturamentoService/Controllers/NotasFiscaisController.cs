using FaturamentoService.DTOs;
using FaturamentoService.Exceptions;
using FaturamentoService.Models;
using FaturamentoService.Services;
using FaturamentoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FaturamentoService.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
public class NotasFiscaisController : ControllerBase
{
    private readonly INotaFiscalService _service;
    private readonly ILogger<NotasFiscaisController> _logger;

    public NotasFiscaisController(
        INotaFiscalService service,
        ILogger<NotasFiscaisController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotaFiscal>>> GetNotasFiscais()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<NotaFiscal>> GetNotaFiscal(int id)
    {
        var notaFiscal = await _service.GetByIdAsync(id);
        if (notaFiscal == null)
            return NotFound();
        return notaFiscal;
    }

    [HttpPost]
    public async Task<ActionResult<NotaFiscal>> EmitirNotaFiscal([FromBody] EmitirNotaFiscalRequest request)
    {
        try
        {
            var notaFiscal = await _service.EmitirAsync(request);
            return CreatedAtAction(nameof(GetNotaFiscal), new { id = notaFiscal.Id }, notaFiscal);
        }
        catch (EstoqueException ex) when (ex.StatusCode == 400)
        {
            _logger.LogWarning("EstoqueService rejeitou a reserva: {Mensagem}", ex.Message);
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (EstoqueException ex) when (ex.StatusCode == 404)
        {
            _logger.LogWarning("Produto não encontrado no estoque: {Mensagem}", ex.Message);
            return UnprocessableEntity(new { mensagem = ex.Message });
        }
        catch (EstoqueException ex)
        {
            _logger.LogError("EstoqueService retornou {Status}: {Mensagem}", ex.StatusCode, ex.Message);
            return StatusCode(503, new { mensagem = "EstoqueService indisponível. Tente novamente em instantes." });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha de conexão com EstoqueService ao emitir nota");
            return StatusCode(503, new { mensagem = "EstoqueService indisponível. Tente novamente em instantes." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir nota fiscal");
            return StatusCode(500, new { mensagem = "Erro interno do servidor." });
        }
    }

    [HttpPost("{id}/imprimir")]
    public async Task<IActionResult> ImprimirNotaFiscal(int id)
    {
        try
        {
            await _service.ImprimirAsync(id);
            return NoContent();
        }
        catch (NotaFiscalNotFoundException)
        {
            return NotFound();
        }
        catch (StatusInvalidoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (EstoqueException ex) when (ex.StatusCode == 400)
        {
            _logger.LogWarning("EstoqueService rejeitou a baixa para nota {Id}: {Mensagem}", id, ex.Message);
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (EstoqueException ex) when (ex.StatusCode == 404)
        {
            _logger.LogWarning("Produto não encontrado no estoque ao imprimir nota {Id}: {Mensagem}", id, ex.Message);
            return UnprocessableEntity(new { mensagem = "Um ou mais produtos desta nota foram removidos do cadastro e não podem ter o estoque baixado. Cancele a nota ou entre em contato com o administrador." });
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
            await _service.CancelarAsync(id);
            return NoContent();
        }
        catch (NotaFiscalNotFoundException)
        {
            return NotFound();
        }
        catch (StatusInvalidoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (EstoqueException ex)
        {
            _logger.LogError("EstoqueService retornou {Status} ao cancelar nota {Id}: {Mensagem}", ex.StatusCode, id, ex.Message);
            return StatusCode(503, new { mensagem = "EstoqueService indisponível. Tente novamente em instantes." });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha de conexão com EstoqueService ao cancelar nota {Id}", id);
            return StatusCode(503, new { mensagem = "EstoqueService indisponível. Tente novamente em instantes." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar nota fiscal {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor." });
        }
    }
}
