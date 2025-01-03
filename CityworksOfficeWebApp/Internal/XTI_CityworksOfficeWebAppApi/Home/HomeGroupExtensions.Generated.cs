using XTI_CityworksOfficeWebAppApiActions.Home;

// Generated Code
namespace XTI_CityworksOfficeWebAppApi;
internal static partial class HomeGroupExtensions
{
    internal static void AddHomeServices(this IServiceCollection services)
    {
        services.AddScoped<IndexAction>();
    }
}