using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Linq;

namespace Xu.IdentityServer
{
    public class Program
    {
        public Program(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public static IWebHostEnvironment Environment { get; set; }

        public static void Main(string[] args)
        {
            Console.Title = "IdentityServerWithEfAndAspNetIdentity";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(@"Logs/identityserver4_log.txt")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            var seed = args.Contains("/seed");
            if (seed)
            {
                args = args.Except(new[] { "/seed" }).ToArray();
            }

            var host = CreateHostBuilder(args).Build();

            if (seed)
            {
                SeedData.EnsureSeedData(host.Services, Environment.WebRootPath);
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
               .ConfigureKestrel(serverOptions =>
               {
                   serverOptions.AllowSynchronousIO = true;//∆Ù”√Õ¨≤Ω IO
               })
               .UseStartup<Startup>()
               .UseUrls("http://*:5004")
               .ConfigureLogging((hostingContext, builder) =>
               {
                   builder.ClearProviders();
                   builder.SetMinimumLevel(LogLevel.Trace);
                   builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                   builder.AddConsole();
                   builder.AddDebug();
               });
                });
    }
}