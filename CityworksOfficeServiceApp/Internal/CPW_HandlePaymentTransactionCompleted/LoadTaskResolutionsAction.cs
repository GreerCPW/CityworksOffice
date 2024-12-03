using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using XTI_Jobs;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class LoadTaskResolutionsAction : JobAction<HandlePaymentTransactionData>
{
    private const string WaterCapacityFeeCode = "WATER CAP";
    private const string WaterTapFeeCode = "WATER TAP";
    private const string IrrigationCapacityFeeCode = "IRR CAP";
    private const string IrrigationTapFeeCode = "IRR TAP";
    private const string FireHydrantFeeCode = "FIRE HYD";
    private const string ElectricFeeCode = "EL DEV";
    private const string GasFeeCode = "GAS FEES";
    private const string SewerCapacityFeeCode = "SEWER CAP";
    private const string SewerTapFeeCode = "SEWER TAP";

    private const string WaterPaidTaskCode = "WATER_PAID";
    private const string FireHydrantPaidTaskCode = "HYD_PAID";
    private const string ElectricPaidTaskCode = "ELEC_PAID";
    private const string GasPaidTaskCode = "GAS_PAID";
    private const string SewerPaidTaskCode = "SEWER_PAID";

    private const string NotApplicableResultCode = "NOT APP";
    private const string PaidActiveResultCode = "PAIDACTIVE";

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
        TryResolveWaterPaidTask(caseDetail, paidFees, next);
        TryResolveHydrantPaidTask(caseDetail, paidFees, next);
        TryResolveElectricPaidTask(caseDetail, paidFees, next);
        TryResolveGasPaidTask(caseDetail, paidFees, next);
        TryResolveSewerPaidTask(caseDetail, paidFees, next);
        TryResolvePaidNotApplicableTasks(caseDetail, next);
    }

    private void TryResolveWaterPaidTask(CaseDetailModel caseDetail, CaseFeeModel[] paidFees, JobActionResultBuilder next)
    {
        var waterCapacityFee = paidFees.FirstOrDefault(f => f.HasCode(WaterCapacityFeeCode)) ?? new();
        var waterTapFee = paidFees.FirstOrDefault(f => f.HasCode(WaterTapFeeCode)) ?? new();
        var irrigationCapacityFee = paidFees.FirstOrDefault(f => f.HasCode(IrrigationCapacityFeeCode)) ?? new();
        var irrigationTapFee = paidFees.FirstOrDefault(f => f.HasCode(IrrigationTapFeeCode)) ?? new();
        if (waterCapacityFee.IsFound() || waterTapFee.IsFound() || irrigationCapacityFee.IsFound() || irrigationTapFee.IsFound())
        {
            if (!waterCapacityFee.IsFound())
            {
                waterCapacityFee = caseDetail.GetFeeDetailOrDefault(WaterCapacityFeeCode).Fee;
            }
            if (!waterTapFee.IsFound())
            {
                waterTapFee = caseDetail.GetFeeDetailOrDefault(WaterTapFeeCode).Fee;
            }
            if (!irrigationCapacityFee.IsFound())
            {
                irrigationCapacityFee = caseDetail.GetFeeDetailOrDefault(IrrigationCapacityFeeCode).Fee;
            }
            if (!irrigationTapFee.IsFound())
            {
                irrigationTapFee = caseDetail.GetFeeDetailOrDefault(IrrigationTapFeeCode).Fee;
            }
            if
            (
                waterCapacityFee.IsPaidInFull() && waterTapFee.IsPaidInFull() &&
                irrigationCapacityFee.IsPaidInFull() && irrigationTapFee.IsPaidInFull()
            )
            {
                var waterPaidTask = caseDetail.GetTaskDetailOrDefault(WaterPaidTaskCode).Task;
                if (waterPaidTask.IsFound())
                {
                    var hasIrrigation = irrigationCapacityFee.IsFound() || irrigationTapFee.IsFound();
                    var hasIrrigationAndWater =
                        hasIrrigation && (waterCapacityFee.IsFound() || waterTapFee.IsFound());
                    var resultCode =
                        hasIrrigationAndWater ? "WT WTR IRR" :
                        hasIrrigation ? "WT IRRIG" :
                        "WT WATER";
                    AddNextResolveCaseTask(next, waterPaidTask, resultCode);
                }
            }
        }
    }

    private void TryResolveHydrantPaidTask(CaseDetailModel caseDetail, CaseFeeModel[] paidFees, JobActionResultBuilder next)
    {
        var hydrantFee = paidFees.FirstOrDefault(f => f.HasCode(FireHydrantFeeCode)) ?? new();
        if (hydrantFee.IsFound() && hydrantFee.IsPaidInFull())
        {
            var hydrantPaidTask = caseDetail.GetTaskDetailOrDefault(FireHydrantPaidTaskCode).Task;
            if (hydrantPaidTask.IsFound())
            {
                AddNextResolveCaseTask(next, hydrantPaidTask, PaidActiveResultCode);
            }
        }
    }

    private void TryResolveElectricPaidTask(CaseDetailModel caseDetail, CaseFeeModel[] paidFees, JobActionResultBuilder next)
    {
        var electricFee = paidFees.FirstOrDefault(f => f.HasCode(ElectricFeeCode)) ?? new();
        if (electricFee.IsFound() && electricFee.IsPaidInFull())
        {
            var electricPaidTask = caseDetail.GetTaskDetailOrDefault(ElectricPaidTaskCode).Task;
            if (electricPaidTask.IsFound())
            {
                var cityLimits = caseDetail.GetDataDetailOrDefault("CITY LIMIT", "IN OR OUT").Value;
                var resultCode = cityLimits.Equals("Outside", StringComparison.OrdinalIgnoreCase) ? "ELEC CNTY" : "ELEC CITY";
                AddNextResolveCaseTask(next, electricPaidTask, resultCode);
            }
        }
    }

    private void TryResolveGasPaidTask(CaseDetailModel caseDetail, CaseFeeModel[] paidFees, JobActionResultBuilder next)
    {
        var gasServiceFee = paidFees.FirstOrDefault(f => f.HasCode(GasFeeCode)) ?? new();
        if (gasServiceFee.IsFound() && gasServiceFee.IsPaidInFull())
        {
            var gasPaidTask = caseDetail.GetTaskDetailOrDefault(GasPaidTaskCode).Task;
            if (gasPaidTask.IsFound())
            {
                AddNextResolveCaseTask(next, gasPaidTask, PaidActiveResultCode);
            }
        }
    }

    private void TryResolveSewerPaidTask(CaseDetailModel caseDetail, CaseFeeModel[] paidFees, JobActionResultBuilder next)
    {
        var sewerCapacityFee = paidFees.FirstOrDefault(f => f.HasCode(SewerCapacityFeeCode)) ?? new();
        var sewerTapFee = paidFees.FirstOrDefault(f => f.HasCode(SewerTapFeeCode)) ?? new();
        if (sewerCapacityFee.IsFound() || sewerTapFee.IsFound())
        {
            if (!sewerCapacityFee.IsFound())
            {
                sewerCapacityFee = caseDetail.GetFeeDetailOrDefault(SewerCapacityFeeCode).Fee;
            }
            if (!sewerTapFee.IsFound())
            {
                sewerTapFee = caseDetail.GetFeeDetailOrDefault(SewerTapFeeCode).Fee;
            }
            if (sewerCapacityFee.IsPaidInFull() && sewerTapFee.IsPaidInFull())
            {
                var sewerPaidTask = caseDetail.GetTaskDetailOrDefault(SewerPaidTaskCode).Task;
                if (sewerPaidTask.IsFound())
                {
                    AddNextResolveCaseTask(next, sewerPaidTask, PaidActiveResultCode);
                }
            }
        }
    }

    private void TryResolvePaidNotApplicableTasks(CaseDetailModel caseDetail, JobActionResultBuilder next)
    {
        if (caseDetail.FeeDetails.All(fd => fd.Fee.Amount <= fd.Fee.PaymentAmount))
        {
            TryResolveWaterPaidNotApplicableTask(caseDetail, next);
            TryResolveHydrantPaidNotApplicableTask(caseDetail, next);
            TryResolveElectricPaidNotApplicableTask(caseDetail, next);
            TryResolveGasPaidNotApplicableTask(caseDetail, next);
            TryResolveSewerPaidNotApplicableTask(caseDetail, next);
        }
    }

    private void TryResolveWaterPaidNotApplicableTask(CaseDetailModel caseDetail, JobActionResultBuilder next)
    {
        var waterCapacityFee = caseDetail.GetFeeDetailOrDefault(WaterCapacityFeeCode).Fee;
        var waterTapFee = caseDetail.GetFeeDetailOrDefault(WaterTapFeeCode).Fee;
        var irrigationCapacityFee = caseDetail.GetFeeDetailOrDefault(IrrigationCapacityFeeCode).Fee;
        var irrigationTapFee = caseDetail.GetFeeDetailOrDefault(IrrigationTapFeeCode).Fee;
        if (waterCapacityFee.Amount == 0 && waterTapFee.Amount == 0 && irrigationCapacityFee.Amount == 0 && irrigationTapFee.Amount == 0)
        {
            var waterPaidTask = caseDetail.GetTaskDetailOrDefault(WaterPaidTaskCode).Task;
            if (waterPaidTask.IsFound())
            {
                AddNextResolveCaseTask(next, waterPaidTask, NotApplicableResultCode);
            }
        }
    }

    private void TryResolveHydrantPaidNotApplicableTask(CaseDetailModel caseDetail, JobActionResultBuilder next)
    {
        var hydrantFee = caseDetail.GetFeeDetailOrDefault(FireHydrantFeeCode).Fee;
        if (hydrantFee.Amount == 0)
        {
            var hydrantPaidTask = caseDetail.GetTaskDetailOrDefault(FireHydrantPaidTaskCode).Task;
            if (hydrantPaidTask.IsFound())
            {
                AddNextResolveCaseTask(next, hydrantPaidTask, NotApplicableResultCode);
            }
        }
    }

    private void TryResolveElectricPaidNotApplicableTask(CaseDetailModel caseDetail, JobActionResultBuilder next)
    {
        var electricFee = caseDetail.GetFeeDetailOrDefault(ElectricFeeCode).Fee;
        if (electricFee.Amount == 0)
        {
            var electricPaidTask = caseDetail.GetTaskDetailOrDefault(ElectricPaidTaskCode).Task;
            if (electricPaidTask.IsFound())
            {
                AddNextResolveCaseTask(next, electricPaidTask, NotApplicableResultCode);
            }
        }
    }

    private void TryResolveGasPaidNotApplicableTask(CaseDetailModel caseDetail, JobActionResultBuilder next)
    {
        var gasFee = caseDetail.GetFeeDetailOrDefault(GasFeeCode).Fee;
        if (gasFee.Amount == 0)
        {
            var gasPaidTask = caseDetail.GetTaskDetailOrDefault(GasPaidTaskCode).Task;
            if (gasPaidTask.IsFound())
            {
                AddNextResolveCaseTask(next, gasPaidTask, NotApplicableResultCode);
            }
        }
    }

    private void TryResolveSewerPaidNotApplicableTask(CaseDetailModel caseDetail, JobActionResultBuilder next)
    {
        var sewerCapacityFee = caseDetail.GetFeeDetailOrDefault(SewerCapacityFeeCode).Fee;
        var sewerTapFee = caseDetail.GetFeeDetailOrDefault(SewerTapFeeCode).Fee;
        if (sewerCapacityFee.Amount == 0 && sewerTapFee.Amount == 0)
        {
            var sewerPaidTask = caseDetail.GetTaskDetailOrDefault(SewerPaidTaskCode).Task;
            if (sewerPaidTask.IsFound())
            {
                AddNextResolveCaseTask(next, sewerPaidTask, NotApplicableResultCode);
            }
        }
    }

    private void AddNextResolveCaseTask(JobActionResultBuilder next, CaseTaskModel paidTask, string resultCode)
    {
        if (!paidTask.ResultCode.Equals(resultCode, StringComparison.OrdinalIgnoreCase))
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
