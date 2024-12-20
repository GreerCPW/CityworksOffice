using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using XTI_CityworksAppClient;

namespace CityworksOfficeServiceApp.Implementations;

public sealed class DefaultCityworksService : ICityworksService
{
    private readonly CityworksAppClient cwClient;

    public DefaultCityworksService(CityworksAppClient cwClient)
    {
        this.cwClient = cwClient;
    }

    public Task<CasePaymentModel> AddCasePayment(AddCasePaymentRequest addRequest, CancellationToken ct) =>
        cwClient.PLL.AddCasePaymentIfNotFound(addRequest, ct);

    public Task<CaseReceiptDetailModel> AddCaseReceipt(AddCaseReceiptRequest addRequest, CancellationToken ct) =>
        cwClient.PLL.AddCaseReceipt(addRequest, ct);

    public Task<CaseModel> GetCase(long caseID, CancellationToken ct) =>
        cwClient.PLL.GetCase(new(caseID), ct);

    public Task<CaseDetailModel> GetCaseDetail(long caseID, CancellationToken ct) =>
        cwClient.PLL.GetCaseDetail(new(caseID), ct);

    public Task<CwTenderTypeModel[]> GetTenderTypes(CancellationToken ct) =>
        cwClient.PLL.GetTenderTypes(ct);

    public Task<CaseTaskModel> ResolveCaseTask(ResolveCaseTaskRequest resolveRequest, CancellationToken ct) =>
        cwClient.PLL.ResolveCaseTask(resolveRequest, ct);

    public Task UploadCaseReceiptFile(UploadCaseReceiptRequest uploadRequest, CancellationToken ct) =>
        cwClient.PLL.UploadCaseReceiptFile(uploadRequest, ct);
}
