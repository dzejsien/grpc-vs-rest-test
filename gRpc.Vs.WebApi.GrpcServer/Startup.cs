using gRpc.Vs.WebApi.GrpcServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Sampler;
using System;
using gRpc.Vs.WebApi.Core.Consul;
using Microsoft.Extensions.Options;

namespace gRpc.Vs.WebApi.GrpcServer
{
    public class Urls
    {
        public string Zipkin { get; set; }
    }

    public class Startup
    {
        private readonly Urls _urls = new Urls();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.GetSection("Urls").Bind(_urls);

            services.AddOpenTelemetry(builder =>
            {
                builder.SetSampler(Samplers.AlwaysSample).UseZipkin(o =>
                    {
                        o.ServiceName = "grpc-server";
                        o.Endpoint = new Uri($"{_urls.Zipkin}api/v2/spans");
                    })
                    // you may also configure request and dependencies collectors
                    .AddRequestCollector();
            });
            services.AddGrpc();

            services.AddHealthChecks();
            services.AddConsul();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<ConsulConfig> consulConfig)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz");
                endpoints.MapGrpcService<DataService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
