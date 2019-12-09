using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics;

namespace gRpc.Vs.WebApi.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                // its not needed, if not set event log will show source as .net runtime - warning will be logged automatically (if you want to add Info - need to change appsettings
                /*
                 * https://github.com/aspnet/Extensions/issues/1839
                 */
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<EventLogSettings>(settings =>
                    {
                        if (string.IsNullOrEmpty(settings.SourceName))
                        {
                            settings.SourceName = hostContext.HostingEnvironment.ApplicationName;
                            settings.LogName = hostContext.HostingEnvironment.ApplicationName;
                        }
                    });
                });

    }
}
