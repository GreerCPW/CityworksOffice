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
        createReceivablesGroup(sp);
    }

    partial void createJobsGroup(IServiceProvider sp);

    partial void createReceivablesGroup(IServiceProvider sp);
}