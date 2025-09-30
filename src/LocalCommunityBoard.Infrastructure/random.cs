using Serilog;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("App started");
    Log.ForContext("RequestId", Guid.NewGuid())
       .Information("Processing {@Payload}", new { Items = new[] { 1, 2, 3 }, Name = "Order" });

    // приклад логування помилки
    try
    {
        throw new InvalidOperationException("Test error");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Something went wrong while processing item {ItemId}", 123);
    }
}
finally
{
    Log.CloseAndFlush();
}
