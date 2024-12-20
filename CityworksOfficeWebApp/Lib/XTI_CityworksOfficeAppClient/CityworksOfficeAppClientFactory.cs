// Generated Code
namespace XTI_CityworksOfficeAppClient;
public sealed partial class CityworksOfficeAppClientFactory
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly XtiTokenAccessorFactory xtiTokenAccessorFactory;
    private readonly AppClientUrl clientUrl;
    private readonly IAppClientRequestKey requestKey;
    private readonly CityworksOfficeAppClientVersion version;
    public CityworksOfficeAppClientFactory(IHttpClientFactory httpClientFactory, XtiTokenAccessorFactory xtiTokenAccessorFactory, AppClientUrl clientUrl, IAppClientRequestKey requestKey, CityworksOfficeAppClientVersion version)
    {
        this.httpClientFactory = httpClientFactory;
        this.xtiTokenAccessorFactory = xtiTokenAccessorFactory;
        this.clientUrl = clientUrl;
        this.requestKey = requestKey;
        this.version = version;
    }

    public CityworksOfficeAppClient Create() => new CityworksOfficeAppClient(httpClientFactory, xtiTokenAccessorFactory, clientUrl, requestKey, version);
}