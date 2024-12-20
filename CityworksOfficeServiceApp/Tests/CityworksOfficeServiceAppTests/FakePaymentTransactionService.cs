using CityworksOfficeServiceApp.Services;
using System.Text;
using XTI_WebAppClient;

namespace CityworksOfficeServiceAppTests;

internal sealed class FakePaymentTransactionService : IPaymentTransactionService
{
    public static string Output(int transactionID)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Payment Transaction: {transactionID}");
        return sb.ToString();
    }

    public Task<AppClientFileResult> DownloadReceipt(int transactionID, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(Output(transactionID));
        var fileResult = new AppClientFileResult(bytes, "application/pdf", "");
        return Task.FromResult(fileResult);
    }
}
