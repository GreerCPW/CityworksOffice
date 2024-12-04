using CPW_Cityworks.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class GasFees : IFees
{
    private const string GasFeeCode = "GAS FEES";
    private const string GasPaidTaskCode = "GAS_PAID";

    public GasFees(CaseDetailModel caseDetail, CaseFeeModel[] paidFees)
    {
        GasFee = caseDetail.GetFeeDetailOrDefault(GasFeeCode).Fee;
        IsCurrentPayment = paidFees.Contains(GasFee);
        HasFee = GasFee.Amount > 0;
        IsPaidInFull = GasFee.IsPaidInFull();
        PaidTask = caseDetail.GetTaskDetailOrDefault(GasPaidTaskCode).Task;
        ResultCode = HasFee && IsPaidInFull ? TaskResultCodes.PaidActive :
            HasFee ? "" :
            TaskResultCodes.NotApplicable;
    }

    public CaseFeeModel GasFee { get; }
    public bool IsCurrentPayment { get; }
    public bool HasFee { get; }
    public bool IsPaidInFull { get; }
    public CaseTaskModel PaidTask { get; }
    public string ResultCode { get; }
}
