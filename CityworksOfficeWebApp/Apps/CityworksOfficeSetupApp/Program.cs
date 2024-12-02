using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CityworksOfficeSetupApp;
using XTI_App.Abstractions;
using XTI_App.Api;
using XTI_AppSetupApp.Extensions;
using XTI_CityworksOfficeWebAppApi;

await XtiSetupAppHost.CreateDefault(CityworksOfficeInfo.AppKey, args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton(_ => AppVersionKey.Current);
        services.AddScoped<AppApiFactory, CityworksOfficeAppApiFactory>();
        services.AddScoped<IAppSetup, CityworksOfficeAppSetup>();
    })
    .RunConsoleAsync();