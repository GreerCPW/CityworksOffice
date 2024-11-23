using System.Text.Json;
using XTI_Core;

namespace CityworksOfficeServiceAppIntegrationTests;

internal static class TestExtensions
{
    public static void WriteToConsole(this object data) =>
        Console.WriteLine
        (
            XtiSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })
        );
}
