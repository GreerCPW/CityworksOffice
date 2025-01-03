using XTI_CityworksOfficeServiceAppApiActions.Jobs;

// Generated Code
#nullable enable
namespace XTI_CityworksOfficeServiceAppApi.Jobs;
public sealed partial class JobsGroupBuilder
{
    private readonly AppApiGroup source;
    internal JobsGroupBuilder(AppApiGroup source)
    {
        this.source = source;
        HandlePaymentTransactionCompleted = source.AddAction<EmptyRequest, EmptyActionResult>("HandlePaymentTransactionCompleted").WithExecution<HandlePaymentTransactionCompletedAction>();
        Configure();
    }

    partial void Configure();
    public AppApiActionBuilder<EmptyRequest, EmptyActionResult> HandlePaymentTransactionCompleted { get; }

    public JobsGroup Build() => new JobsGroup(source, this);
}