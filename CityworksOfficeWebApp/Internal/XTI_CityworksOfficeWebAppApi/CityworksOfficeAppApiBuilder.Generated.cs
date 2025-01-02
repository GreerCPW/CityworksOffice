using XTI_CityworksOfficeWebAppApi.Home;
using XTI_CityworksOfficeWebAppApi.Receivables;

// Generated Code
#nullable enable
namespace XTI_CityworksOfficeWebAppApi;
public sealed partial class CityworksOfficeAppApiBuilder
{
    private readonly AppApi source;
    private readonly IServiceProvider sp;
    public CityworksOfficeAppApiBuilder(IServiceProvider sp, IAppApiUser user)
    {
        source = new AppApi(sp, CityworksOfficeAppKey.Value, user);
        this.sp = sp;
        Home = new HomeGroupBuilder(source.AddGroup("Home"));
        Receivables = new ReceivablesGroupBuilder(source.AddGroup("Receivables"));
        Configure();
    }

    partial void Configure();
    public HomeGroupBuilder Home { get; }
    public ReceivablesGroupBuilder Receivables { get; }

    public CityworksOfficeAppApi Build() => new CityworksOfficeAppApi(source, this);
}