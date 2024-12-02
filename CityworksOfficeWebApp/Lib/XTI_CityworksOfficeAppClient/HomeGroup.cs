// Generated Code
namespace XTI_CityworksOfficeAppClient;
public sealed partial class HomeGroup : AppClientGroup
{
    public HomeGroup(IHttpClientFactory httpClientFactory, XtiTokenAccessor xtiTokenAccessor, AppClientUrl clientUrl, AppClientOptions options) : base(httpClientFactory, xtiTokenAccessor, clientUrl, options, "Home")
    {
        Actions = new HomeGroupActions(Index: CreateGetAction<EmptyRequest>("Index"));
    }

    public HomeGroupActions Actions { get; }

    public sealed record HomeGroupActions(AppClientGetAction<EmptyRequest> Index);
}