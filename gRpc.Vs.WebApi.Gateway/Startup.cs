using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;
using gRpc.Vs.WebApi.Core.Consul;
using gRpc.Vs.WebApi.RestClient;
using GrpcData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Sampler;
using zipkin4net;
using zipkin4net.Middleware;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace gRpc.Vs.WebApi.Gateway
{
    public class Urls
    {
        public string GrpcServer { get; set; }
        public string RestServer { get; set; }
        public string Zipkin { get; set; }
    }

    public class Startup
    {
        private readonly Urls _urls = new Urls();
        private readonly ServiceDiscoveryConfig _sdConfig = new ServiceDiscoveryConfig();
        private const string ServiceDiscoverySection = "serviceDiscovery";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<ServiceDiscoveryConfig>(Configuration.GetSection(ServiceDiscoverySection));
            Configuration.GetSection("Urls").Bind(_urls);
            Configuration.GetSection(ServiceDiscoverySection).Bind(_sdConfig);

            Func<HttpClientHandler> configurePrimaryHttpMessageHandler = () =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
                    {
                        Console.WriteLine($"SslPolicyErrors: {errors}");

                        if (chain == null)
                        {
                            Console.WriteLine("No chain...");
                        }
                        else
                        {
                            foreach (X509ChainElement element in chain.ChainElements)
                            {
                                Console.WriteLine();
                                Console.WriteLine(element.Certificate.Subject);

                                foreach (X509ChainStatus status in element.ChainElementStatus)
                                {
                                    Console.WriteLine($"  {status.Status}: {status.StatusInformation}");
                                }
                            }
                        }

                        return true;
                    }
                };

                return handler;
            };

            services.AddConsul();

            services.AddOpenTelemetry(builder =>
            {
                builder.SetSampler(Samplers.AlwaysSample).UseZipkin(o =>
                    {
                        o.ServiceName = "gateway";
                        o.Endpoint = new Uri($"{_urls.Zipkin}api/v2/spans");
                    })
                    // you may also configure request and dependencies collectors
                    .AddRequestCollector();
            });

            services.AddControllers();

            // its stupid, but we can't use consul here, its not yet initialized.
            // in order to make this work correctly we should create own impl of Clients

            services.AddGrpcClient<GrpcDataService.GrpcDataServiceClient>(c =>
            {
                c.Address = new Uri(_urls.GrpcServer);
            });
            //.ConfigurePrimaryHttpMessageHandler(configurePrimaryHttpMessageHandler);

            services.AddHttpClient<IDataClient, DataClient>(c => { c.BaseAddress = new Uri(_urls.RestServer); });
            //https://github.com/openzipkin/zipkin4net/issues/216
            //.AddHttpMessageHandler(_ => TracingHandler.WithoutInnerHandler("gateway"));
            // for diagnostics of cert - which was used etc 
            //.ConfigurePrimaryHttpMessageHandler(configurePrimaryHttpMessageHandler);
            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // logger can be used only here from core 3.0 https://stackoverflow.com/questions/41287648/how-do-i-write-logs-from-within-startup-cs
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, ILoggerFactory loggerFactory, IOptions<ConsulConfig> consulConfig)
        {
            logger.LogWarning(_urls.GrpcServer);
            logger.LogWarning(_urls.RestServer);
            logger.LogWarning(_urls.Zipkin);

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

            //app.UseTracing("gateway");

            // not sure if it is needed, on k8s its after ingress which terminates tls and gateway port is exposed as 80
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
