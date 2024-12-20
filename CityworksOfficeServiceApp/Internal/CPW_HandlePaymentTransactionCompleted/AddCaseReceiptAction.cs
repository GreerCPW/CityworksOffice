using CityworksOfficeServiceApp.Services;
using XTI_Jobs;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class AddCaseReceiptAction : JobAction<AddCaseReceiptData>
{
    private readonly ICityworksService cwService;

    public AddCaseReceiptAction(ICityworksService cwService, TriggeredJobTask task) : base(task)
    {
        this.cwService = cwService;
    }

    protected override async Task Execute(CancellationToken stoppingToken, TriggeredJobTask task, JobActionResultBuilder next, AddCaseReceiptData data)
    {
        var receiptDetail = await cwService.AddCaseReceipt(data.CaseReceipt, stoppingToken);
        next.AddNext
        (
            HandlePaymentTransactionCompletedInfo.UploadReceiptFile, 
            new UploadReceiptFileData(transactionID: data.TransactionID, receiptID: receiptDetail.Receipt.ID, receiptFileName: receiptDetail.Receipt.FileName)
        );
    }
}
