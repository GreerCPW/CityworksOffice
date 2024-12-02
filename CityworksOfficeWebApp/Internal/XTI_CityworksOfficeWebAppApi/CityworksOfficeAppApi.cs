using XTI_Core;

namespace XTI_CityworksOfficeWebAppApi;

public sealed partial class CityworksOfficeAppApi : WebAppApiWrapper
{
    public CityworksOfficeAppApi
    (
        IAppApiUser user,
        IServiceProvider sp
    )
        : base
        (
            new AppApi
            (
                CityworksOfficeInfo.AppKey,
                user,
                ResourceAccess.AllowAuthenticated()
                    .WithAllowed(AppRoleName.Admin),
                XtiSerializer.Serialize(new CityworksOfficeAppOptions())
            ),
            sp
        )
    {
        createHomeGroup(sp);
    }

    partial void createHomeGroup(IServiceProvider sp);
}