using CPW_Cityworks.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class WaterFees : IFees
{
    private const string WaterCapacityFeeCode = "WATER CAP";
    private const string WaterTapFeeCode = "WATER TAP";
    private const string IrrigationCapacityFeeCode = "IRR CAP";
    private const string IrrigationTapFeeCode = "IRR TAP";
    private const string WaterPaidTaskCode = "WATER_PAID";

    public WaterFees(CaseDetailModel caseDetail, CaseFeeModel[] paidFees)
    {
        WaterCapacityFee = caseDetail.GetFeeDetailOrDefault(WaterCapacityFeeCode).Fee;
        WaterTapFee = caseDetail.GetFeeDetailOrDefault(WaterTapFeeCode).Fee;
        IrrigationCapacityFee = caseDetail.GetFeeDetailOrDefault(IrrigationCapacityFeeCode).Fee;
        IrrigationTapFee = caseDetail.GetFeeDetailOrDefault(IrrigationTapFeeCode).Fee;
        IsCurrentPayment = paidFees.Contains(WaterCapacityFee) || paidFees.Contains(WaterTapFee) ||
            paidFees.Contains(IrrigationCapacityFee) || paidFees.Contains(IrrigationTapFee);
        HasFee = WaterCapacityFee.Amount > 0 || WaterTapFee.Amount > 0 ||
            IrrigationCapacityFee.Amount > 0 || IrrigationTapFee.Amount > 0;
        IsPaidInFull = WaterCapacityFee.IsPaidInFull() && WaterTapFee.IsPaidInFull() &&
            IrrigationCapacityFee.IsPaidInFull() && IrrigationTapFee.IsPaidInFull();
        PaidTask = caseDetail.GetTaskDetailOrDefault(WaterPaidTaskCode).Task;
        ResultCode = HasFee && IsPaidInFull ? GetPaidResultCode() :
            HasFee ? "" :
            TaskResultCodes.NotApplicable;
    }

    private string GetPaidResultCode()
    {
        var hasIrrigation = IrrigationCapacityFee.Amount > 0 || IrrigationTapFee.Amount > 0;
        var hasIrrigationAndWater =
            hasIrrigation && (WaterCapacityFee.Amount > 0 || WaterTapFee.Amount > 0);
        var paidResultCode =
            hasIrrigationAndWater ? "WT WTR IRR" :
            hasIrrigation ? "WT IRRIG" :
            "WT WATER";
        return paidResultCode;
    }

    public CaseFeeModel WaterCapacityFee { get; }
    public CaseFeeModel WaterTapFee { get; }
    public CaseFeeModel IrrigationCapacityFee { get; }
    public CaseFeeModel IrrigationTapFee { get; }
    public bool IsCurrentPayment { get; }
    public bool HasFee { get; }
    public bool IsPaidInFull { get; }
    public CaseTaskModel PaidTask { get; }
    public string ResultCode { get; }
}
