using FaturamentoService.DTOs;
using FaturamentoService.Exceptions;
using FaturamentoService.Models;
using FaturamentoService.Services;
using FaturamentoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FaturamentoService.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(
        IInvoiceService service,
        ILogger<InvoicesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Invoice>> GetInvoice(int id)
    {
        var invoice = await _service.GetByIdAsync(id);
        if (invoice == null)
            return NotFound();
        return invoice;
    }

    [HttpPost]
    public async Task<ActionResult<Invoice>> IssueInvoice([FromBody] IssueInvoiceRequest request)
    {
        try
        {
            var invoice = await _service.IssueAsync(request);
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
        }
        catch (EstoqueException ex) when (ex.StatusCode == 400)
        {
            _logger.LogWarning("EstoqueService rejected the reservation: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (EstoqueException ex) when (ex.StatusCode == 404)
        {
            _logger.LogWarning("Product not found in inventory: {Message}", ex.Message);
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (EstoqueException ex)
        {
            _logger.LogError("EstoqueService returned {Status}: {Message}", ex.StatusCode, ex.Message);
            return StatusCode(503, new { message = "EstoqueService unavailable. Try again in a moment." });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection failure with EstoqueService when issuing invoice");
            return StatusCode(503, new { message = "EstoqueService unavailable. Try again in a moment." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error issuing invoice");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpPost("{id}/print")]
    public async Task<IActionResult> PrintInvoice(int id)
    {
        try
        {
            await _service.PrintAsync(id);
            return NoContent();
        }
        catch (InvoiceNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidStatusException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (EstoqueException ex) when (ex.StatusCode == 400)
        {
            _logger.LogWarning("EstoqueService rejected the withdrawal for invoice {Id}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (EstoqueException ex) when (ex.StatusCode == 404)
        {
            _logger.LogWarning("Product not found in inventory when printing invoice {Id}: {Message}", id, ex.Message);
            return UnprocessableEntity(new { message = "One or more products in this invoice have been removed from the catalog and cannot have their stock withdrawn. Cancel the invoice or contact the administrator." });
        }
        catch (EstoqueException ex)
        {
            _logger.LogError("EstoqueService returned {Status} for invoice {Id}: {Message}", ex.StatusCode, id, ex.Message);
            return StatusCode(503, new { message = "EstoqueService unavailable. Try again in a moment." });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection failure with EstoqueService when printing invoice {Id}", id);
            return StatusCode(503, new { message = "EstoqueService unavailable. Try again in a moment." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing invoice {Id}", id);
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelInvoice(int id)
    {
        try
        {
            await _service.CancelAsync(id);
            return NoContent();
        }
        catch (InvoiceNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidStatusException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (EstoqueException ex)
        {
            _logger.LogError("EstoqueService returned {Status} when cancelling invoice {Id}: {Message}", ex.StatusCode, id, ex.Message);
            return StatusCode(503, new { message = "EstoqueService unavailable. Try again in a moment." });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection failure with EstoqueService when cancelling invoice {Id}", id);
            return StatusCode(503, new { message = "EstoqueService unavailable. Try again in a moment." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invoice {Id}", id);
            return StatusCode(500, new { message = "Internal server error." });
        }
    }
}
