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

    [HttpPost("interpret-order")]
    public async Task<ActionResult<InterpretOrderResponse>> InterpretOrder(
        [FromBody] InterpretOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "The order text cannot be empty." });

        if (request.Products.Count == 0)
            return BadRequest(new { message = "The product catalog cannot be empty." });

        try
        {
            var result = await _geminiService.InterpretOrderAsync(request);
            return Ok(result);
        }
        catch (GeminiRateLimitException ex)
        {
            return StatusCode(429, new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not configured"))
        {
            _logger.LogError("Gemini API Key not configured");
            return StatusCode(503, new { message = "AI service not configured. Check the Gemini API Key." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interpreting order with Gemini");
            return StatusCode(500, new { message = "Error processing the order. Please try again." });
        }
    }
}
