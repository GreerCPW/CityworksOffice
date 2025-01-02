using XTI_CityworksOfficeWebAppApi.Receivables;

// Generated Code
namespace XTI_CityworksOfficeWebAppApi;
internal static partial class ReceivablesGroupExtensions
{
    internal static void AddReceivablesServices(this IServiceCollection services)
    {
        services.AddScoped<AddOrUpdateReceivablesAction>();
    }
}