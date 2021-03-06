using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Sampler;
using System;
using gRpc.Vs.WebApi.Core.Consul;
using Microsoft.Extensions.Options;
using zipkin4net;
using zipkin4net.Middleware;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace gRpc.Vs.WebApi.RestServer
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
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.GetSection("Urls").Bind(_urls);

            services.AddOpenTelemetry(builder =>
            {
                builder.SetSampler(Samplers.AlwaysSample).UseZipkin(o =>
                    {
                        o.ServiceName = "rest-server";
                        o.Endpoint = new Uri($"{_urls.Zipkin}api/v2/spans");
                    })
                    // you may also configure request and dependencies collectors
                    .AddRequestCollector();
            });

            services.AddResponseCompression();
            services.AddControllers();
            services.AddHealthChecks();
            services.AddConsul();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IOptions<ConsulConfig> consulConfig)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            //var lifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            //lifetime.ApplicationStarted.Register(() => {
            //    TraceManager.SamplingRate = 1.0f;
            //    var logger = new TracingLogger(loggerFactory, "zipkin4net");
            //    var httpSender = new HttpZipkinSender("http://localhost:9411", "application/json");
            //    var tracer = new ZipkinTracer(httpSender, new JSONSpanSerializer());
            //    TraceManager.RegisterTracer(tracer);
            //    TraceManager.Start(logger);
            //});
            //lifetime.ApplicationStopped.Register(() => TraceManager.Stop());

            //app.UseTracing("rest-server");

            app.UseResponseCompression();
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseHealthChecks(consulConfig.Value.HealthCheckEndpoint);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
