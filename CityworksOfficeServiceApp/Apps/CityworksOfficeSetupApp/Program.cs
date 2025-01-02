using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CityworksOfficeSetupApp;
using XTI_App.Abstractions;
using XTI_App.Api;
using XTI_AppSetupApp.Extensions;
using XTI_CityworksOfficeServiceAppApi;
using XTI_Jobs.Abstractions;
using XTI_Jobs;
using XTI_ScheduledJobsAppClient;

await XtiSetupAppHost.CreateDefault(CityworksOfficeAppKey.Value, args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton(_ => AppVersionKey.Current);
        services.AddScoped<AppApiFactory, CityworksOfficeAppApiFactory>();
        services.AddScoped<IAppSetup, CityworksOfficeAppSetup>();
        services.AddScheduledJobsAppClient();
        services.AddScoped<IJobDb, SjcJobDb>();
        services.AddScoped<JobRegistrationBuilder>();
        services.AddScoped<CityworksOfficeJobSetup>();
    })
    .RunConsoleAsync();