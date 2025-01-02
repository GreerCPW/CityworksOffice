// Generated Code
namespace XTI_CityworksOfficeWebAppApi;
public static partial class CityworksOfficeApiExtensions
{
    public static void AddCityworksOfficeAppApiServices(this IServiceCollection services)
    {
        services.AddHomeServices();
        services.AddReceivablesServices();
        services.AddScoped<AppApiFactory, CityworksOfficeAppApiFactory>();
        services.AddMoreServices();
    }

    static partial void AddMoreServices(this IServiceCollection services);
}