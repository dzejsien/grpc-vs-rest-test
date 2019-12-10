using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;
using gRpc.Vs.WebApi.RestClient;
using GrpcData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

            services.AddControllers();
            services.AddGrpcClient<GrpcDataService.GrpcDataServiceClient>(c =>
            {
                c.Address = new Uri(_urls.GrpcServer);
            });
            //.ConfigurePrimaryHttpMessageHandler(configurePrimaryHttpMessageHandler);

            services.AddHttpClient<IDataClient, DataClient>(c => { c.BaseAddress = new Uri(_urls.RestServer); })
                //https://github.com/openzipkin/zipkin4net/issues/216
                .AddHttpMessageHandler(_ => TracingHandler.WithoutInnerHandler("gateway"));
            // for diagnostics of cert - which was used etc 
            //.ConfigurePrimaryHttpMessageHandler(configurePrimaryHttpMessageHandler);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // logger can be used only here from core 3.0 https://stackoverflow.com/questions/41287648/how-do-i-write-logs-from-within-startup-cs
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, ILoggerFactory loggerFactory)
        {
            logger.LogWarning(_urls.GrpcServer);
            logger.LogWarning(_urls.RestServer);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var lifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            lifetime.ApplicationStarted.Register(() => {
                TraceManager.SamplingRate = 1.0f;
                var logger = new TracingLogger(loggerFactory, "zipkin4net");
                var httpSender = new HttpZipkinSender("http://localhost:9411", "application/json");
                var tracer = new ZipkinTracer(httpSender, new JSONSpanSerializer());
                TraceManager.RegisterTracer(tracer);
                TraceManager.Start(logger);
            });
            lifetime.ApplicationStopped.Register(() => TraceManager.Stop());

            app.UseTracing("gateway");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
