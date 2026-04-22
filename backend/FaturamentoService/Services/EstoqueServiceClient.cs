using System.Text.Json;
using FaturamentoService.Exceptions;
using FaturamentoService.Services.Interfaces;

namespace FaturamentoService.Services;

public class EstoqueClient : IEstoqueClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EstoqueClient> _logger;

    public EstoqueClient(HttpClient httpClient, ILogger<EstoqueClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task ReserveStockAsync(StockReservationRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/products/reserve-stock", request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("EstoqueService returned {Status}: {Content}", response.StatusCode, content);

            var message = ExtractMessage(content) ?? $"Error {(int)response.StatusCode} from EstoqueService";
            throw new EstoqueException(message, (int)response.StatusCode);
        }
    }

    public async Task ReleaseReservationAsync(StockReservationRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/products/release-reservation", request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("EstoqueService returned {Status}: {Content}", response.StatusCode, content);

            var message = ExtractMessage(content) ?? $"Error {(int)response.StatusCode} from EstoqueService";
            throw new EstoqueException(message, (int)response.StatusCode);
        }
    }

    public async Task WithdrawStockAsync(StockWithdrawalRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/products/withdraw-stock", request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("EstoqueService returned {Status}: {Content}", response.StatusCode, content);

            var message = ExtractMessage(content) ?? $"Error {(int)response.StatusCode} from EstoqueService";
            throw new EstoqueException(message, (int)response.StatusCode);
        }
    }

    private static string? ExtractMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var prop))
                return prop.GetString();
            return json.Length > 0 ? json : null;
        }
        catch
        {
            return json.Length > 0 ? json : null;
        }
    }
}
