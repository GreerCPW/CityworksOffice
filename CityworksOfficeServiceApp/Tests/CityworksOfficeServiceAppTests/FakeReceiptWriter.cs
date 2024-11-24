using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using System.Text;
using XTI_Core;

namespace CityworksOfficeServiceAppTests;

internal sealed class FakeReceiptWriter : IReceiptWriter
{
    public static string Output(CaseModel pllCase, CaseReceiptDetailModel receiptDetail)
    {
        var sb = new StringBuilder();
        sb.AppendLine(XtiSerializer.Serialize(pllCase));
        sb.AppendLine(XtiSerializer.Serialize(receiptDetail));
        return sb.ToString();
    }

    private readonly CaseModel pllCase;
    private readonly CaseReceiptDetailModel receiptDetail;

    public FakeReceiptWriter(CaseModel pllCase, CaseReceiptDetailModel receiptDetail)
    {
        this.pllCase = pllCase;
        this.receiptDetail = receiptDetail;
    }

    public byte[] Write() => UTF8Encoding.UTF8.GetBytes(Output(pllCase, receiptDetail));
}
