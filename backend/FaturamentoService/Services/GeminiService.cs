using System.Text;
using System.Text.Json;
using FaturamentoService.DTOs;
using FaturamentoService.Services.Interfaces;

namespace FaturamentoService.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<InterpretarPedidoResponse> InterpretarPedidoAsync(InterpretarPedidoRequest request)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model  = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Chave da API do Gemini não configurada.");

        var catalogo = string.Join("\n", request.Produtos.Select(p =>
            $"[{p.Id}] {p.Codigo} — {p.Descricao} (disponível: {p.SaldoDisponivel})"));

        var prompt = $$"""
            Você é um assistente que interpreta pedidos em linguagem natural e os converte em itens de nota fiscal.
            Retorne APENAS JSON válido, sem texto adicional, sem markdown, sem blocos de código.

            Catálogo de produtos disponíveis:
            {{catalogo}}

            Pedido: "{{request.Texto}}"

            Regras:
            - Associe cada item mencionado ao produto mais similar do catálogo
            - Se a quantidade não for mencionada, assuma 1
            - Se o preço não for mencionado, use 0
            - Se um produto mencionado não existir no catálogo, inclua em "naoEncontrados"
            - Nunca inclua produtos com saldoDisponivel 0 se houver alternativa

            Formato de resposta:
            {
              "itens": [
                { "produtoId": 1, "codigo": "PROD-001", "descricao": "Nome do produto", "quantidade": 5, "precoUnitario": 2.50 }
              ],
              "naoEncontrados": ["descrição do que não foi encontrado"]
            }
            """;

        var body = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.1
            }
        };

        var url      = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
        var content  = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Gemini retornou {Status}: {Error}", response.StatusCode, error);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                throw new GeminiRateLimitException();

            throw new InvalidOperationException($"Erro ao chamar o Gemini: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();

        using var doc      = JsonDocument.Parse(json);
        var responseText   = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        return JsonSerializer.Deserialize<InterpretarPedidoResponse>(responseText, _jsonOptions)
            ?? new InterpretarPedidoResponse();
    }
}
