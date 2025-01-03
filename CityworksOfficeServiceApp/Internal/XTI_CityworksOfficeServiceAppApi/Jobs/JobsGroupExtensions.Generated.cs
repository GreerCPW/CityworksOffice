using XTI_CityworksOfficeServiceAppApiActions.Jobs;

// Generated Code
namespace XTI_CityworksOfficeServiceAppApi;
internal static partial class JobsGroupExtensions
{
    internal static void AddJobsServices(this IServiceCollection services)
    {
        services.AddScoped<HandlePaymentTransactionCompletedAction>();
    }
}