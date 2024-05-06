using Azure.Messaging.ServiceBus;
using ContactProvider.Contexts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()

    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(Environment.GetEnvironmentVariable("AzureSql")));

        services.AddSingleton(new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnection")));
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var migration = context.Database.GetPendingMigrations();
        if (migration != null && migration.Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {

        Debug.WriteLine(ex.Message);
    }
}


host.Run();