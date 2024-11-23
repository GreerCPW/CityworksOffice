namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class HandlePaymentTransactionData
{
    public HandlePaymentTransactionData()
        : this(0, [])
    {
    }

    public HandlePaymentTransactionData(long caseID, HandleAppliedPaymentData[] appliedPayments)
    {
        CaseID = caseID;
        AppliedPayments = appliedPayments;
    }

    public long CaseID { get; set; }
    public int AppliedPaymentIndex { get; set; }
    public HandleAppliedPaymentData[] AppliedPayments { get; set; }

    public HandleAppliedPaymentData GetCurrentAppliedPayment() => AppliedPayments[AppliedPaymentIndex];

    public bool HasCurrentAppliedPayment() => AppliedPayments.Length > AppliedPaymentIndex;

    public void NextAppliedPayment() => AppliedPaymentIndex++;
}
