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

    public async Task<InterpretOrderResponse> InterpretOrderAsync(InterpretOrderRequest request)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model  = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini API key not configured.");

        var catalog = string.Join("\n", request.Products.Select(p =>
            $"[{p.Id}] {p.Code} — {p.Description} (available: {p.AvailableBalance})"));

        var prompt = $$"""
            You are an assistant that interprets natural language orders and converts them into invoice items.
            Return ONLY valid JSON, without additional text, without markdown, without code blocks.

            Available product catalog:
            {{catalog}}

            Order: "{{request.Text}}"

            Rules:
            - Match each mentioned item to the most similar product in the catalog
            - If quantity is not mentioned, assume 1
            - If price is not mentioned, use 0
            - If a mentioned product does not exist in the catalog, include it in "notFound"
            - Never include products with availableBalance 0 if there is an alternative

            Response format:
            {
              "items": [
                { "productId": 1, "code": "PROD-001", "description": "Product name", "quantity": 5, "unitPrice": 2.50 }
              ],
              "notFound": ["description of what was not found"]
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
            _logger.LogError("Gemini returned {Status}: {Error}", response.StatusCode, error);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                throw new GeminiRateLimitException();

            throw new InvalidOperationException($"Error calling Gemini: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();

        using var doc      = JsonDocument.Parse(json);
        var responseText   = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        return JsonSerializer.Deserialize<InterpretOrderResponse>(responseText, _jsonOptions)
            ?? new InterpretOrderResponse();
    }
}
