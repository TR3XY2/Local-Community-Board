using Serilog;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting up");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var app = builder.Build();

    app.MapGet("/", (ILogger<Program> logger) =>
    {
        logger.LogInformation("Hello from endpoint with structured data: {@User}", new { Id = 1, Name = "Þëÿ" });
        return "Hello";
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
