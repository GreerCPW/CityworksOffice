using XTI_App.Abstractions;

namespace CityworksOfficeSetupApp;

internal sealed class CityworksOfficeAppSetup : IAppSetup
{
    public Task Run(AppVersionKey versionKey)
    {
        return Task.CompletedTask;
    }
}
