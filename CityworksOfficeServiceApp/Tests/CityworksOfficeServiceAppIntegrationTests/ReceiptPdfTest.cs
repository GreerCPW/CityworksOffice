using CityworksOfficeServiceApp.Implementations;
using CPW_Cityworks.Abstractions;
using DinkToPdf.Contracts;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CityworksOfficeServiceAppIntegrationTests;

internal sealed class ReceiptPdfTest
{
    [Test]
    public async Task ShouldWriteReceiptPdf()
    {
        var sp = await Setup();
        var converter = sp.GetRequiredService<IConverter>();
        var receiptPdf = new ReceiptPdf
        (
            converter,
            new CaseDetailModel
            {
                Case = new CaseModel
                {
                    CaseNumber = "SVC-0001",
                    CaseType = new CaseTypeModel { Description = "Install Service" }
                }
            },
            new CaseReceiptDetailModel
            (
                Receipt: new(1, DateTime.Now, 23.45M, 23.45M, ""),
                TenderTypes: [
                    new CaseReceiptTenderTypeModel("CASH", "Cash", 13.45M),
                    new CaseReceiptTenderTypeModel("CHECK", "Check", 10M)
                ],
                Fees: [
                    new CaseReceiptFeeModel("FEE1", "Fee 1", 50, 50, 10),
                    new CaseReceiptFeeModel("FEE2", "Fee 2", 30, 30, 13.45M)
                ]
            )
        );
        var receiptPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "test.pdf");
        if (File.Exists(receiptPath))
        {
            File.Delete(receiptPath);
        }
        var receiptContent = receiptPdf.Write();
        File.WriteAllBytes(receiptPath, receiptContent);
        Console.WriteLine(receiptPath);
    }

    private Task<IServiceProvider> Setup(string envName = "Development")
    {
        var host = new CityworksOfficeTestHost();
        return host.Setup(envName);
    }
}