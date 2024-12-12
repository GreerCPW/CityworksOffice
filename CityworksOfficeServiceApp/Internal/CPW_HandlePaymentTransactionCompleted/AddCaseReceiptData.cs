using CPW_Cityworks.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

public sealed class AddCaseReceiptData
{
    public AddCaseReceiptData()
        : this(0, new())
    {
    }

    public AddCaseReceiptData(int transactionID, AddCaseReceiptRequest caseReceipt)
    {
        TransactionID = transactionID;
        CaseReceipt = caseReceipt;
    }

    public int TransactionID { get; set; }
    public AddCaseReceiptRequest CaseReceipt { get; set; }
}
