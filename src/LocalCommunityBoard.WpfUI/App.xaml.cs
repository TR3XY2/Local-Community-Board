// <copyright file="App.xaml.cs" company="PlaceholderCompanн">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI;

using System.Windows;
using LocalCommunityBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

/// <summary>
/// Interaction logic for the WPF application.
/// </summary>
public partial class App : Application
{
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// Sets up the logging configuration for the application.
    /// </summary>
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

        Log.Information("Application starting up...");
    }

    /// <summary>
    /// Handles the startup logic for the application.
    /// </summary>
    /// <param name="e">The startup event arguments.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var connectionString = this.configuration.GetConnectionString("Postgres");

        var optionsBuilder = new DbContextOptionsBuilder<LocalCommunityBoardDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        using (var context = new LocalCommunityBoardDbContext(optionsBuilder.Options))
        {
            context.Database.Migrate();
        }

        Log.Information("WPF application started");
    }

    /// <summary>
    /// Handles the shutdown logic for the application.
    /// </summary>
    /// <param name="e">The exit event arguments.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Application is shutting down");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
