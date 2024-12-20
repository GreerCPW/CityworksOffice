using CPW_Cityworks.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

internal interface IFees
{
    bool IsCurrentPayment { get; }
    bool HasFee { get; }
    bool IsPaidInFull { get; }
    CaseTaskModel PaidTask { get; }
    string ResultCode { get; }
}
