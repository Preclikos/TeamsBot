using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net;
using Serilog.Events;
using Serilog.Core;
using Serilog;
using System;

namespace AutoDeployment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();
            try
            {
                Log.Information("Starting up");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                 .UseStartup<Startup>()
                 .UseSerilog()
                 .UseKestrel((hostingContext, options) =>
                 {
                     if (hostingContext.HostingEnvironment.IsProduction())
                     {
                         options.Listen(IPAddress.Any, 9002, listenOptions =>
                         {
                             listenOptions.UseHttps("certificates/cert.pfx", "prdel");
                         }
                     );
                     }
                 });
        }
    }
}
