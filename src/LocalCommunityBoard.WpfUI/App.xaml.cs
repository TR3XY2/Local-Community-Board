// <copyright file="App.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI
{
    using System.Windows;
    using Serilog;

    public partial class App : Application
    {
        public App()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Application starting up...");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Log.Information("WPF application started");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application is shutting down");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
