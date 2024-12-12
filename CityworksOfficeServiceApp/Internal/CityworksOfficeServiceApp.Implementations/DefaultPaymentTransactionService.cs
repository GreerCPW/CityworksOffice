using CityworksOfficeServiceApp.Services;
using XTI_PaymentTransactionAppClient;
using XTI_WebAppClient;

namespace CityworksOfficeServiceApp.Implementations;

public sealed class DefaultPaymentTransactionService : IPaymentTransactionService
{
    private readonly PaymentTransactionAppClient payTranClient;

    public DefaultPaymentTransactionService(PaymentTransactionAppClient payTranClient)
    {
        this.payTranClient = payTranClient;
    }

    public Task<AppClientFileResult> DownloadReceipt(int transactionID, CancellationToken ct) =>
        payTranClient.Transaction.DownloadReceipt(new(transactionID), ct);
}
