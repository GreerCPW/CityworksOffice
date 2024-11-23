using CPW_Cityworks.Abstractions;
using CPW_Contact;
using CPW_PaymentTransaction.Abstractions;
using CPW_PaymentTransaction.Events;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using XTI_Jobs;
using XTI_Jobs.Abstractions;

namespace CityworksOfficeServiceAppTests;

internal sealed class HandlePaymentTransactionCompletedTest
{
    [Test]
    public async Task ShouldCompleteJobSuccessfully()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(sp);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 10);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(fee1.Amount, PaymentMethod.Values.Cash)
            .Build();
        var eventNotification = await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var triggeredJobs = await eventNotification.TriggeredJobs();
        var triggeredJob = triggeredJobs.FirstOrDefault();
        Assert.That(triggeredJob?.Status(), Is.EqualTo(JobTaskStatus.Values.Completed), "Should complete job successfully");
    }

    [Test]
    public async Task ShouldOnlyHandlePllPayments()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var eventData = new PaymentTransactionEventData
        {
            ID = 3,
            SourceAppCode = "OTHER",
            SourceKey = "5",
            LineItems = [
                new LineItemEventData
                {
                    ID = 4,
                    SourceKey = "17",
                    AmountCharged = 23,
                    AppliedPayments = [
                        new AppliedPaymentEventData
                        {
                            ID = 6,
                            AmountPaid = 23,
                            PaymentMethod = PaymentMethod.Values.Cash
                        }
                    ]
                }
            ]
        };
        var eventNotification = await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var triggeredJobs = await eventNotification.TriggeredJobs();
        var triggeredJob = triggeredJobs.FirstOrDefault();
        Assert.That(triggeredJob?.Status(), Is.EqualTo(JobTaskStatus.Values.Completed), "Should only handle PLL payments");
    }

    [Test]
    public async Task ShouldAddPaymentForFee()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(sp);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 10);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(fee1.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedFeeDetail = updatedCaseDetail.GetFeeDetailOrDefault(fee1.ID);
        Assert.That(updatedFeeDetail.Payments.Length, Is.EqualTo(1), "Should add payment for fee");
    }

    [Test]
    public async Task ShouldAddMultiplePaymentsForFee()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(sp);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 10);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(7, PaymentMethod.Values.Cash)
            .AddAppliedPayment(3, PaymentMethod.Values.Check)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedFeeDetail = updatedCaseDetail.GetFeeDetailOrDefault(fee1.ID);
        Assert.That(updatedFeeDetail.Payments.Length, Is.EqualTo(2), "Should add multiple payments for fee");
        Assert.That
        (
            updatedFeeDetail.Payments.Select(p => p.PaymentAmount),
            Is.EquivalentTo(new[] { 7, 3 }),
            "Should add multiple payments for fee"
        );
    }

    [Test]
    public async Task ShouldSetPaymentTenderType_WhenAddingPayment()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(sp);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 10);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(fee1.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedFeeDetail = updatedCaseDetail.GetFeeDetailOrDefault(fee1.ID);
        var payment = updatedFeeDetail.Payments.FirstOrDefault() ?? new();
        Assert.That(payment.TenderType, Is.EqualTo("Cash"), "Should set payment tender type when adding payment");
    }

    [Test]
    public async Task ShouldSetPaymentReferenceInfo_WhenAddingPayment()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(sp);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 10);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(fee1.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedFeeDetail = updatedCaseDetail.GetFeeDetailOrDefault(fee1.ID);
        var payment = updatedFeeDetail.Payments.FirstOrDefault() ?? new();
        Assert.That(payment.ReferenceInfo, Is.EqualTo($"PYMT:{eventData.ID:0000000}|APPLIED:{1:0000000}"), "Should set payment reference info when adding payment");
    }

    [Test]
    public async Task ShouldAddPaymentsForMultipleFees()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(sp);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 10);
        var fee2 = cwService.AddCaseFee(caseDetail, "FIRE HYD", 11);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(fee1.Amount, PaymentMethod.Values.Cash)
            .AddLine(fee2)
            .AddAppliedPayment(fee2.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedFee1Detail = updatedCaseDetail.GetFeeDetailOrDefault(fee1.ID);
        Assert.That(updatedFee1Detail.Payments.Length, Is.EqualTo(1), "Should add payments for multiple fees");
        var updatedFee2Detail = updatedCaseDetail.GetFeeDetailOrDefault(fee2.ID);
        Assert.That(updatedFee2Detail.Payments.Length, Is.EqualTo(1), "Should add payments for multiple fees");
    }

    [Test]
    public async Task ShouldResolveFireHydrantPaidTask()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(sp);
        var hydrantFee = cwService.AddFireHydrantFee(caseDetail);
        var hydrantTask = cwService.AddFireHydrantFee(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(hydrantFee)
            .AddAppliedPayment(hydrantFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedHydrantPaidTask = updatedCaseDetail.GetTaskDetails();
        var updatedFee1Detail = updatedCaseDetail.GetFeeDetailOrDefault(fee1.ID);
        Assert.That(updatedFee1Detail.Payments.Length, Is.EqualTo(1), "Should add payments for multiple fees");
        var updatedFee2Detail = updatedCaseDetail.GetFeeDetailOrDefault(fee2.ID);
        Assert.That(updatedFee2Detail.Payments.Length, Is.EqualTo(1), "Should add payments for multiple fees");
    }

    private Task<IServiceProvider> Setup(string envName = "Development")
    {
        var host = new CityworksOfficeTestHost();
        return host.Setup(envName);
    }

    private static CaseDetailModel AddCaseDetail(IServiceProvider sp)
    {
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        return cwService.AddInstallServiceCase
        (
            location: "101 Somewhere Ln, Greer, SC 29652",
            accountType: "Residential",
            companyName: "Developer",
            billingAddress: new Address(new StreetAddress("101", "Arrakis Ln"), "Greer", "SC", new ZipCode(29652)),
            coords: new CwCoordinates()
        );
    }

    private static CaseDetailModel GetCaseDetail(IServiceProvider sp, CaseDetailModel caseDetail)
    {
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        return cwService.GetCaseDetail(caseDetail);
    }

    private async Task<EventNotification> NotifyPaymentTransactionCompleted(IServiceProvider sp, PaymentTransactionEventData eventData)
    {
        var incomingEventFactory = sp.GetRequiredService<IncomingEventFactory>();
        var eventNotifications = await incomingEventFactory
            .Incoming(PaymentTransactionEvents.PaymentTransactionCompleted)
            .From(eventData.ToEventSource())
            .Notify();
        return eventNotifications[0];
    }

    private Task HandlePaymentTransactionCompleted(IServiceProvider sp)
    {
        var tester = CityworksOfficeActionTester.Create(sp, api => api.Jobs.HandlePaymentTransactionCompleted);
        return tester.Execute(new());
    }
}