using XTI_Core;
using XTI_Schedule;

namespace XTI_CityworksOfficeServiceAppApi.Receivables;

partial class ReceivablesGroupBuilder
{
    partial void Configure()
    {
        AddOrUpdateReceivables
            .RunContinuously()
            .Interval(TimeSpan.FromHours(1))
            .AddSchedule
            (
                Schedule.EveryDay().At(TimeRange.From(new TimeOnly(6, 0)).For(17).Hours())
            );
    }
}
