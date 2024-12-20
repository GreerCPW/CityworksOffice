namespace XTI_CityworksOfficeWebAppApi.Receivables;

public sealed class ReceivablesGroup : AppApiGroupWrapper
{
    public ReceivablesGroup(AppApiGroup source, IServiceProvider sp)
        : base(source)
    {
        AddOrUpdateReceivables = source.AddAction(nameof(AddOrUpdateReceivables), () => sp.GetRequiredService<AddOrUpdateReceivablesAction>());
    }

    public AppApiAction<EmptyRequest, EmptyActionResult> AddOrUpdateReceivables { get; }
}