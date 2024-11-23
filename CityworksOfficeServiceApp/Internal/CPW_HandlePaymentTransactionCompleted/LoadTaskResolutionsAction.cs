using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using XTI_Jobs;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class LoadTaskResolutionsAction : JobAction<HandlePaymentTransactionData>
{
    private readonly ICityworksService cwService;

    public LoadTaskResolutionsAction(ICityworksService cwService, TriggeredJobTask task) : base(task)
    {
        this.cwService = cwService;
    }

    protected override async Task Execute(CancellationToken stoppingToken, TriggeredJobTask task, JobActionResultBuilder next, HandlePaymentTransactionData data)
    {
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
    }

    private static void TryResolveWaterPaidTask(CaseDetailModel caseDetail, CaseFeeModel[] paidFees, JobActionResultBuilder next)
    {
        const string waterCapacityFeeCode = "WATER CAP";
        const string waterTapFeeCode = "WATER TAP";
        const string irrigationCapacityFeeCode = "IRR CAP";
        const string irrigationTapFeeCode = "IRR TAP";
        var waterCapacityFee = paidFees.FirstOrDefault(f => f.HasCode(waterCapacityFeeCode)) ?? new();
        var waterTapFee = paidFees.FirstOrDefault(f => f.HasCode(waterTapFeeCode)) ?? new();
        var irrigationCapacityFee = paidFees.FirstOrDefault(f => f.HasCode(irrigationCapacityFeeCode)) ?? new();
        var irrigationTapFee = paidFees.FirstOrDefault(f => f.HasCode(irrigationTapFeeCode)) ?? new();
        if (waterCapacityFee.IsFound() || waterTapFee.IsFound() || irrigationCapacityFee.IsFound() || irrigationTapFee.IsFound())
        {
            if (!waterCapacityFee.IsFound())
            {
                waterCapacityFee = caseDetail.GetFeeDetailOrDefault(waterCapacityFeeCode).Fee;
            }
            if (!waterTapFee.IsFound())
            {
                waterTapFee = caseDetail.GetFeeDetailOrDefault(waterTapFeeCode).Fee;
            }
            if (!irrigationCapacityFee.IsFound())
            {
                irrigationCapacityFee = caseDetail.GetFeeDetailOrDefault(irrigationCapacityFeeCode).Fee;
            }
            if (!irrigationTapFee.IsFound())
            {
                irrigationTapFee = caseDetail.GetFeeDetailOrDefault(irrigationTapFeeCode).Fee;
            }
            if
            (
                waterCapacityFee.IsPaidInFull() && waterTapFee.IsPaidInFull() &&
                irrigationCapacityFee.IsPaidInFull() && irrigationTapFee.IsPaidInFull()
            )
            {
                var waterPaidTask = caseDetail.GetTaskDetailOrDefault("WATER_PAID").Task;
                if (waterPaidTask.IsFound())
                {
                    var hasIrrigation = irrigationCapacityFee.IsFound() || irrigationTapFee.IsFound();
                    var hasIrrigationAndWater =
                        hasIrrigation && (waterCapacityFee.IsFound() || waterTapFee.IsFound());
                    var resultCode =
                        hasIrrigationAndWater ? "WT WTR IRR" :
                        hasIrrigation ? "WT IRRIG" :
                        "WT WATER";
                    next.AddNext
                    (
                        HandlePaymentTransactionCompletedInfo.ResolveCaseTask,
                        new ResolveCaseTaskRequest
                        (
                            id: waterPaidTask.ID,
                            resultCode: resultCode
                        )
                    );
                }
            }
        }
    }

    private static void TryResolveHydrantPaidTask(CaseDetailModel caseDetail, CaseFeeModel[] paidFees, JobActionResultBuilder next)
    {
        var hydrantFee = paidFees.FirstOrDefault(f => f.HasCode("FIRE HYD")) ?? new();
        if (hydrantFee.IsFound() && hydrantFee.IsPaidInFull())
        {
            var hydrantPaidTask = caseDetail.GetTaskDetailOrDefault("HYD_PAID").Task;
            if (hydrantPaidTask.IsFound())
            {
                next.AddNext
                (
                    HandlePaymentTransactionCompletedInfo.ResolveCaseTask,
                    new ResolveCaseTaskRequest(id: hydrantPaidTask.ID, resultCode: "PAIDACTIVE")
                );
            }
        }
    }

    private static void TryResolveElectricPaidTask(CaseDetailModel caseDetail, CaseFeeModel[] paidFees, JobActionResultBuilder next)
    {
        var electricFee = paidFees.FirstOrDefault(f => f.HasCode("EL DEV")) ?? new();
        if (electricFee.IsFound() && electricFee.IsPaidInFull())
        {
            var electricPaidTask = caseDetail.GetTaskDetailOrDefault("ELEC_PAID").Task;
            if (electricPaidTask.IsFound())
            {
                var cityLimits = caseDetail.GetDataDetailOrDefault("CITY LIMIT", "IN OR OUT").Value;
                next.AddNext
                (
                    HandlePaymentTransactionCompletedInfo.ResolveCaseTask,
                    new ResolveCaseTaskRequest
                    (
                        id: electricPaidTask.ID,
                        resultCode: cityLimits.Equals("Outside", StringComparison.OrdinalIgnoreCase) ? "ELEC CNTY" : "ELEC CITY"
                    )
                );
            }
        }
    }

    private static void TryResolveGasPaidTask(CaseDetailModel caseDetail, CaseFeeModel[] paidFees, JobActionResultBuilder next)
    {
        var gasServiceFee = paidFees.FirstOrDefault(f => f.HasCode("GAS FEES")) ?? new();
        if (gasServiceFee.IsFound() && gasServiceFee.IsPaidInFull())
        {
            var gasPaidTask = caseDetail.GetTaskDetailOrDefault("GAS_PAID").Task;
            if (gasPaidTask.IsFound())
            {
                next.AddNext
                (
                    HandlePaymentTransactionCompletedInfo.ResolveCaseTask,
                    new ResolveCaseTaskRequest(id: gasPaidTask.ID, resultCode: "PAIDACTIVE")
                );
            }
        }
    }

    private static void TryResolveSewerPaidTask(CaseDetailModel caseDetail, CaseFeeModel[] paidFees, JobActionResultBuilder next)
    {
        const string sewerCapacityFeeCode = "SEWER CAP";
        const string sewerTapFeeCode = "SEWER TAP";
        var sewerCapacityFee = paidFees.FirstOrDefault(f => f.HasCode(sewerCapacityFeeCode)) ?? new();
        var sewerTapFee = paidFees.FirstOrDefault(f => f.HasCode(sewerTapFeeCode)) ?? new();
        if (sewerCapacityFee.IsFound() || sewerTapFee.IsFound())
        {
            if (!sewerCapacityFee.IsFound())
            {
                sewerCapacityFee = caseDetail.GetFeeDetailOrDefault(sewerCapacityFeeCode).Fee;
            }
            if (!sewerTapFee.IsFound())
            {
                sewerTapFee = caseDetail.GetFeeDetailOrDefault(sewerTapFeeCode).Fee;
            }
            if (sewerCapacityFee.IsPaidInFull() && sewerTapFee.IsPaidInFull())
            {
                var sewerPaidTask = caseDetail.GetTaskDetailOrDefault("SEWER_PAID").Task;
                if (sewerPaidTask.IsFound())
                {
                    next.AddNext
                    (
                        HandlePaymentTransactionCompletedInfo.ResolveCaseTask,
                        new ResolveCaseTaskRequest
                        (
                            id: sewerPaidTask.ID,
                            resultCode: "PAIDACTIVE"
                        )
                    );
                }
            }
        }
    }
}
