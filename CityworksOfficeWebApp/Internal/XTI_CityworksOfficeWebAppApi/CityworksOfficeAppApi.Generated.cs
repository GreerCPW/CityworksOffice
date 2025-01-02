using XTI_CityworksOfficeWebAppApi.Home;
using XTI_CityworksOfficeWebAppApi.Receivables;

// Generated Code
#nullable enable
namespace XTI_CityworksOfficeWebAppApi;
public sealed partial class CityworksOfficeAppApi : WebAppApiWrapper
{
    internal CityworksOfficeAppApi(AppApi source, CityworksOfficeAppApiBuilder builder) : base(source)
    {
        Home = builder.Home.Build();
        Receivables = builder.Receivables.Build();
        Configure();
    }

    partial void Configure();
    public HomeGroup Home { get; }
    public ReceivablesGroup Receivables { get; }
}