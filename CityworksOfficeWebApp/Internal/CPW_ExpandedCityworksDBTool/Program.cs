using CPW_ExpandedCityworksDB.SqlServer;
using CPW_ExpandedCityworksDBTool;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XTI_Core;
using XTI_Core.Extensions;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.UseXtiConfiguration(hostingContext.HostingEnvironment, "", "", args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddConfigurationOptions<ToolOptions>();
        services.AddSingleton(_ => XtiEnvironment.Parse(hostContext.HostingEnvironment.EnvironmentName));
        services.AddExpandedCityworksDbContextForSqlServer();
        services.AddHostedService<HostedService>();
    })
    .RunConsoleAsync();