using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using XTI_Core;
using XTI_Jobs;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class AddCasePaymentAction : JobAction<HandlePaymentTransactionData>
{
    private readonly ICityworksService cwService;
    private readonly IClock clock;

    public AddCasePaymentAction(ICityworksService cwService, IClock clock, TriggeredJobTask task) : base(task)
    {
        this.cwService = cwService;
        this.clock = clock;
    }

    protected override async Task Execute(CancellationToken stoppingToken, TriggeredJobTask task, JobActionResultBuilder next, HandlePaymentTransactionData data)
    {
        var handleAppliedPayment = data.GetCurrentAppliedPayment();
        var payment = await cwService.AddCasePayment
        (
            new AddCasePaymentRequest
            {
                CaseID = data.CaseID,
                CaseFeeID = handleAppliedPayment.CaseFeeID,
                AmountPaid = handleAppliedPayment.AmountPaid,
                TenderTypeID = handleAppliedPayment.TenderTypeID,
                ReferenceInfo = handleAppliedPayment.ReferenceInfo,
                TimePaid = clock.Now()
            },
            stoppingToken
        );
        handleAppliedPayment.CasePaymentID = payment.ID;
        data.NextAppliedPayment();
        if (data.HasCurrentAppliedPayment())
        {
            next.AddNext(HandlePaymentTransactionCompletedInfo.AddCasePayment, data);
        }
        else
        {
            next.AddNext(HandlePaymentTransactionCompletedInfo.LoadTaskResolutions, data);
            var addReceiptRequest = new AddCaseReceiptRequest
            (
                paymentTransactionID: data.PaymentTransactionID,
                caseID: data.CaseID,
                casePaymentIDs: data.AppliedPayments.Select(ap => ap.CasePaymentID).ToArray()
            );
            next.AddNext(HandlePaymentTransactionCompletedInfo.AddCaseReceipt, addReceiptRequest);
        }
    }
}
