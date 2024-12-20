// Generated Code
namespace XTI_CityworksOfficeAppClient;
public sealed partial class CityworksOfficeAppClient : AppClient
{
    public CityworksOfficeAppClient(IHttpClientFactory httpClientFactory, XtiTokenAccessorFactory xtiTokenAccessorFactory, AppClientUrl clientUrl, IAppClientRequestKey requestKey, CityworksOfficeAppClientVersion version) : base(httpClientFactory, xtiTokenAccessorFactory, clientUrl, requestKey, "CityworksOffice", version.Value)
    {
        Home = CreateGroup((_clientFactory, _tokenAccessor, _url, _options) => new HomeGroup(_clientFactory, _tokenAccessor, _url, _options));
        Receivables = CreateGroup((_clientFactory, _tokenAccessor, _url, _options) => new ReceivablesGroup(_clientFactory, _tokenAccessor, _url, _options));
    }

    public CityworksOfficeRoleNames RoleNames { get; } = CityworksOfficeRoleNames.Instance;
    public string AppName { get; } = "CityworksOffice";
    public HomeGroup Home { get; }
    public ReceivablesGroup Receivables { get; }
}