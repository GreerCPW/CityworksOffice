namespace XTI_CityworksOfficeServiceAppApi;

public sealed partial class CityworksOfficeAppApi : AppApiWrapper
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
                ""
            )
        )
    {
        createJobsGroup(sp);
    }

    partial void createJobsGroup(IServiceProvider sp);
}