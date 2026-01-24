using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BaseService.Infraestructure.HealthCheck
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseHealthCheck(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Crear un scope para resolver el DbContext
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<Persistence.AppDbContext>();

                // Verificar conexión ejecutando una query simple
                await dbContext.Database.CanConnectAsync(cancellationToken);

                return HealthCheckResult.Healthy("Database is accessible");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Database is not accessible",
                    exception: ex);
            }
        }
    }
}
