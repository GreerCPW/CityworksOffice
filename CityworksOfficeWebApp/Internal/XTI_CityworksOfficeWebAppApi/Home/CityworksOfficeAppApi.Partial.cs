using XTI_CityworksOfficeWebAppApi.Home;

namespace XTI_CityworksOfficeWebAppApi;

partial class CityworksOfficeAppApi
{
    private HomeGroup? home;

    public HomeGroup Home { get => home ?? throw new ArgumentNullException(nameof(home)); }

    partial void createHomeGroup(IServiceProvider sp)
    {
        home = new HomeGroup
        (
            source.AddGroup(nameof(Home), ResourceAccess.AllowAuthenticated()),
            sp
        );
    }
}