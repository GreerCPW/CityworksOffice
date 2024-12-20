using XTI_CityworksOfficeWebAppApi.Receivables;

namespace XTI_CityworksOfficeWebAppApi;

internal static class ReceivablesGroupExtensions
{
    public static void AddReceivablesGroupServices(this IServiceCollection services)
    {
        services.AddScoped<AddOrUpdateReceivablesAction>();
    }
}