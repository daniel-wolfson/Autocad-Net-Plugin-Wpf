using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.IO;

namespace ID.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string baseDir = Directory.GetCurrentDirectory();
            if (WindowsServiceHelpers.IsWindowsService())
            {
                baseDir = AppDomain.CurrentDomain.BaseDirectory;
            }

            try
            {
                CreateHostBuilder(args, baseDir).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"API start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string baseDir) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;
#if RELEASE
                env.EnvironmentName = "Production";
#endif
                var environmentName = env.IsProduction() ? "" : "." + env.EnvironmentName;

                config.SetBasePath(baseDir)
                      .AddJsonFile($"appsettings{environmentName}.json")
                      .AddEnvironmentVariables(prefix: "ID_")
                      .Build();
            })
            .ConfigureServices((context, services) =>
            {
                var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(context.Configuration);
                Log.Logger = loggerConfig.CreateLogger();
                services.AddSingleton<Serilog.ILogger>(x => Log.Logger);
                services.AddSingleton<ILoggerFactory>(x => new SerilogLoggerFactory(null, false));

                var env = context.HostingEnvironment;
                var ver = typeof(Startup).Assembly.GetName().Version.ToString();
                var environmentName = env.IsProduction() ? "Release" : env.EnvironmentName;

                Log.Information(WindowsServiceHelpers.IsWindowsService()
                    ? $"Starting Windows Service ({environmentName} ver.{ver})"
                    : $"Starting Web host Service ({ environmentName} ver.{ver})");
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.AddSerilog(logging.Services
                    .BuildServiceProvider()
                    .GetRequiredService<Serilog.ILogger>(),
                    dispose: true);
            })
            .UseSerilog()
            .UseWindowsService()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
