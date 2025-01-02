using XTI_CityworksOfficeServiceAppApiActions.Receivables;

// Generated Code
namespace XTI_CityworksOfficeServiceAppApi;
internal static partial class ReceivablesGroupExtensions
{
    internal static void AddReceivablesServices(this IServiceCollection services)
    {
        services.AddScoped<AddOrUpdateReceivablesAction>();
    }
}