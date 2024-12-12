using CPW_Cityworks.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

public sealed class UploadReceiptFileData
{
    public UploadReceiptFileData()
        : this(0, 0, "")
    {
    }

    public UploadReceiptFileData(int transactionID, long receiptID, string receiptFileName)
    {
        TransactionID = transactionID;
        ReceiptID = receiptID;
        ReceiptFileName = receiptFileName;
    }

    public int TransactionID { get; set; }
    public long ReceiptID { get; set; }
    public string ReceiptFileName { get; set; }
}
