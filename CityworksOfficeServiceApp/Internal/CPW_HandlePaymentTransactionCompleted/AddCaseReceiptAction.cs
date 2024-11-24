using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using XTI_Jobs;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class AddCaseReceiptAction : JobAction<AddCaseReceiptRequest>
{
    private readonly ICityworksService cwService;

    public AddCaseReceiptAction(ICityworksService cwService, TriggeredJobTask task) : base(task)
    {
        this.cwService = cwService;
    }

    protected override async Task Execute(CancellationToken stoppingToken, TriggeredJobTask task, JobActionResultBuilder next, AddCaseReceiptRequest data)
    {
        var receiptDetail = await cwService.AddCaseReceipt(data, stoppingToken);
        next.AddNext
        (
            HandlePaymentTransactionCompletedInfo.UploadReceiptFile, 
            new UploadReceiptFileData(caseID: data.CaseID, receiptDetail: receiptDetail)
        );
    }
}
