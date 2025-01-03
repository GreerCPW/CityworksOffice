// Generated Code
namespace XTI_CityworksOfficeWebAppApi;
public sealed class CityworksOfficeAppApiFactory : AppApiFactory
{
    private readonly IServiceProvider sp;
    public CityworksOfficeAppApiFactory(IServiceProvider sp)
    {
        this.sp = sp;
    }

    public new CityworksOfficeAppApi Create(IAppApiUser user) => (CityworksOfficeAppApi)base.Create(user);
    public new CityworksOfficeAppApi CreateForSuperUser() => (CityworksOfficeAppApi)base.CreateForSuperUser();
    protected override IAppApi _Create(IAppApiUser user) => new CityworksOfficeAppApiBuilder(sp, user).Build();
}