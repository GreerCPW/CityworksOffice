using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using CPW_PaymentTransaction.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class CachedTenderTypes
{
    private readonly IMemoryCache cache;
    private readonly ICityworksService cwService;

    private static readonly string cacheKey = "cw_tender_types";

    public CachedTenderTypes(IMemoryCache cache, ICityworksService cwService)
    {
        this.cache = cache;
        this.cwService = cwService;
    }

    public async Task<CwTenderTypeModel> FromPaymentMethod(PaymentMethod paymentMethod, CancellationToken ct)
    {
        var tenderTypes = await TenderTypes(ct);
        var cwTenderType =
            paymentMethod.Equals(PaymentMethod.Values.Cash) ? CwTenderCode.Values.Cash :
            paymentMethod.Equals(PaymentMethod.Values.Check) ? CwTenderCode.Values.Check :
            paymentMethod.Equals(PaymentMethod.Values.Internet) ? CwTenderCode.Values.CreditCard :
            paymentMethod.Equals(PaymentMethod.Values.CardReader) ? CwTenderCode.Values.CreditCard :
            CwTenderCode.Values.NotSet;
        return tenderTypes.FirstOrDefault(tt => tt.Code.Equals(cwTenderType)) ?? new();
    }

    private async Task<CwTenderTypeModel[]> TenderTypes(CancellationToken ct)
    {
        if (!cache.TryGetValue<CwTenderTypeModel[]>(cacheKey, out var tenderTypes))
        {
            tenderTypes = await cwService.GetTenderTypes(ct);
            cache.Set(cacheKey, tenderTypes);
        }
        return tenderTypes ?? [];
    }
}
