using CPW_Cityworks.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class SewerFees : IFees
{
    private const string SewerCapacityFeeCode = "SEWER CAP";
    private const string SewerTapFeeCode = "SEWER TAP";
    private const string SewerPaidTaskCode = "SEWER_PAID";

    public SewerFees(CaseDetailModel caseDetail, CaseFeeModel[] paidFees)
    {
        CapacityFee = caseDetail.GetFeeDetailOrDefault(SewerCapacityFeeCode).Fee;
        TapFee = caseDetail.GetFeeDetailOrDefault(SewerTapFeeCode).Fee;
        IsCurrentPayment = paidFees.Contains(CapacityFee) || paidFees.Contains(TapFee);
        HasFee = CapacityFee.Amount > 0 || TapFee.Amount > 0;
        IsPaidInFull = CapacityFee.IsPaidInFull() && TapFee.IsPaidInFull();
        PaidTask = caseDetail.GetTaskDetailOrDefault(SewerPaidTaskCode).Task;
        ResultCode = HasFee && IsPaidInFull ? TaskResultCodes.PaidActive :
            HasFee ? "" :
            TaskResultCodes.NotApplicable;
    }

    public CaseFeeModel CapacityFee { get; }
    public CaseFeeModel TapFee { get; }
    public bool IsCurrentPayment { get; }
    public bool HasFee { get; }
    public bool IsPaidInFull { get; }
    public CaseTaskModel PaidTask { get; }
    public string ResultCode { get; }
}
