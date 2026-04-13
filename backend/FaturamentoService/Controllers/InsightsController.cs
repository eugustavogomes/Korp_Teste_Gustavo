using FaturamentoService.DTOs;
using FaturamentoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FaturamentoService.Controllers;

[ApiController]
[Route("api/insights")]
public class InsightsController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<InsightsController> _logger;

    public InsightsController(IGeminiService geminiService, ILogger<InsightsController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    [HttpPost("interpretar-pedido")]
    public async Task<ActionResult<InterpretarPedidoResponse>> InterpretarPedido(
        [FromBody] InterpretarPedidoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
            return BadRequest(new { mensagem = "O texto do pedido não pode ser vazio." });

        if (request.Produtos.Count == 0)
            return BadRequest(new { mensagem = "O catálogo de produtos não pode ser vazio." });

        try
        {
            var resultado = await _geminiService.InterpretarPedidoAsync(request);
            return Ok(resultado);
        }
        catch (GeminiRateLimitException ex)
        {
            return StatusCode(429, new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("não configurada"))
        {
            _logger.LogError("API Key do Gemini não configurada");
            return StatusCode(503, new { mensagem = "Serviço de IA não configurado. Verifique a API Key do Gemini." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao interpretar pedido com Gemini");
            return StatusCode(500, new { mensagem = "Erro ao processar o pedido. Tente novamente." });
        }
    }
}
