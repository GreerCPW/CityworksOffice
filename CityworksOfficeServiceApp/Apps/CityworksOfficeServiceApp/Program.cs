using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XTI_App.Api;
using XTI_CityworksOfficeServiceAppApi;
using XTI_Core;
using XTI_HubAppClient.ServiceApp.Extensions;
using XTI_Jobs.Abstractions;
using XTI_Jobs;
using XTI_Schedule;
using XTI_ScheduledJobsAppClient;
using DinkToPdf.Contracts;
using DinkToPdf;
using CPW_HandlePaymentTransactionCompleted;

await XtiServiceAppHost.CreateDefault(CityworksOfficeInfo.AppKey, args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddCityworksOfficeAppApiServices();
        services.AddScoped<AppApiFactory, CityworksOfficeAppApiFactory>();
        services.AddScoped(sp => (CityworksOfficeAppApi)sp.GetRequiredService<IAppApi>());
        services.AddScheduledJobsAppClient();
        services.AddScoped<IJobDb, SjcJobDb>();
        services.AddScoped<EventMonitorBuilder>();
        services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        services.AddScoped<HandlePaymentTransactionCompletedActionFactory>();
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