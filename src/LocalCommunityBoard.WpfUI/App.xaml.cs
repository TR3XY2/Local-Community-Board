// <copyright file="App.xaml.cs" company="PlaceholderCompanн">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI;

using System.Windows;
using Serilog;

/// <summary>
/// Interaction logic for the WPF application.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// Sets up the logging configuration for the application.
    /// </summary>
    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Seq("http://localhost:5341")
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
