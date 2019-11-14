using System;
using gRpc.Vs.WebApi.RestClient;
using GrpcData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

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
            
            services.AddControllers();
            services.AddGrpcClient<GrpcDataService.GrpcDataServiceClient>(c =>
            {
                c.Address = new Uri(_urls.GrpcServer);
            });
            services.AddHttpClient<IDataClient, DataClient>(c =>
            {
                c.BaseAddress = new Uri(_urls.RestServer);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // logger can be used only here from core 3.0 https://stackoverflow.com/questions/41287648/how-do-i-write-logs-from-within-startup-cs
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogWarning(_urls.GrpcServer);
            logger.LogWarning(_urls.RestServer);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
