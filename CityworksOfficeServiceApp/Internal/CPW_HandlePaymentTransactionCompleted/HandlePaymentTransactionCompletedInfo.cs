using XTI_Jobs.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

public static class HandlePaymentTransactionCompletedInfo
{
    public static JobKey JobKey = new("Handle PLL Payment Transaction Completed");
    public static readonly JobTaskKey LoadCaseDetail = new(nameof(LoadCaseDetail));
    public static readonly JobTaskKey AddCasePayment = new(nameof(AddCasePayment));

    public static readonly JobTaskKey[] AllTasks =
        [
            LoadCaseDetail,
            AddCasePayment
        ];
}
