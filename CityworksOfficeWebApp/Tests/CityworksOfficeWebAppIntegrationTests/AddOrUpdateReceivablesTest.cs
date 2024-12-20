using NUnit.Framework;

namespace CityworksOfficeWebAppIntegrationTests;

internal sealed class AddOrUpdateReceivablesTest
{
    [Test]
    public async Task ShouldAddOrUpdateReceivables()
    {
        var sp = await Setup();
        var tester = CityworksOfficeActionTester.Create(sp, api => api.Receivables.AddOrUpdateReceivables);
        await tester.Execute(new());
    }

    private Task<IServiceProvider> Setup(string envName = "Development")
    {
        var host = new CityworksOfficeTestHost();
        return host.Setup(envName);
    }
}