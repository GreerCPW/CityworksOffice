using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using XTI_App.Abstractions;
using XTI_Jobs;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class UploadReceiptFileAction : JobAction<UploadReceiptFileData>
{
    private readonly ICityworksService cwService;
    private readonly IReceiptWriterFactory receiptWriterFactory;

    public UploadReceiptFileAction(ICityworksService cwService, IReceiptWriterFactory receiptWriterFactory, TriggeredJobTask jobTask) : base(jobTask)
    {
        this.cwService = cwService;
        this.receiptWriterFactory = receiptWriterFactory;
    }

    protected override async Task Execute(CancellationToken stoppingToken, TriggeredJobTask task, JobActionResultBuilder next, UploadReceiptFileData data)
    {
        var pllCase = await cwService.GetCase(data.CaseID, stoppingToken);
        var receiptWriter = receiptWriterFactory.Create(pllCase, data.ReceiptDetail);
        var bytes = receiptWriter.Write();
        var stream = new MemoryStream(bytes);
        stream.Seek(0, SeekOrigin.Begin);
        await cwService.UploadCaseReceiptFile
        (
            new UploadCaseReceiptRequest
            (
                receiptID: data.ReceiptDetail.Receipt.ID,
                receiptFile: new FileUpload(stream, "application/pdf", $"{data.ReceiptDetail.Receipt.FileName}.pdf")
            ), 
            stoppingToken
        );
    }
}
