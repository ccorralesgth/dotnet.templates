using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace dotnet.templates.serilog
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildServices(builder);
                       
            //LogginConfScenarios(LogScenario.Basic);
            //LogginConfScenarios(LogScenario.File);            
            LogginConfScenarios(LogScenario.AppSettings, builder);
        }

        /// <summary>
        /// configuration builder
        /// </summary>
        /// <param name="builder"></param>
        private static void BuildServices(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                    .AddEnvironmentVariables();


            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                })
                .UseSerilog()
                .Build();
        }

        /// <summary>
        /// logging diff scenarios configurations
        /// </summary>
        /// <param name="logSceneario"></param>
        private static void LogginConfScenarios(LogScenario logSceneario,ConfigurationBuilder builder)
        {
            switch (logSceneario)
            {
                case LogScenario.Basic:
                    {
                        //basic logging configuration
                        var logconsole = new LoggerConfiguration()
                            .WriteTo.Console()
                            .CreateLogger();

                        logconsole.Information("Hello Serilog");

                        //using static
                        //Log.Logger = logconsole;
                        //Log.Logger.Information("Hello Serilog >> Console");

                        break;
                    }
                case LogScenario.File:
                    {
                        var logfile = new LoggerConfiguration()
                            .WriteTo.File($"../log-.txt", rollingInterval: RollingInterval.Day)
                            .CreateLogger();

                        logfile.Information("Hello Serilog >> file");
                        break;
                    }
                case LogScenario.Seq:
                    {
                        var logseq = new LoggerConfiguration()
                            .MinimumLevel.Verbose()
                            //.Enrich.FromLogContext()
                            //.WriteTo.Console()
                            //.WriteTo.Seq("http://localhost:5341")
                            .CreateLogger();

                        Log.Logger = logseq;
                        foreach (var item in new[] { 1, 2, 3, 4 })
                        {
                            logseq.Information("Hello Serilog >> Seq {item}", item);
                            logseq.Error("Hello Serilog >> Seq {item}", item);
                            logseq.Warning("Hello Serilog >> Seq {item}", item);


                            Log.Logger.Information("Hello Serilog >> Seq");
                        }

                    }
                    break;
                case LogScenario.AppSettings:
                    {
                        Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(builder.Build())
                            .CreateLogger();

                        foreach (var item in new[] { 1, 2, 3, 4 })
                        {
                            Log.Logger.Information("Hello Serilog >> Seq {item}", item);
                            Log.Logger.Error("Hello Serilog >> Seq {item}", item);
                            Log.Logger.Warning("Hello Serilog >> Seq {item}", item);                            
                        }
                    }
                    break;
                default:
                    Log.CloseAndFlush();
                    break;
            }
        }

        private enum LogScenario
        {
            Basic,
            Console,
            File,
            Seq,
            SQLServer,
            SqlLite,
            AppSettings

        }
    }
}
