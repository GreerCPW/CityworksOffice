// Generated Code
namespace XTI_CityworksOfficeServiceAppApi;
public static partial class CityworksOfficeApiExtensions
{
    public static void AddCityworksOfficeAppApiServices(this IServiceCollection services)
    {
        services.AddJobsServices();
        services.AddReceivablesServices();
        services.AddScoped<AppApiFactory, CityworksOfficeAppApiFactory>();
        services.AddMoreServices();
    }

    static partial void AddMoreServices(this IServiceCollection services);
}