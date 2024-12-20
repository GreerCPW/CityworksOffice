using XTI_CityworksOfficeWebAppApi.Home;

namespace XTI_CityworksOfficeWebAppApi;

internal static class HomeGroupExtensions
{
    public static void AddHomeGroupServices(this IServiceCollection services)
    {
        services.AddScoped<IndexAction>();
    }
}