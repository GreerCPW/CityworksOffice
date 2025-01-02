using XTI_Core;
using XTI_Schedule;

namespace XTI_CityworksOfficeServiceAppApi.Jobs;

partial class JobsGroupBuilder
{
    partial void Configure()
    {
        HandlePaymentTransactionCompleted
            .ThrottleRequestLogging().ForOneHour()
            .ThrottleExceptionLogging().For(5).Minutes()
            .RunContinuously()
            .Interval(TimeSpan.FromSeconds(10))
            .AddSchedule
            (
                Schedule.EveryDay().At(TimeRange.AllDay())
            );
    }
}
