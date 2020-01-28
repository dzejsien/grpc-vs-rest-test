namespace gRpc.Vs.WebApi.Core.Consul
{
    public class ConsulConfig
    {
        public bool Enabled { get; set; }
        public string Url { get; set; }
        public string Service { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public bool HealthCheckEnabled { get; set; }
        public string HealthCheckEndpoint { get; set; }
        public int HealthCheckInterval { get; set; }
        public int RemoveAfterInterval { get; set; }
    }
}