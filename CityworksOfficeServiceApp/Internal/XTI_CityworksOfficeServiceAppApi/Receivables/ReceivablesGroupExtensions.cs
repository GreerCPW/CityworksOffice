using XTI_CityworksOfficeServiceAppApi.Receivables;

namespace XTI_CityworksOfficeServiceAppApi;

internal static class ReceivablesGroupExtensions
{
    public static void AddReceivablesGroupServices(this IServiceCollection services)
    {
        services.AddScoped<AddOrUpdateReceivablesAction>();
    }
}