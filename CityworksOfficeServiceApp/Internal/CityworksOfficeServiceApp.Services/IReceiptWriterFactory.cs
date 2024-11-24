using CPW_Cityworks.Abstractions;

namespace CityworksOfficeServiceApp.Services;

public interface IReceiptWriterFactory
{
    IReceiptWriter Create(CaseModel pllCase, CaseReceiptDetailModel receiptDetail);
}
