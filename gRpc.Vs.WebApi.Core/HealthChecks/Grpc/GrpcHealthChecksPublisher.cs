using System.Threading;
using System.Threading.Tasks;
using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using  Grpc.HealthCheck;

namespace gRpc.Vs.WebApi.Core.HealthChecks.Grpc
{
    public class GrpcHealthChecksPublisher : IHealthCheckPublisher
    {
        private readonly HealthServiceImpl _healthService;

        public GrpcHealthChecksPublisher(HealthServiceImpl healthService)
        {
            _healthService = healthService;
        }

        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            // for every registered health check
            foreach (var entry in report.Entries)
            {
                var status = entry.Value.Status;
                _healthService.SetStatus(entry.Key, ResolveStatus(status));
            }

            _healthService.SetStatus(string.Empty, HealthCheckResponse.Types.ServingStatus.Serving);

            return Task.CompletedTask;
        }

        private static HealthCheckResponse.Types.ServingStatus ResolveStatus(HealthStatus status)
        {
            return status == HealthStatus.Unhealthy
                ? HealthCheckResponse.Types.ServingStatus.NotServing
                : HealthCheckResponse.Types.ServingStatus.Serving;
        }
    }
}