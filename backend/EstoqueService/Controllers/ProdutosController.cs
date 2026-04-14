using Microsoft.AspNetCore.Mvc;
using EstoqueService.DTOs;
using EstoqueService.Exceptions;
using EstoqueService.Models;
using EstoqueService.Services.Interfaces;

namespace EstoqueService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(IProdutoService service, ILogger<ProdutosController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Produto>> GetProduto(int id)
    {
        var produto = await _service.GetByIdAsync(id);
        if (produto == null)
            return NotFound();
        return produto;
    }

    [HttpPost]
    public async Task<ActionResult<Produto>> CreateProduto([FromBody] Produto produto)
    {
        try
        {
            var criado = await _service.CreateAsync(produto);
            return CreatedAtAction(nameof(GetProduto), new { id = criado.Id }, criado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduto(int id, [FromBody] Produto produto)
    {
        if (id != produto.Id)
            return BadRequest("ID da rota não corresponde ao ID do produto");

        try
        {
            await _service.UpdateAsync(id, produto);
            return NoContent();
        }
        catch (ProdutoNotFoundException)
        {
            return NotFound();
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar produto {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduto(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (ProdutoNotFoundException)
        {
            return NotFound();
        }
        catch (ProdutoComReservaAtivaException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar produto {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPost("reservar-estoque")]
    public async Task<IActionResult> ReservarEstoque([FromBody] ReservaEstoqueRequest request)
    {
        try
        {
            await _service.ReservarEstoqueAsync(request);
            return Ok();
        }
        catch (ProdutoNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (SaldoInsuficienteException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reservar estoque");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPost("liberar-reserva")]
    public async Task<IActionResult> LiberarReserva([FromBody] ReservaEstoqueRequest request)
    {
        try
        {
            await _service.LiberarReservaAsync(request);
            return Ok();
        }
        catch (ProdutoNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (ReservaInsuficienteException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao liberar reserva");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPost("baixa-estoque")]
    public async Task<IActionResult> BaixarEstoque([FromBody] BaixaEstoqueRequest request)
    {
        try
        {
            await _service.BaixarEstoqueAsync(request);
            return Ok();
        }
        catch (ProdutoNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (SaldoInsuficienteException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar estoque");
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
