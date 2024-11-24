using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using DinkToPdf.Contracts;

namespace CityworksOfficeServiceApp.Implementations;

public sealed class PdfReceiptWriterFactory : IReceiptWriterFactory
{
    private readonly IConverter converter;

    public PdfReceiptWriterFactory(IConverter converter)
    {
        this.converter = converter;
    }

    public IReceiptWriter Create(CaseModel pllCase, CaseReceiptDetailModel receiptDetail) =>
        new PdfReceiptWriter(converter, pllCase, receiptDetail);
}
