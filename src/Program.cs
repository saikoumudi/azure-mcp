using AzureMCP.Commands;
using AzureMCP.Models;
using AzureMCP.Services.Azure.AppConfig;
using AzureMCP.Services.Azure.Cosmos;
using AzureMCP.Services.Azure.Monitor;
using AzureMCP.Services.Azure.ResourceGroup;
using AzureMCP.Services.Azure.Storage;
using AzureMCP.Services.Azure.Subscription;
using AzureMCP.Services.Caching;
using AzureMCP.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.Json;

namespace AzureMCP;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            var commandFactory = serviceProvider.GetRequiredService<CommandFactory>();
            var rootCommand = commandFactory.RootCommand;

            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            var response = new CommandResponse
            {
                Status = 500,
                Message = ex.Message,
                Arguments = [],
                Duration = 0
            };
            Console.WriteLine(JsonSerializer.Serialize(response));
            return 1;
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();
        services.AddSingleton<ICosmosService, CosmosService>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<IMonitorService, MonitorService>();
        services.AddSingleton<IResourceGroupService, ResourceGroupService>();
        services.AddSingleton<IAppConfigService, AppConfigService>();
        services.AddSingleton<CommandFactory>();
    }
}
