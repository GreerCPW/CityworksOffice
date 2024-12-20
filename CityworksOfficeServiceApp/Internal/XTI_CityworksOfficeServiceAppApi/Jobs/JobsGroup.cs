namespace XTI_CityworksOfficeServiceAppApi.Jobs;

public sealed class JobsGroup : AppApiGroupWrapper
{
    public JobsGroup(AppApiGroup source, IServiceProvider sp)
        : base(source)
    {
        HandlePaymentTransactionCompleted = source.AddAction
        (
            nameof(HandlePaymentTransactionCompleted), 
            () => sp.GetRequiredService<HandlePaymentTransactionCompletedAction>()
        );
    }

    public AppApiAction<EmptyRequest, EmptyActionResult> HandlePaymentTransactionCompleted { get; }
}