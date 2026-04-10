using System.Text.Json;

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

    public async Task BaixarEstoqueAsync(BaixaEstoqueRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/produtos/baixa-estoque", request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("EstoqueService retornou {Status}: {Content}", response.StatusCode, content);

            var mensagem = ExtrairMensagem(content) ?? $"Erro {(int)response.StatusCode} no EstoqueService";
            throw new EstoqueException(mensagem, (int)response.StatusCode);
        }
    }

    private static string? ExtrairMensagem(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("mensagem", out var prop))
                return prop.GetString();
            return json.Length > 0 ? json : null;
        }
        catch
        {
            return json.Length > 0 ? json : null;
        }
    }
}
