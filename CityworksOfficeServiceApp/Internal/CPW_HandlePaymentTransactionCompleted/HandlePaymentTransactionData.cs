namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class HandlePaymentTransactionData
{
    public HandlePaymentTransactionData()
        : this(0, 0, [])
    {
    }

    public HandlePaymentTransactionData(int paymentTransactionID, long caseID, HandleAppliedPaymentData[] appliedPayments)
    {
        PaymentTransactionID = paymentTransactionID;
        CaseID = caseID;
        AppliedPayments = appliedPayments;
    }

    public int PaymentTransactionID { get; set; }
    public long CaseID { get; set; }
    public int AppliedPaymentIndex { get; set; }
    public HandleAppliedPaymentData[] AppliedPayments { get; set; }

    public HandleAppliedPaymentData GetCurrentAppliedPayment() => AppliedPayments[AppliedPaymentIndex];

    public bool HasCurrentAppliedPayment() => AppliedPayments.Length > AppliedPaymentIndex;

    public void NextAppliedPayment() => AppliedPaymentIndex++;
}
