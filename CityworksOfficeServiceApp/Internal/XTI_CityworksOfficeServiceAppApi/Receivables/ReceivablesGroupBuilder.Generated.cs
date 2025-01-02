using XTI_CityworksOfficeServiceAppApiActions.Receivables;

// Generated Code
#nullable enable
namespace XTI_CityworksOfficeServiceAppApi.Receivables;
public sealed partial class ReceivablesGroupBuilder
{
    private readonly AppApiGroup source;
    internal ReceivablesGroupBuilder(AppApiGroup source)
    {
        this.source = source;
        AddOrUpdateReceivables = source.AddAction<EmptyRequest, EmptyActionResult>("AddOrUpdateReceivables").WithExecution<AddOrUpdateReceivablesAction>();
        Configure();
    }

    partial void Configure();
    public AppApiActionBuilder<EmptyRequest, EmptyActionResult> AddOrUpdateReceivables { get; }

    public ReceivablesGroup Build() => new ReceivablesGroup(source, this);
}