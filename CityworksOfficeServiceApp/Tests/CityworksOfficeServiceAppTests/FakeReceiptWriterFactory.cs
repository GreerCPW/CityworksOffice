using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;

namespace CityworksOfficeServiceAppTests;

internal sealed class FakeReceiptWriterFactory : IReceiptWriterFactory
{
    public IReceiptWriter Create(CaseModel pllCase, CaseReceiptDetailModel receiptDetail) =>
        new FakeReceiptWriter(pllCase, receiptDetail);
}
