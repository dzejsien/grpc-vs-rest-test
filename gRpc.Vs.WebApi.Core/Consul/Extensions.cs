using System;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace gRpc.Vs.WebApi.Core.Consul
{
    public static class Extensions
    {
        private const string ConsulSectionName = "consul";

        public static IServiceCollection AddConsul(this IServiceCollection services)
        {
            IConfiguration configuration;

            using (var serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }

            services.Configure<ConsulConfig>(configuration.GetSection(ConsulSectionName));
            services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            
            var config = new ConsulConfig();
            configuration.GetSection(ConsulSectionName).Bind(config);

            services.AddSingleton<IConsulClient>(c => new ConsulClient(cfg =>
            {
                if (!string.IsNullOrEmpty(config.Url))
                {
                    cfg.Address = new Uri(config.Url);
                }
            }));

            return services;
        }

        // different approach - not used here - I used IHostedService
        public static string UseConsul(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var consulOptions = scope.ServiceProvider.GetService<IOptions<ConsulConfig>>();

            if (!consulOptions.Value.Enabled)
            {
                return string.Empty;
            }

            var address = consulOptions.Value.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Consul address can not be empty.", nameof(consulOptions.Value.Address));
            }

            var uniqueId = Guid.NewGuid();
            var client = scope.ServiceProvider.GetService<IConsulClient>();
            var serviceName = consulOptions.Value.Service;
            var serviceId = $"{serviceName}:{uniqueId}";
            var port = consulOptions.Value.Port;
            var healthCheckEndpoint = consulOptions.Value.HealthCheckEndpoint;
            var healthCheckInterval = consulOptions.Value.HealthCheckInterval <= 0 ? 5 : consulOptions.Value.HealthCheckInterval;
            var removeAfterInterval = consulOptions.Value.RemoveAfterInterval <= 0 ? 10 : consulOptions.Value.RemoveAfterInterval;

            var registration = new AgentServiceRegistration
            {
                Name = serviceName,
                ID = serviceId,
                Address = address,
                Port = port,
            };

            if (consulOptions.Value.HealthCheckEnabled)
            {
                var check = new AgentServiceCheck
                {
                    Interval = TimeSpan.FromSeconds(healthCheckInterval),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(removeAfterInterval),
                    HTTP = $"{address}:{port}/{healthCheckEndpoint}",
                    TLSSkipVerify = true
                };

                registration.Checks = new[] { check };
            }

            client.Agent.ServiceRegister(registration);

            return serviceId;
        }
    }
}