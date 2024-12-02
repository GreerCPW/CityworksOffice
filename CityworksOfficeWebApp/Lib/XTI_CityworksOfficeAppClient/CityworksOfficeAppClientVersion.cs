// Generated Code
namespace XTI_CityworksOfficeAppClient;
public sealed partial class CityworksOfficeAppClientVersion
{
    public static CityworksOfficeAppClientVersion Version(string value) => new CityworksOfficeAppClientVersion(value);
    public CityworksOfficeAppClientVersion(IHostEnvironment hostEnv) : this(getValue(hostEnv))
    {
    }

    private static string getValue(IHostEnvironment hostEnv)
    {
        string value;
        if (hostEnv.IsProduction())
        {
            value = "Current";
        }
        else
        {
            value = "Current";
        }

        return value;
    }

    private CityworksOfficeAppClientVersion(string value)
    {
        Value = value;
    }

    public string Value { get; }
}