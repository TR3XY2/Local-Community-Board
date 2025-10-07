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
            .CreateLogger();

        LoggerFactory = new SerilogLoggerFactory(Log.Logger);
        Logger = LoggerFactory.CreateLogger<App>();

        Logging.Factory = LoggerFactory;

        Logger.LogInformation("Application starting up...");
    }

    public static ILoggerFactory LoggerFactory { get; private set; } = null!;

    public static ILogger<App> Logger { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                Logger.LogCritical(args.ExceptionObject as Exception, "Unhandled domain exception");

            this.DispatcherUnhandledException += (_, args) =>
            {
                Logger.LogCritical(args.Exception, "Unhandled UI exception");
                args.Handled = true;
            };

            var connectionString = this.configuration.GetConnectionString("Postgres");
            var optionsBuilder = new DbContextOptionsBuilder<LocalCommunityBoardDbContext>()
                .UseNpgsql(connectionString);

            using (var context = new LocalCommunityBoardDbContext(optionsBuilder.Options))
            {
                context.Database.Migrate();
            }

            Logger.LogInformation("WPF application started successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during application startup");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Logger.LogInformation("Application is shutting down...");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
