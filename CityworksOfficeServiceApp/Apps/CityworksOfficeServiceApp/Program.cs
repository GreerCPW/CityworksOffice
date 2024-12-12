using CityworksOfficeServiceApp.Implementations;
using CityworksOfficeServiceApp.Services;
using CPW_HandlePaymentTransactionCompleted;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XTI_App.Api;
using XTI_CityworksAppClient;
using XTI_CityworksOfficeAppClient;
using XTI_CityworksOfficeServiceAppApi;
using XTI_Core;
using XTI_HubAppClient.ServiceApp.Extensions;
using XTI_Jobs;
using XTI_Jobs.Abstractions;
using XTI_PaymentTransactionAppClient;
using XTI_Schedule;
using XTI_ScheduledJobsAppClient;

await XtiServiceAppHost.CreateDefault(CityworksOfficeInfo.AppKey, args)
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
        services.AddAppAgenda
        (
            (sp, agenda) =>
            {
                agenda.AddScheduled<CityworksOfficeAppApi>
                (
                    (api, agendaItem) =>
                    {
                        agendaItem.Action(api.Jobs.HandlePaymentTransactionCompleted)
                            .Interval(TimeSpan.FromSeconds(10))
                            .AddSchedule
                            (
                                Schedule.EveryDay().At(TimeRange.AllDay())
                            );
                    }
                );
                agenda.AddScheduled<CityworksOfficeAppApi>
                (
                    (api, agendaItem) =>
                    {
                        agendaItem.Action(api.Receivables.AddOrUpdateReceivables)
                            .Interval(TimeSpan.FromHours(1))
                            .AddSchedule
                            (
                                Schedule.EveryDay().At(TimeRange.From(new TimeOnly(6, 0)).For(17).Hours())
                            );
                    }
                );
            }
        );
        services.AddThrottledLog<CityworksOfficeAppApi>
        (
            (api, throttle) =>
            {
                throttle.Throttle(api.Jobs.HandlePaymentTransactionCompleted)
                    .Requests().ForOneHour()
                    .Exceptions().For(5).Minutes();
            }
        );
    })
    .UseWindowsService()
    .Build()
    .RunAsync();