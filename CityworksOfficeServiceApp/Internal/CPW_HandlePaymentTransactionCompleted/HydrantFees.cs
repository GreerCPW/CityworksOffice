using CPW_Cityworks.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class HydrantFees : IFees
{
    private const string FireHydrantFeeCode = "FIRE HYD";
    private const string FireHydrantPaidTaskCode = "HYD_PAID";

    public HydrantFees(CaseDetailModel caseDetail, CaseFeeModel[] paidFees)
    {
        HydrantFee = caseDetail.GetFeeDetailOrDefault(FireHydrantFeeCode).Fee;
        IsCurrentPayment = paidFees.Contains(HydrantFee);
        HasFee = HydrantFee.Amount > 0;
        IsPaidInFull = HydrantFee.IsPaidInFull();
        PaidTask = caseDetail.GetTaskDetailOrDefault(FireHydrantPaidTaskCode).Task;
        ResultCode = HasFee && IsPaidInFull ? TaskResultCodes.PaidActive :
            HasFee ? "" :
            TaskResultCodes.NotApplicable;
    }

    public CaseFeeModel HydrantFee { get; }
    public bool IsCurrentPayment { get; }
    public bool HasFee { get; }
    public bool IsPaidInFull { get; }
    public CaseTaskModel PaidTask { get; }
    public string ResultCode { get; }
}
