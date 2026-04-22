using Microsoft.AspNetCore.Mvc;
using EstoqueService.DTOs;
using EstoqueService.Exceptions;
using EstoqueService.Models;
using EstoqueService.Services.Interfaces;

namespace EstoqueService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService service, ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null)
            return NotFound();
        return product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        try
        {
            var created = await _service.CreateAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
    {
        if (id != product.Id)
            return BadRequest("Route ID does not match product ID");

        try
        {
            await _service.UpdateAsync(id, product);
            return NoContent();
        }
        catch (ProductNotFoundException)
        {
            return NotFound();
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (ProductNotFoundException)
        {
            return NotFound();
        }
        catch (ProductWithActiveReservationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("reserve-stock")]
    public async Task<IActionResult> ReserveStock([FromBody] StockReservationRequest request)
    {
        try
        {
            await _service.ReserveStockAsync(request);
            return Ok();
        }
        catch (ProductNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InsufficientBalanceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("release-reservation")]
    public async Task<IActionResult> ReleaseReservation([FromBody] StockReservationRequest request)
    {
        try
        {
            await _service.ReleaseReservationAsync(request);
            return Ok();
        }
        catch (ProductNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InsufficientReservationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing reservation");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("withdraw-stock")]
    public async Task<IActionResult> WithdrawStock([FromBody] StockWithdrawalRequest request)
    {
        try
        {
            await _service.WithdrawStockAsync(request);
            return Ok();
        }
        catch (ProductNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InsufficientBalanceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing stock");
            return StatusCode(500, "Internal server error");
        }
    }
}
