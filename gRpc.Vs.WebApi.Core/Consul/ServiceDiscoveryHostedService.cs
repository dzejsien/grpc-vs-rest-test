using System;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace gRpc.Vs.WebApi.Core.Consul
{
    public class ServiceDiscoveryHostedService : IHostedService
    {
        private readonly IConsulClient _client;
        private readonly ConsulConfig _config;
        private string _registrationId;

        public ServiceDiscoveryHostedService(IConsulClient client, IOptions<ConsulConfig> config)
        {
            _client = client;
            _config = config.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _registrationId = $"{_config.Service}-{Guid.NewGuid()}";
            var registration = new AgentServiceRegistration
            {
                ID = _registrationId,
                Name = _config.Service,
                Address = _config.Address,
                Port = _config.Port
            };

            if (_config.HealthCheckEnabled)
            {
                var healthCheckEndpoint = _config.HealthCheckEndpoint;
                var healthCheckInterval = _config.HealthCheckInterval <= 0 ? 5 : _config.HealthCheckInterval;
                var removeAfterInterval = _config.RemoveAfterInterval <= 0 ? 10 : _config.RemoveAfterInterval;

                var tlsSkipVerify = _config.Address.StartsWith("https", StringComparison.InvariantCultureIgnoreCase);

                var check = new AgentCheckRegistration()
                {
                    Interval = TimeSpan.FromSeconds(healthCheckInterval),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(removeAfterInterval),
                    HTTP = $"{_config.Address}:{_config.Port}{healthCheckEndpoint}",
                    TLSSkipVerify = tlsSkipVerify // if is hosted as tls
                };

                registration.Checks = new AgentServiceCheck[] { check };
            }

            await _client.Agent.ServiceDeregister(registration.ID, cancellationToken);
            await _client.Agent.ServiceRegister(registration, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.Agent.ServiceDeregister(_registrationId, cancellationToken);
        }
    }
}