using CPW_Cityworks.Abstractions;

namespace CityworksOfficeServiceApp.Services;

public interface ICityworksService
{
    Task<CwTenderTypeModel[]> GetTenderTypes(CancellationToken ct);
    Task<CaseDetailModel> GetCaseDetail(long caseID, CancellationToken ct);
    Task<CasePaymentModel> AddCasePayment(AddCasePaymentRequest addRequest, CancellationToken ct);
    Task<CaseTaskModel> ResolveCaseTask(ResolveCaseTaskRequest resolveRequest, CancellationToken ct);
}
