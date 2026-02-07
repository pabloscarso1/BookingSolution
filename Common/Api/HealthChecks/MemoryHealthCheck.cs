using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Common.Api.HealthChecks
{
    public class MemoryHealthCheck : IHealthCheck
    {
        private const long WarningThresholdInBytes = 1024L * 1024L * 1024L; // 1 GB
        private const long UnhealthyThresholdInBytes = 2048L * 1024L * 1024L; // 2 GB

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var process = Process.GetCurrentProcess();
            var allocatedBytes = process.WorkingSet64;
            var allocatedMB = allocatedBytes / 1024 / 1024;

            var data = new Dictionary<string, object>
            {
                { "AllocatedMB", allocatedMB },
                { "AllocatedBytes", allocatedBytes }
            };

            if (allocatedBytes >= UnhealthyThresholdInBytes)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        $"Memory usage is critical: {allocatedMB} MB",
                        data: data));
            }

            if (allocatedBytes >= WarningThresholdInBytes)
            {
                return Task.FromResult(
                    HealthCheckResult.Degraded(
                        $"Memory usage is high: {allocatedMB} MB",
                        data: data));
            }

            return Task.FromResult(
                HealthCheckResult.Healthy(
                    $"Memory usage is normal: {allocatedMB} MB",
                    data: data));
        }
    }
}
