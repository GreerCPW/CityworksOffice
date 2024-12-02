using CPW_ExpandedCityworksDB;
using CPW_ExpandedCityworksDB.SqlServer;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XTI_Core;
using XTI_Core.Extensions;

namespace CPW_ExpandedCityworksDBTool;

public sealed class ExpandedCityworksDbContextFactory : IDesignTimeDbContextFactory<ExpandedCityworksDbContext>
{
    public ExpandedCityworksDbContext CreateDbContext(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.UseXtiConfiguration(hostingContext.HostingEnvironment, "", "", args);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton(_ => XtiEnvironment.Parse(hostContext.HostingEnvironment.EnvironmentName));
                services.AddExpandedCityworksDbContextForSqlServer();
            })
            .Build();
        var scope = host.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ExpandedCityworksDbContext>();
    }
}
