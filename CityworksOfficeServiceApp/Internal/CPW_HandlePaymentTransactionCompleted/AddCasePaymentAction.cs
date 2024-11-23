using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using Microsoft.Extensions.Caching.Memory;
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
        await cwService.AddCasePayment
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
        data.NextAppliedPayment();
        if (data.HasCurrentAppliedPayment())
        {
            next.AddNext(HandlePaymentTransactionCompletedInfo.AddCasePayment, data);
        }
    }
}
