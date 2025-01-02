using CityworksOfficeServiceApp.Implementations;
using CityworksOfficeServiceApp.Services;
using CPW_HandlePaymentTransactionCompleted;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XTI_App.Api;
using XTI_CityworksAppClient;
using XTI_CityworksOfficeAppClient;
using XTI_CityworksOfficeServiceAppApi;
using XTI_HubAppClient.ServiceApp.Extensions;
using XTI_Jobs;
using XTI_Jobs.Abstractions;
using XTI_PaymentTransactionAppClient;
using XTI_ScheduledJobsAppClient;

await XtiServiceAppHost.CreateDefault(CityworksOfficeAppKey.Value, args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddCityworksOfficeAppApiServices();
        services.AddScoped<AppApiFactory, CityworksOfficeAppApiFactory>();
        services.AddScoped(sp => (CityworksOfficeAppApi)sp.GetRequiredService<IAppApi>());
        services.AddScheduledJobsAppClient();
        services.AddScoped<IJobDb, SjcJobDb>();
        services.AddScoped<EventMonitorBuilder>();
        services.AddScoped<HandlePaymentTransactionCompletedActionFactory>();
        services.AddCityworksAppClient();
        services.AddScoped<ICityworksService, DefaultCityworksService>();
        services.AddCityworksOfficeAppClient();
        services.AddPaymentTransactionAppClient();
        services.AddScoped<IPaymentTransactionService, DefaultPaymentTransactionService>();
    })
    .UseWindowsService()
    .Build()
    .RunAsync();