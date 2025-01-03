using XTI_CityworksOfficeWebAppApiActions.Receivables;

// Generated Code
#nullable enable
namespace XTI_CityworksOfficeWebAppApi.Receivables;
public sealed partial class ReceivablesGroup : AppApiGroupWrapper
{
    internal ReceivablesGroup(AppApiGroup source, ReceivablesGroupBuilder builder) : base(source)
    {
        AddOrUpdateReceivables = builder.AddOrUpdateReceivables.Build();
        Configure();
    }

    partial void Configure();
    public AppApiAction<EmptyRequest, EmptyActionResult> AddOrUpdateReceivables { get; }
}