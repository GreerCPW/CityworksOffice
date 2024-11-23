using XTI_CityworksOfficeServiceAppApi.Jobs;

namespace XTI_CityworksOfficeServiceAppApi;

internal static class JobsGroupExtensions
{
    public static void AddJobsGroupServices(this IServiceCollection services)
    {
        services.AddScoped<HandlePaymentTransactionCompletedAction>();
    }
}