using FaturamentoService.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FaturamentoService.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly FaturamentoDbContext _db;

    public DatabaseHealthCheck(FaturamentoDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _db.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Banco de dados acessível.")
                : HealthCheckResult.Unhealthy("Não foi possível conectar ao banco de dados.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Erro ao verificar banco de dados.", ex);
        }
    }
}
