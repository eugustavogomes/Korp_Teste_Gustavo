using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FaturamentoService.HealthChecks;

public class EstoqueServiceHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public EstoqueServiceHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _configuration["EstoqueService:BaseUrl"] ?? "http://localhost:5189";
            var client = _httpClientFactory.CreateClient("HealthCheck");
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetAsync($"{baseUrl}/health", cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("EstoqueService accessible.")
                : HealthCheckResult.Degraded($"EstoqueService returned HTTP {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("EstoqueService inaccessible.", ex);
        }
    }
}
