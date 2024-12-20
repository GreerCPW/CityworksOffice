// Generated Code
namespace XTI_CityworksOfficeAppClient;
public sealed partial class ReceivablesGroup : AppClientGroup
{
    public ReceivablesGroup(IHttpClientFactory httpClientFactory, XtiTokenAccessor xtiTokenAccessor, AppClientUrl clientUrl, AppClientOptions options) : base(httpClientFactory, xtiTokenAccessor, clientUrl, options, "Receivables")
    {
        Actions = new ReceivablesGroupActions(AddOrUpdateReceivables: CreatePostAction<EmptyRequest, EmptyActionResult>("AddOrUpdateReceivables"));
    }

    public ReceivablesGroupActions Actions { get; }

    public Task<EmptyActionResult> AddOrUpdateReceivables(CancellationToken ct = default) => Actions.AddOrUpdateReceivables.Post("", new EmptyRequest(), ct);
    public sealed record ReceivablesGroupActions(AppClientPostAction<EmptyRequest, EmptyActionResult> AddOrUpdateReceivables);
}