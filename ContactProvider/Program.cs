using Azure.Messaging.ServiceBus;
using ContactProvider.Contexts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var configuration = context.Configuration;

        services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AzureSql")));

        services.AddSingleton(new ServiceBusClient(configuration.GetConnectionString("ServiceBusConnection")));
    })
    .Build();
