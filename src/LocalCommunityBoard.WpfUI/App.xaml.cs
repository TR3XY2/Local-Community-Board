// <copyright file="App.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI;

using System;
using System.Windows;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;
using LocalCommunityBoard.Infrastructure.Data;
using LocalCommunityBoard.Infrastructure.Logging;
using LocalCommunityBoard.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        // Load configuration
        this.configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<App>()
            .Build();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(this.configuration)
            .Enrich.FromLogContext()
            .WriteTo.File("logs/user-actions.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day)
            .WriteTo.File("logs/errors.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Logging.Factory = new SerilogLoggerFactory(Log.Logger);

        var logger = Logging.CreateLogger<App>();
        logger.LogInformation("Application starting up...");
    }

    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var logger = Logging.CreateLogger<App>();

        try
        {
            // Global exception handling
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                logger.LogCritical(args.ExceptionObject as Exception, "Unhandled domain exception");

            this.DispatcherUnhandledException += (_, args) =>
            {
                logger.LogCritical(args.Exception, "Unhandled UI exception");
                args.Handled = true;
            };

            // Configure services (DI)
            var services = new ServiceCollection();
            this.ConfigureServices(services);

            Services = services.BuildServiceProvider();

            // Run migrations
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LocalCommunityBoardDbContext>();

                db.Database.Migrate();
            }

            // Start main window
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

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

    private void ConfigureServices(IServiceCollection services)
    {
        // Ensure Microsoft.Extensions.Logging is configured and Serilog is hooked into DI
        services.AddLogging(builder =>
        {
            builder.ClearProviders();

            // Integrate Serilog with the Microsoft logging abstractions so ILogger<T> can be injected
            builder.AddSerilog(Log.Logger, dispose: false);
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });

        // Database
        var connectionString = this.configuration.GetConnectionString("Postgres");
        services.AddDbContext<LocalCommunityBoardDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<IRepository<Location>, Repository<Location>>();
        services.AddScoped<IRepository<Category>, Repository<Category>>();
        services.AddScoped<ICommentRepository, CommentRepository>();

        // Application Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddSingleton<UserSession>(); // session to track logged-in user
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportService, ReportService>();

        // WPF Windows
        services.AddTransient<MainWindow>();
    }
}
