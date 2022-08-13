using EdgeDB;
using EdgeDB.ExampleApp;
using EdgeDB.ILExpressionParser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

void Test()
{
    var func = (int x) => x += 1;
    var result = ExpressionParser.Parse<Func<int, int>>(func);

}

Test();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} - {Level}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

using var host = Host.CreateDefaultBuilder()
    .ConfigureServices((services) =>
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        services.AddEdgeDB();

        services.AddSingleton<ExampleRunner>();
    }).Build();

await host.Services.GetRequiredService<ExampleRunner>().StartAsync();

// hault the program
await Task.Delay(-1);
