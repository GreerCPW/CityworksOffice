using XTI_App.Abstractions;
using XTI_CityworksOfficeServiceAppApi;
using XTI_Hub.Abstractions;
using XTI_HubAppClient;

namespace CityworksOfficeSetupApp;

internal sealed class CityworksOfficeAppSetup : IAppSetup
{
    private readonly HubAppClient hubClient;
    private readonly CityworksOfficeJobSetup jobSetup;

    public CityworksOfficeAppSetup(HubAppClient hubClient, CityworksOfficeJobSetup jobSetup)
    {
        this.hubClient = hubClient;
        this.jobSetup = jobSetup;
    }

    public async Task Run(AppVersionKey versionKey)
    {
        await hubClient.Install.SetUserAccess
        (
            new SetUserAccessRequest
            (
                new SystemUserName(CityworksOfficeInfo.AppKey, Environment.MachineName).UserName,
                new SetUserAccessRoleRequest
                (
                    AppKey.WebApp("Cityworks"),
                    AppRoleName.Admin
                ),
                new SetUserAccessRoleRequest
                (
                    AppKey.WebApp("Scheduled Jobs"),
                    AppRoleName.Admin
                )
            )
        );
        await jobSetup.Run();
    }
}
