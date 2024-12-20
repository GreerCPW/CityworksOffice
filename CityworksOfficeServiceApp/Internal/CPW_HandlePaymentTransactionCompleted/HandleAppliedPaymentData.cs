namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class HandleAppliedPaymentData
{
    public HandleAppliedPaymentData()
        : this(0, 0, 0, 0)
    {
    }

    public HandleAppliedPaymentData(long caseFeeID, decimal amountPaid, long tenderTypeID, int appliedPaymentID)
    {
        CaseFeeID = caseFeeID;
        AmountPaid = amountPaid;
        TenderTypeID = tenderTypeID;
        AppliedPaymentID = appliedPaymentID;
    }

    public long CaseFeeID { get; set; }
    public decimal AmountPaid { get; set; }
    public long TenderTypeID { get; set; }
    public int AppliedPaymentID { get; set; }
    public long CasePaymentID { get; set; }
}
