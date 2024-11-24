using XTI_Jobs.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

public static class HandlePaymentTransactionCompletedInfo
{
    public static JobKey JobKey = new("Handle PLL Payment Transaction Completed");
    public static readonly JobTaskKey LoadCaseDetail = new(nameof(LoadCaseDetail));
    public static readonly JobTaskKey AddCasePayment = new(nameof(AddCasePayment));
    public static readonly JobTaskKey LoadTaskResolutions = new(nameof(LoadTaskResolutions));
    public static readonly JobTaskKey ResolveCaseTask = new(nameof(ResolveCaseTask));
    public static readonly JobTaskKey AddCaseReceipt = new(nameof(AddCaseReceipt));
    public static readonly JobTaskKey UploadReceiptFile = new(nameof(UploadReceiptFile));

    public static readonly JobTaskKey[] AllTasks =
        [
            LoadCaseDetail,
            AddCasePayment,
            LoadTaskResolutions,
            ResolveCaseTask,
            AddCaseReceipt,
            UploadReceiptFile
        ];
}
