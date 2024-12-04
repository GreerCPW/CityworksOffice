using CPW_Cityworks.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class ElectricFees : IFees
{
    private const string ElectricFeeCode = "EL DEV";
    private const string ElectricPaidTaskCode = "ELEC_PAID";

    public ElectricFees(CaseDetailModel caseDetail, CaseFeeModel[] paidFees)
    {
        ElectricFee = caseDetail.GetFeeDetailOrDefault(ElectricFeeCode).Fee;
        IsCurrentPayment = paidFees.Contains(ElectricFee);
        HasFee = ElectricFee.Amount > 0;
        IsPaidInFull = ElectricFee.IsPaidInFull();
        PaidTask = caseDetail.GetTaskDetailOrDefault(ElectricPaidTaskCode).Task;
        ResultCode =
            HasFee && IsPaidInFull ? GetPaidResultCode(caseDetail) :
            HasFee ? "" :
            TaskResultCodes.NotApplicable;
    }

    private static string GetPaidResultCode(CaseDetailModel caseDetail)
    {
        var cityLimits = caseDetail.GetDataDetailOrDefault("CITY LIMIT", "IN OR OUT").Value;
        return cityLimits.Equals("Outside", StringComparison.OrdinalIgnoreCase) ? "ELEC CNTY" : "ELEC CITY";
    }

    public CaseFeeModel ElectricFee { get; }
    public bool IsCurrentPayment { get; }
    public bool HasFee { get; }
    public bool IsPaidInFull { get; }
    public CaseTaskModel PaidTask { get; }
    public string ResultCode { get; }
}
