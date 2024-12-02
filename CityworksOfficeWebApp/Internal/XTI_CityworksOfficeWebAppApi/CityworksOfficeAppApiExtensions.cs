namespace XTI_CityworksOfficeWebAppApi;

public static class CityworksOfficeAppApiExtensions
{
    public static void AddCityworksOfficeAppApiServices(this IServiceCollection services)
    {
        services.AddHomeGroupServices();
    }
}