using CPW_ExpandedCityworksDB;
using CPW_PaymentTransaction.Abstractions;
using Microsoft.EntityFrameworkCore;
using XTI_PaymentTransactionAppClient;

namespace XTI_CityworksOfficeWebAppApiActions.Receivables;

public sealed class AddOrUpdateReceivablesAction : AppAction<EmptyRequest, EmptyActionResult>
{
    private const string SourceApp = "PLL";
    private const string VoidStatus = "VOID";

    private readonly ExpandedCityworksDbContext db;
    private readonly PaymentTransactionAppClient payTranClient;

    public AddOrUpdateReceivablesAction(ExpandedCityworksDbContext db, PaymentTransactionAppClient payTranClient)
    {
        this.db = db;
        this.payTranClient = payTranClient;
    }

    public async Task<EmptyActionResult> Execute(EmptyRequest model, CancellationToken ct)
    {
        var feesToAdd = await GetFeesToAdd();
        var addRequests = GetAddRequests(feesToAdd);
        await payTranClient.Receivables.AddOrUpdateReceivableBatch(new(addRequests), ct);
        var feesToUpdate = await GetFeesToUpdate();
        var updateRequests = GetAddRequests(feesToUpdate);
        await payTranClient.Receivables.AddOrUpdateReceivableBatch(new(updateRequests), ct);
        var feesToVoid = await GetFeesToVoid();
        var voidRequests = GetVoidRequests(feesToVoid);
        await payTranClient.Receivables.AddOrUpdateReceivableBatch(new(voidRequests), ct);
        var feesToDelete = await GetFeesToDelete();
        var deleteRequests = GetDeleteRequests(feesToDelete);
        await payTranClient.Receivables.AddOrUpdateReceivableBatch(new(deleteRequests), ct);
        return new EmptyActionResult();
    }

    private Task<ExpandedFeeEntity[]> GetFeesToAdd()
    {
        var receivableFeeIDs = db.ExpandedReceivableLineItems.Retrieve()
            .Select(li => li.FeeID);
        return db.ExpandedFees.Retrieve()
            .Where
            (
                f =>
                    !receivableFeeIDs.Contains(f.FeeID) &&
                    f.GlAccountNumber != "" &&
                    f.FeeAmount > 0 &&
                    f.FeeAmount > f.PaymentAmount &&
                    f.IsFeeRegistered &&
                    f.CaseStatus != VoidStatus
            )
            .ToArrayAsync();
    }

    private Task<ExpandedFeeEntity[]> GetFeesToUpdate()
    {
        return db.ExpandedFees.Retrieve()
            .Where(li => li.CaseStatus != VoidStatus)
            .Join
            (
                db.ExpandedReceivableLineItems.Retrieve(),
                f => f.FeeID,
                li => li.FeeID,
                (f, li) => new { Fee = f, LineItem = li }
            )
            .Where
            (
                joined =>
                    joined.Fee.FeeAmount != joined.LineItem.AmountDue ||
                    (
                        joined.Fee.GlAccountNumber != joined.LineItem.GlAccountNumber && 
                        joined.Fee.GlAccountNumber != joined.LineItem.GlAccountNumber.Replace("-000000", "")
                    )
            )
            .Select(joined => joined.Fee)
            .ToArrayAsync();
    }

    private Task<ExpandedFeeEntity[]> GetFeesToVoid()
    {
        return db.ExpandedFees.Retrieve()
            .Where(f => f.CaseStatus == VoidStatus && f.FeeAmount > f.PaymentAmount)
            .Join
            (
                db.ExpandedReceivableLineItems.Retrieve(),
                f => f.FeeID,
                li => li.FeeID,
                (f, li) => new { Fee = f, LineItem = li }
            )
            .Where
            (
                joined => joined.LineItem.AmountDue > 0
            )
            .Select(joined => joined.Fee)
            .ToArrayAsync();
    }

    private Task<ExpandedReceivableLineItemEntity[]> GetFeesToDelete()
    {
        var feeIDs = db.ExpandedFees.Retrieve()
            .Select(f => f.FeeID);
        return db.ExpandedReceivableLineItems.Retrieve()
            .Where(li => !feeIDs.Contains(li.FeeID) && li.AmountDue > 0)
            .ToArrayAsync();
    }

    private AddOrUpdateReceivableRequest[] GetAddRequests(ExpandedFeeEntity[] fees) =>
        fees
            .ToLookup(f => new { f.CaseID, f.CaseNumber }, f => f)
            .Select
            (
                group =>
                {
                    var lineItems = group
                        .Select
                        (
                            g => new AddOrUpdateReceivableLineItemRequest
                            (
                                sourceKey: g.FeeID.ToString(),
                                sourceCode: g.FeeCode,
                                sourceDescription: g.FeeDescription,
                                amountDue: g.FeeAmount,
                                glAccountNumber: new PymtGlAccountNumber(g.GlAccountNumber)
                            )
                        )
                        .ToArray();
                    return new AddOrUpdateReceivableRequest
                    (
                        sourceApp: SourceApp,
                        sourceKey: group.Key.CaseID.ToString(),
                        sourceDescription: group.Key.CaseNumber,
                        lineItems: lineItems
                    );
                }
            )
            .ToArray();

    private AddOrUpdateReceivableRequest[] GetVoidRequests(ExpandedFeeEntity[] fees)
    {
        var addRequests = GetAddRequests(fees);
        foreach (var addRequest in addRequests)
        {
            foreach (var lineItem in addRequest.LineItems)
            {
                lineItem.AmountDue = 0;
            }
        }
        return addRequests;
    }

    private AddOrUpdateReceivableRequest[] GetDeleteRequests(ExpandedReceivableLineItemEntity[] lineItems) =>
        lineItems
            .ToLookup(li => new { li.CaseID, li.CaseNumber }, f => f)
            .Select
            (
                group =>
                {
                    var lineItems = group
                        .Select
                        (
                            g => new AddOrUpdateReceivableLineItemRequest
                            (
                                sourceKey: g.FeeID.ToString(),
                                sourceCode: g.FeeCode,
                                sourceDescription: g.FeeDescription,
                                amountDue: 0,
                                glAccountNumber: new PymtGlAccountNumber(g.GlAccountNumber)
                            )
                        )
                        .ToArray();
                    return new AddOrUpdateReceivableRequest
                    (
                        sourceApp: SourceApp,
                        sourceKey: group.Key.CaseID.ToString(),
                        sourceDescription: group.Key.CaseNumber,
                        lineItems: lineItems
                    );
                }
            )
            .ToArray();

}