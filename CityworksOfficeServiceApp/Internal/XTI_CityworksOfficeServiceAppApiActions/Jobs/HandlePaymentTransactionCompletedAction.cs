using CPW_HandlePaymentTransactionCompleted;
using CPW_PaymentTransaction.Events;
using XTI_Jobs;

namespace XTI_CityworksOfficeServiceAppApiActions.Jobs;

public sealed class HandlePaymentTransactionCompletedAction : AppAction<EmptyRequest, EmptyActionResult>
{
    private readonly EventMonitorBuilder eventMonitorBuilder;
    private readonly HandlePaymentTransactionCompletedActionFactory jobActionFactory;

    public HandlePaymentTransactionCompletedAction
    (
        EventMonitorBuilder eventMonitorBuilder,
        HandlePaymentTransactionCompletedActionFactory jobActionFactory
    )
    {
        this.eventMonitorBuilder = eventMonitorBuilder;
        this.jobActionFactory = jobActionFactory;
    }

    public async Task<EmptyActionResult> Execute(EmptyRequest model, CancellationToken stoppingToken)
    {
        await eventMonitorBuilder
            .When(PaymentTransactionEvents.PaymentTransactionCompleted)
            .Trigger(HandlePaymentTransactionCompletedInfo.JobKey)
            .UseJobActionFactory(jobActionFactory)
            .TransformEventData(new UnchangedEventData())
            .Build()
            .Run(stoppingToken);
        return new EmptyActionResult();
    }
}
