// Generated Code
using Microsoft.Extensions.DependencyInjection;

namespace XTI_CityworksOfficeAppClient;
public static class CityworksOfficeAppClientExtensions
{
    public static void AddCityworksOfficeAppClient(this IServiceCollection services)
    {
        services.AddScoped<CityworksOfficeAppClientFactory>();
        services.AddScoped(sp => sp.GetRequiredService<CityworksOfficeAppClientFactory>().Create());
        services.AddScoped<CityworksOfficeAppClientVersion>();
    }
}