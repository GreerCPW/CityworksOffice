using CPW_ExpandedCityworksDB;
using XTI_App.Abstractions;
using XTI_CityworksOfficeWebAppApi;
using XTI_DB;
using XTI_Hub.Abstractions;
using XTI_HubAppClient;

namespace CityworksOfficeSetupApp;

internal sealed class CityworksOfficeAppSetup : IAppSetup
{
    private readonly HubAppClient hubClient;
    private readonly DbAdmin<ExpandedCityworksDbContext> dbAdmin;

    public CityworksOfficeAppSetup(HubAppClient hubClient, DbAdmin<ExpandedCityworksDbContext> dbAdmin)
    {
        this.hubClient = hubClient;
        this.dbAdmin = dbAdmin;
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
                    AppKey.WebApp("PaymentTransaction"),
                    AppRoleName.Admin
                )
            )
        );
        await dbAdmin.Update();
    }
}
