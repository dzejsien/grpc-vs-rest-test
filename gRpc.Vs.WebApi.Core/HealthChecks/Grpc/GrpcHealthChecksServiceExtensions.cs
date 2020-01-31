using System;
using Grpc.HealthCheck;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace gRpc.Vs.WebApi.Core.HealthChecks.Grpc
{
    /// <summary>
    /// Extension methods for the gRPC health checks services.
    /// </summary>
    public static class GrpcHealthChecksServiceExtensions
    {
        /// <summary>
        /// Adds gRPC health check services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
        /// <returns>An instance of <see cref="IHealthChecksBuilder"/> from which health checks can be registered.</returns>
        public static IHealthChecksBuilder AddGrpcHealthChecks(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // HealthServiceImpl is designed to be a singleton
            services.TryAddSingleton<HealthServiceImpl>();
            // to uphold many impl of IHealthCheckPublisher
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheckPublisher, GrpcHealthChecksPublisher>());
            // add default heath check which will be called by publisher
            return services.AddHealthChecks();
        }
    }
}