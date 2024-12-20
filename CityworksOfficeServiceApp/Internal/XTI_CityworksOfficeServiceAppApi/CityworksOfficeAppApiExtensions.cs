namespace XTI_CityworksOfficeServiceAppApi;

public static class CityworksOfficeAppApiExtensions
{
    public static void AddCityworksOfficeAppApiServices(this IServiceCollection services)
    {
        services.AddJobsGroupServices();
        services.AddReceivablesGroupServices();
    }
}