using CPW_ExpandedCityworksDB;
using CPW_ExpandedCityworksDB.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XTI_Core;

namespace CPW_ExpandedCityworksDBTool;

public sealed class HostedService : IHostedService
{
    private readonly IServiceProvider services;

    public HostedService(IServiceProvider services)
    {
        this.services = services;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        try
        {
            var xtiEnv = scope.ServiceProvider.GetRequiredService<XtiEnvironment>();
            EnvironmentSettings.LoadEnvironment(xtiEnv);
            var options = scope.ServiceProvider.GetRequiredService<ToolOptions>();
            if (options.Command.Equals("update", StringComparison.OrdinalIgnoreCase))
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ExpandedCityworksDbContext>();
                await dbContext.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Environment.ExitCode = 999;
        }
        var lifetime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
        lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
