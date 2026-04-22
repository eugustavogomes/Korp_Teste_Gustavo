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
                ? HealthCheckResult.Healthy("Database accessible.")
                : HealthCheckResult.Unhealthy("Unable to connect to database.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error checking database.", ex);
        }
    }
}
