using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using CPW_PaymentTransaction.Abstractions;
using CPW_PaymentTransaction.Events;
using Microsoft.Extensions.Caching.Memory;
using XTI_Jobs;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class LoadCaseDetailAction : JobAction<PaymentTransactionEventData>
{
    private readonly CachedTenderTypes cachedTenderTypes;
    private readonly ICityworksService cwService;

    public LoadCaseDetailAction(IMemoryCache cache, ICityworksService cwService, TriggeredJobTask task) : base(task)
    {
        cachedTenderTypes = new CachedTenderTypes(cache, cwService);
        this.cwService = cwService;
    }

    protected override async Task Execute(CancellationToken stoppingToken, TriggeredJobTask task, JobActionResultBuilder next, PaymentTransactionEventData data)
    {
        if(data.SourceAppCode.Equals("PLL", StringComparison.OrdinalIgnoreCase))
        {
            if (!long.TryParse(data.SourceKey, out var caseID))
            {
                caseID = 0;
            }
            var caseDetail = await cwService.GetCaseDetail(caseID, stoppingToken);
            if (!caseDetail.IsFound())
            {
                throw new Exception($"Case '{data.SourceKey}' was not found.");
            }
            var handleAppliedPayments = new List<HandleAppliedPaymentData>();
            foreach (var lineItem in data.LineItems)
            {
                if (!long.TryParse(lineItem.SourceKey, out var caseFeeID))
                {
                    caseFeeID = 0;
                }
                var caseFee = caseDetail.GetFeeDetailOrDefault(caseFeeID).Fee;
                if (!caseFee.IsFound())
                {
                    throw new Exception($"Fee {caseFeeID} was not found for case {caseID}");
                }
                foreach (var appliedPayment in lineItem.AppliedPayments)
                {
                    var paymentMethod = PaymentMethod.Values.Value(appliedPayment.PaymentMethod);
                    var tenderType = await cachedTenderTypes.FromPaymentMethod(paymentMethod, stoppingToken);
                    var handleAppliedPayment = new HandleAppliedPaymentData
                    (
                        caseFeeID: caseFeeID,
                        amountPaid: appliedPayment.AmountPaid,
                        tenderTypeID: tenderType.ID,
                        appliedPaymentID: appliedPayment.ID
                    );
                    handleAppliedPayments.Add(handleAppliedPayment);
                }
            }
            var handleTransactionData = new HandlePaymentTransactionData
            (
                paymentTransactionID: data.ID,
                caseID: caseID,
                appliedPayments: handleAppliedPayments.ToArray()
            );
            next.AddNext(HandlePaymentTransactionCompletedInfo.AddCasePayment, handleTransactionData);
        }
    }
}
