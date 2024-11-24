using CPW_Cityworks.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

public sealed class UploadReceiptFileData
{
    public UploadReceiptFileData()
        : this(0, new())
    {
    }

    public UploadReceiptFileData(long caseID, CaseReceiptDetailModel receiptDetail)
    {
        CaseID = caseID;
        ReceiptDetail = receiptDetail;
    }

    public long CaseID { get; set; }
    public CaseReceiptDetailModel ReceiptDetail { get; set; }
}
