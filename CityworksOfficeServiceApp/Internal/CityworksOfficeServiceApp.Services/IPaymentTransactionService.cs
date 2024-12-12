using XTI_WebAppClient;

namespace CityworksOfficeServiceApp.Services;

public interface IPaymentTransactionService
{
    Task<AppClientFileResult> DownloadReceipt(int transactionID, CancellationToken ct);
}
