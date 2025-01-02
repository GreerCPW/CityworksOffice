using XTI_CityworksOfficeServiceAppApiActions.Jobs;

// Generated Code
#nullable enable
namespace XTI_CityworksOfficeServiceAppApi.Jobs;
public sealed partial class JobsGroup : AppApiGroupWrapper
{
    internal JobsGroup(AppApiGroup source, JobsGroupBuilder builder) : base(source)
    {
        HandlePaymentTransactionCompleted = builder.HandlePaymentTransactionCompleted.Build();
        Configure();
    }

    partial void Configure();
    public AppApiAction<EmptyRequest, EmptyActionResult> HandlePaymentTransactionCompleted { get; }
}