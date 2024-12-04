using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using XTI_Jobs;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class LoadTaskResolutionsAction : JobAction<HandlePaymentTransactionData>
{
    private readonly ICityworksService cwService;
    private readonly List<long> taskIDs = new();

    public LoadTaskResolutionsAction(ICityworksService cwService, TriggeredJobTask task) : base(task)
    {
        this.cwService = cwService;
    }

    protected override async Task Execute(CancellationToken stoppingToken, TriggeredJobTask task, JobActionResultBuilder next, HandlePaymentTransactionData data)
    {
        taskIDs.Clear();
        var caseDetail = await cwService.GetCaseDetail(data.CaseID, stoppingToken);
        var paidFees = data.AppliedPayments
            .Select(ap => ap.CasePaymentID)
            .Select(pid => caseDetail.FeeDetails.FirstOrDefault(fd => fd.Payments.Any(p => p.ID == pid)) ?? new())
            .Where(fd => fd.IsFound())
            .Select(fd => fd.Fee)
            .ToArray();
        var waterFees = new WaterFees(caseDetail, paidFees);
        var hydrantFees = new HydrantFees(caseDetail, paidFees);
        var electricFees = new ElectricFees(caseDetail, paidFees);
        var gasFees = new GasFees(caseDetail, paidFees);
        var sewerFees = new SewerFees(caseDetail, paidFees);
        IFees[] fees = [waterFees, hydrantFees, electricFees, gasFees, sewerFees];
        foreach (var fee in fees)
        {
            TryResolvePaidTaskAfterCurrentPayment(next, fee);
        }
        if (caseDetail.FeeDetails.All(fd => fd.Fee.Amount <= fd.Fee.PaymentAmount))
        {
            foreach (var fee in fees)
            {
                TryResolvePaidTaskAfterAllFeesArePaid(next, fee);
            }
        }
    }

    private void TryResolvePaidTaskAfterCurrentPayment(JobActionResultBuilder next, IFees fees)
    {
        if (fees.IsCurrentPayment && fees.HasFee && fees.IsPaidInFull)
        {
            AddNextResolveCaseTask(next, fees.PaidTask, fees.ResultCode);
        }
    }

    private void TryResolvePaidTaskAfterAllFeesArePaid(JobActionResultBuilder next, IFees fees)
    {
        if (!fees.HasFee || (!fees.IsCurrentPayment && fees.IsPaidInFull))
        {
            AddNextResolveCaseTask(next, fees.PaidTask, fees.ResultCode);
        }
    }

    private void AddNextResolveCaseTask(JobActionResultBuilder next, CaseTaskModel paidTask, string resultCode)
    {
        if
        (
            paidTask.IsFound() &&
            !string.IsNullOrWhiteSpace(resultCode) &&
            !paidTask.ResultCode.Equals(resultCode, StringComparison.OrdinalIgnoreCase)
        )
        {
            if (taskIDs.Contains(paidTask.ID))
            {
                throw new Exception($"Task {paidTask.ID} has already been resolved. Unable to set result to '{resultCode}'.");
            }
            next.AddNext
            (
                HandlePaymentTransactionCompletedInfo.ResolveCaseTask,
                new ResolveCaseTaskRequest
                (
                    id: paidTask.ID,
                    resultCode: resultCode
                )
            );
        }
    }
}
