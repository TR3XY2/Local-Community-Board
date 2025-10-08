// <copyright file="App.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI;

using System.Windows;
using LocalCommunityBoard.Infrastructure.Data;
using LocalCommunityBoard.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

/// <summary>
/// Interaction logic for the WPF application.
/// </summary>
public partial class App : Application
{
    private readonly IConfiguration configuration;

    public App()
    {
        this.configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<App>()
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(this.configuration)
            .Enrich.FromLogContext()
            .WriteTo.File("logs/user-actions.log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
            .WriteTo.File("logs/errors.log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
            .CreateLogger();

        Logging.Factory = new SerilogLoggerFactory(Log.Logger);

        var logger = Logging.CreateLogger<App>();
        logger.LogInformation("Application starting up...");
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var logger = Logging.CreateLogger<App>();

        try
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                logger.LogCritical(args.ExceptionObject as Exception, "Unhandled domain exception");

            this.DispatcherUnhandledException += (_, args) =>
            {
                logger.LogCritical(args.Exception, "Unhandled UI exception");
                args.Handled = true;
            };

            var connectionString = this.configuration.GetConnectionString("Postgres");
            var optionsBuilder = new DbContextOptionsBuilder<LocalCommunityBoardDbContext>()
                .UseNpgsql(connectionString);

            using (var context = new LocalCommunityBoardDbContext(optionsBuilder.Options))
            {
                context.Database.Migrate();
            }

            logger.LogInformation("WPF application started successfully.");
            logger.LogInformation("User session started at {Time}", DateTime.Now);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during application startup");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var logger = Logging.CreateLogger<App>();
        logger.LogInformation("User session ended at {Time}", DateTime.Now);
        logger.LogInformation("Application is shutting down...");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
