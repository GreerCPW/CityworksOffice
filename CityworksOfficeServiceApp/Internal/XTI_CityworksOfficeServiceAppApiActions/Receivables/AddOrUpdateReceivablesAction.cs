using XTI_CityworksOfficeAppClient;

namespace XTI_CityworksOfficeServiceAppApiActions.Receivables;

public sealed class AddOrUpdateReceivablesAction : AppAction<EmptyRequest, EmptyActionResult>
{
    private readonly CityworksOfficeAppClient cwOfficeClient;

    public AddOrUpdateReceivablesAction(CityworksOfficeAppClient cwOfficeClient)
    {
        this.cwOfficeClient = cwOfficeClient;
    }

    public Task<EmptyActionResult> Execute(EmptyRequest model, CancellationToken ct) =>
        cwOfficeClient.Receivables.AddOrUpdateReceivables(ct);
}