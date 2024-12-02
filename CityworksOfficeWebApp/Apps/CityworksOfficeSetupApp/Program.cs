using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CityworksOfficeSetupApp;
using XTI_App.Abstractions;
using XTI_App.Api;
using XTI_AppSetupApp.Extensions;
using XTI_CityworksOfficeWebAppApi;
using XTI_DB;
using CPW_ExpandedCityworksDB;
using CPW_ExpandedCityworksDB.SqlServer;

await XtiSetupAppHost.CreateDefault(CityworksOfficeInfo.AppKey, args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton(_ => AppVersionKey.Current);
        services.AddScoped<AppApiFactory, CityworksOfficeAppApiFactory>();
        services.AddScoped<IAppSetup, CityworksOfficeAppSetup>();
        services.AddExpandedCityworksDbContextForSqlServer();
        services.AddScoped<DbAdmin<ExpandedCityworksDbContext>>();
    })
    .RunConsoleAsync();