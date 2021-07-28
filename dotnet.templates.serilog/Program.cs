using dotnet.templates.serilog.figures;
using dotnet.templates.serilog.figures.square;
using dotnet.templates.serilog.figures.triangle;
using dotnet.templates.serilog.shapes.square;
using dotnet.templates.serilog.shapes.triangle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Filters;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace dotnet.templates.serilog
{
    internal class Program
    {
        /// <summary>
        /// main
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static async Task Main(string[] args)
        {            
             LoggingConfigurationScenarios(LogConfigurationTypes.FiltersAndSubLogger, null);            
        }

        /// <summary>
        /// configuration builder
        /// </summary>
        /// <param name="builder"></param>
        private static IConfigurationBuilder BuildExtendedConfiguration()
        {
            return new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                    .AddEnvironmentVariables();
        }

        /// <summary>
        /// different way to configure
        /// </summary>
        static IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
               .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .Build();
        }

        /// <summary>
        /// logger creator
        /// </summary>
        /// <param name="configurationRoot"></param>
        private static void CreateLogger(IConfigurationRoot configurationRoot)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(configurationRoot)
                .CreateLogger();
        }

        /// <summary>
        /// return IHost builded
        /// </summary>
        /// <returns></returns>
        private static IHost BuildHostServices()
        {
            return Host.CreateDefaultBuilder()
               .ConfigureServices((context, services) =>
               {
                   services.AddScoped<ISquareServices, Square>();
                   services.AddScoped<ITriangle, Triangle>();

               })
               .UseSerilog()
               .Build();
        }

        /// <summary>
        /// logging diff scenarios configurations
        /// </summary>
        /// <param name="logSceneario"></param>
        private static void LoggingConfigurationScenarios(LogConfigurationTypes logSceneario,ConfigurationBuilder builder)
        {
            switch (logSceneario)
            {
                case LogConfigurationTypes.Basic:
                    {                        
                        //basic logging configuration
                        var basicConsole = new LoggerConfiguration()
                           .WriteTo.Console()                           
                           .CreateLogger();

                        basicConsole.Information("Hello Serilog >> Basic Configuration <<");
                        
                        // assigning static logger
                        Log.Logger = basicConsole;

                        //other ways to call logger
                        Log.Logger.Warning("Hello Serilog >> From Static Logger in Basic Configuration <<");
                        Log.Error("Hello Serilog >> From Logger in Basic Configuration <<");
                        break;
                    }
                case LogConfigurationTypes.Root:
                    {
                        var root = BuildConfiguration();
                        Log.Logger = new LoggerConfiguration()
                        .ReadFrom
                        .Configuration(root)
                        .WriteTo
                        .Console()
                        .CreateLogger();
                    }
                    break;
                case LogConfigurationTypes.File:
                    {                        
                        var logFile = new LoggerConfiguration()
                            .WriteTo.File($"../log-.txt", rollingInterval: RollingInterval.Day)
                            .CreateLogger();

                        logFile.Information("Hello Serilog >> File Configuration <<");
                        break;
                    }
                case LogConfigurationTypes.Enrich:
                    
                    //basic logging configuration
                    var logconsole = new LoggerConfiguration()
                       .WriteTo.Console()
                       .Enrich.WithProcessId()
                       .Enrich.WithProcessName()
                       .Enrich.FromLogContext()
                       .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {Properties:j}] {Message:lj} {NewLine}{Exception}")
                       .CreateLogger();

                    logconsole.Information("Hello Serilog >> From Enrich Configiration <<");
                    
                    break;
                case LogConfigurationTypes.Seq:
                    {                       

                        var logseq = new LoggerConfiguration()
                            .MinimumLevel.Verbose()
                            .WriteTo.Seq("http://localhost:5341")
                            .CreateLogger();

                        Log.Logger = logseq;
                        foreach (var item in new[] { 1, 2, 3, 4 })
                        {
                            logseq.Information("Hello Serilog >> Seq {item}", item);
                            logseq.Error("Hello Serilog >> Seq {item}", item);
                            logseq.Warning("Hello Serilog >> Seq {item}", item);
                            //other way
                            Log.Logger.Information("Hello Serilog >> Seq");
                        }

                    }
                    break;
                case LogConfigurationTypes.Filters:
                    
                    Log.Logger = new LoggerConfiguration()                                                
                        .WriteTo
                        .Console()
                        //.Filter.ByExcluding(Matching.WithProperty<int>("Items", p => p > 100))
                        //.Filter.ByIncludingOnly(Matching.WithProperty<int>("Items", p => p > 100))
                        .Filter.ByIncludingOnly(Matching.WithProperty<int>("Items", p => p % 2 == 0))                                                
                        .CreateLogger();

                    int item2 = 0;
                    while (true)
                    {
                        Log.Logger.Information("Hello Serilog to Console filtering {Items}", item2);
                        item2++;
                        Task.Delay(TimeSpan.FromSeconds(5));
                    }

                    break;

                case LogConfigurationTypes.FiltersAndSubLogger:
                    
                    var host = BuildHostServices();
                    //var isSquare = Matching.FromSource<Square>();
                    //var isTriangle = Matching.FromSource<Triangle>();

                    var isSquare = Matching.FromSource("dotnet.templates.serilog.figures.square");
                    var isTriangle = Matching.FromSource("dotnet.templates.serilog.figures.triangle");

                    Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.File("C:\\logs\\appserilog\\general.log")
                    .WriteTo.Logger(l => l
                        .Filter.ByIncludingOnly(isSquare)
                        .WriteTo.File("C:\\logs\\appserilog\\square.log"))
                    .WriteTo.Logger(l => l
                        .Filter.ByIncludingOnly(isTriangle)
                        .WriteTo.File("C:\\logs\\appserilog\\triangle.log"))                  
                    .CreateLogger();

                    int subitem = 0;

                    while (true)
                    {
                        Log.Information("Im a general log");
                        
                        //if using host builder with <<itself>> logger context
                        var au_s = ActivatorUtilities.CreateInstance<Square>(host.Services);
                        var au_t = ActivatorUtilities.CreateInstance<Triangle>(host.Services);
                        
                        //if using this <<main>> serilog context
                        var s = new Square(Log.Logger);
                        var t = new Triangle(Log.Logger);

                        subitem++;
                        Task.Delay(TimeSpan.FromSeconds(5));
                    }

                    break;
                case LogConfigurationTypes.AppSettings:
                    {
                        //Define logger only  and use appsettings for the other configurations
                        //like enrichers, writers, using, etc
                        //Use imperative (or non declarative ) configuration for complex more scenarios
                        Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(builder.Build())
                            .CreateLogger();

                        foreach (var item in new[] { 1, 2, 3, 4 })
                        {
                            Log.Logger.Information("Hello Serilog From Appsettings >>  {item}", item);
                            Log.Logger.Error("Hello Serilog >> From Appsettings {item}", item);
                            Log.Logger.Warning("Hello Serilog >> From Appsettings {item}", item);                            
                        }
                    }
                    break;
                default:
                    Log.CloseAndFlush();
                    break;
            }
        }

        /// <summary>
        /// log configuration  types enum
        /// </summary>
        private enum LogConfigurationTypes
        {
            Basic,
            Root,
            Console,
            File,
            Seq,
            SQLServer,
            SqlLite,
            Enrich,
            AppSettings,
            Filters,
            FiltersAndSubLogger
        }
    }
}
