using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using XTI_Jobs;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class UploadReceiptFileAction : JobAction<UploadReceiptFileData>
{
    private readonly ICityworksService cwService;
    private readonly IPaymentTransactionService payTranService;

    public UploadReceiptFileAction(ICityworksService cwService, IPaymentTransactionService payTranService, TriggeredJobTask jobTask) : base(jobTask)
    {
        this.cwService = cwService;
        this.payTranService = payTranService;
    }

    protected override async Task Execute(CancellationToken stoppingToken, TriggeredJobTask task, JobActionResultBuilder next, UploadReceiptFileData data)
    {
        var fileResult = await payTranService.DownloadReceipt(data.TransactionID, stoppingToken);
        var stream = new MemoryStream(fileResult.Content);
        stream.Seek(0, SeekOrigin.Begin);
        await cwService.UploadCaseReceiptFile
        (
            new UploadCaseReceiptRequest
            (
                receiptID: data.ReceiptID,
                receiptFile: new(stream, "application/pdf", $"{data.ReceiptFileName}.pdf")
            ),
            stoppingToken
        );
    }
}
