using XTI_Core;

namespace CPW_ExpandedCityworksDB.SqlServer;

public static class EnvironmentSettings
{
    private static bool isLoaded = false;
    private static string cityworksDatabaseName = "";
    private static string paymentDatabaseName = "";

    public static string GetCityworksDatabaseName()
    {
        if (!isLoaded)
        {
            throw new Exception("Environment has not been loaded");
        }
        return cityworksDatabaseName;
    }

    public static string GetPaymentDatabaseName()
    {
        if (!isLoaded)
        {
            throw new Exception("Environment has not been loaded");
        }
        return paymentDatabaseName;
    }

    public static void LoadEnvironment(XtiEnvironment xtiEnv)
    {
        isLoaded = true;
        cityworksDatabaseName = xtiEnv.IsProduction() ? "CityworksProd" : "[srv-db1].CityworksDev";
        paymentDatabaseName = xtiEnv.IsProduction() ? "XTI_Production_Payment" : "XTI_Development_Payment";
    }
}
