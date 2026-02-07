using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiGateway.Api.HealthChecks;

/// <summary>
/// Health check para validar la disponibilidad del API Gateway
/// </summary>
public class ApiGatewayHealthCheck : IHealthCheck
{
    private readonly ILogger<ApiGatewayHealthCheck> _logger;

    public ApiGatewayHealthCheck(ILogger<ApiGatewayHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Realizando health check del API Gateway");
            return Task.FromResult(HealthCheckResult.Healthy("API Gateway is operational"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check del API Gateway falló");
            return Task.FromResult(HealthCheckResult.Unhealthy("API Gateway health check failed", ex));
        }
    }
}
