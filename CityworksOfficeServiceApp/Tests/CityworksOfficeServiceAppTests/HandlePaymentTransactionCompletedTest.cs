﻿using CPW_Cityworks.Abstractions;
using CPW_Contact;
using CPW_PaymentTransaction.Abstractions;
using CPW_PaymentTransaction.Events;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Immutable;
using System.Text;
using System.Text.Unicode;
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
        var caseDetail = AddCaseDetail(cwService);
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
        var caseDetail = AddCaseDetail(cwService);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 10);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(fee1.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedFeeDetail = updatedCaseDetail.GetFeeDetailOrDefault(fee1.ID);
        Assert.That(updatedFeeDetail.PaymentDetails.Length, Is.EqualTo(1), "Should add payment for fee");
    }

    [Test]
    public async Task ShouldAddMultiplePaymentsForFee()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
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
        Assert.That(updatedFeeDetail.PaymentDetails.Length, Is.EqualTo(2), "Should add multiple payments for fee");
        Assert.That
        (
            updatedFeeDetail.PaymentDetails.Select(p => p.Payment.PaymentAmount),
            Is.EquivalentTo(new[] { 7, 3 }),
            "Should add multiple payments for fee"
        );
    }

    [Test]
    public async Task ShouldSetPaymentTenderType_WhenAddingPayment()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 10);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(fee1.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedFeeDetail = updatedCaseDetail.GetFeeDetailOrDefault(fee1.ID);
        var paymentDetail = updatedFeeDetail.PaymentDetails.FirstOrDefault() ?? new();
        Assert.That(paymentDetail.Payment.TenderType, Is.EqualTo("Cash"), "Should set payment tender type when adding payment");
    }

    [Test]
    public async Task ShouldSetPaymentReferenceInfo_WhenAddingPayment()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 10);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(fee1.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedFeeDetail = updatedCaseDetail.GetFeeDetailOrDefault(fee1.ID);
        var paymentDetail = updatedFeeDetail.PaymentDetails.FirstOrDefault() ?? new();
        var appliedPaymentID = eventData.LineItems.First().AppliedPayments.First().ID;
        Assert.That(paymentDetail.Payment.AppliedPaymentID, Is.EqualTo(appliedPaymentID), "Should set payment reference info when adding payment");
    }

    [Test]
    public async Task ShouldAddPaymentsForMultipleFees()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
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
        Assert.That(updatedFee1Detail.PaymentDetails.Length, Is.EqualTo(1), "Should add payments for multiple fees");
        var updatedFee2Detail = updatedCaseDetail.GetFeeDetailOrDefault(fee2.ID);
        Assert.That(updatedFee2Detail.PaymentDetails.Length, Is.EqualTo(1), "Should add payments for multiple fees");
    }

    [Test]
    public async Task ShouldResolveFireHydrantPaidTask()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var hydrantFee = cwService.AddFireHydrantFee(caseDetail);
        var hydrantPaidTask = cwService.AddFireHydrantPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(hydrantFee)
            .AddAppliedPayment(hydrantFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedHydrantPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(hydrantPaidTask.ID).Task;
        Assert.That(updatedHydrantPaidTask.ResultCode, Is.EqualTo("PAIDACTIVE"), "Should resolve fire hydrant paid task");
    }

    [Test]
    public async Task ShouldNotResolveFireHydrantPaidTask_WhenNotFullyPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var hydrantFee = cwService.AddFireHydrantFee(caseDetail);
        var hydrantPaidTask = cwService.AddFireHydrantPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(hydrantFee, hydrantFee.Amount - 1)
            .AddAppliedPayment(hydrantFee.Amount - 1, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedHydrantPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(hydrantPaidTask.ID).Task;
        Assert.That(updatedHydrantPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve fire hydrant paid task when not fully paid");
    }

    [Test]
    public async Task ShouldResolveFireHydrantPaidTask_AfterAdditionalPayment()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var hydrantFee = cwService.AddFireHydrantFee(caseDetail);
        var hydrantPaidTask = cwService.AddFireHydrantPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(hydrantFee, hydrantFee.Amount - 1)
            .AddAppliedPayment(hydrantFee.Amount - 1, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var eventData2 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(hydrantFee, 1)
            .AddAppliedPayment(1, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData2);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedHydrantPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(hydrantPaidTask.ID).Task;
        Assert.That(updatedHydrantPaidTask.ResultCode, Is.EqualTo("PAIDACTIVE"), "Should resolve fire hydrant paid task after additional payment");
    }

    [Test]
    public async Task ShouldResolveGasPaidTask()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var gasServiceFee = cwService.AddGasServiceFee(caseDetail);
        var gasPaidTask = cwService.AddGasPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(gasServiceFee)
            .AddAppliedPayment(gasServiceFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedGasPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(gasPaidTask.ID).Task;
        Assert.That(updatedGasPaidTask.ResultCode, Is.EqualTo("PAIDACTIVE"), "Should resolve gas paid task");
    }

    [Test]
    public async Task ShouldNotResolveGasPaidTask_WhenNotFullyPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var gasServiceFee = cwService.AddGasServiceFee(caseDetail);
        var gasPaidTask = cwService.AddGasPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(gasServiceFee, gasServiceFee.Amount - 1)
            .AddAppliedPayment(gasServiceFee.Amount - 1, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedGasPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(gasPaidTask.ID).Task;
        Assert.That(updatedGasPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve gas paid task when not fully paid");
    }

    [Test]
    public async Task ShouldResolveElectricPaidTaskForCity()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var electricPaidTask = cwService.AddElectricPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee)
            .AddAppliedPayment(electricFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedElectricPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(electricPaidTask.ID).Task;
        Assert.That(updatedElectricPaidTask.ResultCode, Is.EqualTo("ELEC CITY"), "Should resolve electric paid task for city");
    }

    [Test]
    public async Task ShouldResolveElectricPaidTaskForCounty()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        caseDetail = cwService.LocationIsOutsideTheCity(caseDetail);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var electricPaidTask = cwService.AddElectricPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee)
            .AddAppliedPayment(electricFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedElectricPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(electricPaidTask.ID).Task;
        Assert.That(updatedElectricPaidTask.ResultCode, Is.EqualTo("ELEC CNTY"), "Should resolve electric paid task for county");
    }

    [Test]
    public async Task ShouldNotResolveElectricPaidTask_WhenNotFullyPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        caseDetail = cwService.LocationIsInsideTheCity(caseDetail);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var electricPaidTask = cwService.AddElectricPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee, electricFee.Amount - 1)
            .AddAppliedPayment(electricFee.Amount - 1, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedElectricPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(electricPaidTask.ID).Task;
        Assert.That(updatedElectricPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve electric paid task when not fully paid");
    }

    [Test]
    public async Task ShouldResolveElectricPaidTask_AfterAdditionalPayment()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var electricPaidTask = cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee, electricFee.Amount - 1)
            .AddAppliedPayment(electricFee.Amount - 1, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var eventData2 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee, 1)
            .AddAppliedPayment(1, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData2);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedElectricPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(electricPaidTask.ID).Task;
        Assert.That(updatedElectricPaidTask.ResultCode, Is.EqualTo("ELEC CITY"), "Should resolve electric paid task for city when later fully paid");
    }

    [Test]
    public async Task ShouldResolveSewerPaidTask_WhenSewerCapacityFeeIsPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var sewerCapacityFee = cwService.AddSewerCapacityFee(caseDetail);
        var sewerPaidTask = cwService.AddSewerPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(sewerCapacityFee)
            .AddAppliedPayment(sewerCapacityFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedSewerPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(sewerPaidTask.ID).Task;
        Assert.That(updatedSewerPaidTask.ResultCode, Is.EqualTo("PAIDACTIVE"), "Should resolve sewer paid when sewer capacity fee is paid");
    }

    [Test]
    public async Task ShouldResolveSewerPaidTask_WhenSewerTapFeeIsPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var sewerTapFee = cwService.AddSewerTapFee(caseDetail);
        var sewerPaidTask = cwService.AddSewerPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(sewerTapFee)
            .AddAppliedPayment(sewerTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedSewerPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(sewerPaidTask.ID).Task;
        Assert.That(updatedSewerPaidTask.ResultCode, Is.EqualTo("PAIDACTIVE"), "Should resolve sewer paid when sewer tap fee is paid");
    }

    [Test]
    public async Task ShouldNotResolveSewerPaidTask_WhenSewerCapacityFeeIsPaidButNotSewerTapFee()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var sewerCapacityFee = cwService.AddSewerCapacityFee(caseDetail);
        var sewerTapFee = cwService.AddSewerTapFee(caseDetail);
        var sewerPaidTask = cwService.AddSewerPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(sewerCapacityFee)
            .AddAppliedPayment(sewerCapacityFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedSewerPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(sewerPaidTask.ID).Task;
        Assert.That(updatedSewerPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve sewer paid when sewer capacity fee is paid but not sewer tap fee");
    }

    [Test]
    public async Task ShouldNotResolveSewerPaidTask_WhenSewerTapFeeIsPaidButNotSewerCapacityFee()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var sewerCapacityFee = cwService.AddSewerCapacityFee(caseDetail);
        var sewerTapFee = cwService.AddSewerTapFee(caseDetail);
        var sewerPaidTask = cwService.AddSewerPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(sewerTapFee)
            .AddAppliedPayment(sewerTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedSewerPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(sewerPaidTask.ID).Task;
        Assert.That(updatedSewerPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve sewer paid when sewer tap fee is paid but not sewer capacity fee");
    }

    [Test]
    public async Task ShouldResolveSewerPaidTask_WhenSewerTapFeeIsPaidAndThenSewerCapacityFeeIsPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var sewerCapacityFee = cwService.AddSewerCapacityFee(caseDetail);
        var sewerTapFee = cwService.AddSewerTapFee(caseDetail);
        var sewerPaidTask = cwService.AddSewerPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(sewerTapFee)
            .AddAppliedPayment(sewerTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var eventData2 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(sewerCapacityFee)
            .AddAppliedPayment(sewerCapacityFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData2);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedSewerPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(sewerPaidTask.ID).Task;
        Assert.That(updatedSewerPaidTask.ResultCode, Is.EqualTo("PAIDACTIVE"), "Should resolve sewer paid when sewer tap fee is paid and then sewer capacity fee is paid");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTask_WhenWaterCapacityFeeIsPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        cwService.AddIrrigationCapacityFee(caseDetail, amount: 0);
        cwService.AddIrrigationTapFee(caseDetail, amount: 0);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(waterCapacityFee)
            .AddAppliedPayment(waterCapacityFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("WT WATER"), "Should resolve water paid when water capacity fee is paid");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTask_WhenWaterTapFeeIsPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        cwService.AddIrrigationCapacityFee(caseDetail, amount: 0);
        cwService.AddIrrigationTapFee(caseDetail, amount: 0);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(waterTapFee)
            .AddAppliedPayment(waterTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("WT WATER"), "Should resolve water paid when water tap fee is paid");
    }

    [Test]
    public async Task ShouldNotResolveWaterPaidTask_WhenWaterCapacityFeeIsPaidButNotWaterTapFee()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        cwService.AddIrrigationCapacityFee(caseDetail, amount: 0);
        cwService.AddIrrigationTapFee(caseDetail, amount: 0);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(waterCapacityFee)
            .AddAppliedPayment(waterCapacityFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve water paid when water capacity fee is paid but not water tap fee");
    }

    [Test]
    public async Task ShouldNotResolveWaterPaidTask_WhenWaterTapFeeIsPaidButNotWaterCapacityFee()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        cwService.AddIrrigationCapacityFee(caseDetail, amount: 0);
        cwService.AddIrrigationTapFee(caseDetail, amount: 0);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(waterTapFee)
            .AddAppliedPayment(waterTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve water paid when water tap fee is paid but not water capacity fee");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTask_WhenWaterTapFeeIsPaidAndThenWaterCapacityFeeIsPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        cwService.AddIrrigationCapacityFee(caseDetail, amount: 0);
        cwService.AddIrrigationTapFee(caseDetail, amount: 0);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(waterTapFee)
            .AddAppliedPayment(waterTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var eventData2 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(waterCapacityFee)
            .AddAppliedPayment(waterCapacityFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData2);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("WT WATER"), "Should resolve water paid when water tap fee is paid and then water capacity fee is paid");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTask_WhenIrrigationCapacityFeeIsPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.AddWaterCapacityFee(caseDetail, amount: 0);
        cwService.AddWaterTapFee(caseDetail, amount: 0);
        var irrigationCapacityFee = cwService.AddIrrigationCapacityFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(irrigationCapacityFee)
            .AddAppliedPayment(irrigationCapacityFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("WT IRRIG"), "Should resolve water paid when irrigation capacity fee is paid");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTask_WhenIrrigationTapFeeIsPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.AddWaterCapacityFee(caseDetail, amount: 0);
        cwService.AddWaterTapFee(caseDetail, amount: 0);
        var irrigationTapFee = cwService.AddIrrigationTapFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(irrigationTapFee)
            .AddAppliedPayment(irrigationTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("WT IRRIG"), "Should resolve water paid when irrigation tap fee is paid");
    }

    [Test]
    public async Task ShouldNotResolveWaterPaidTask_WhenIrrigationTapFeeIsPaidButNotIrrigationCapacityFee()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.AddWaterCapacityFee(caseDetail, amount: 0);
        cwService.AddWaterTapFee(caseDetail, amount: 0);
        var irrigationCapacityFee = cwService.AddIrrigationCapacityFee(caseDetail);
        var irrigationTapFee = cwService.AddIrrigationTapFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(irrigationTapFee)
            .AddAppliedPayment(irrigationTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve water paid when irrigation tap fee is paid but not irrigation capacity fee");
    }

    [Test]
    public async Task ShouldNotResolveWaterPaidTask_WhenIrrigationCapacityFeeIsPaidButNotIrrigationTapFee()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.AddWaterCapacityFee(caseDetail, amount: 0);
        cwService.AddWaterTapFee(caseDetail, amount: 0);
        var irrigationCapacityFee = cwService.AddIrrigationCapacityFee(caseDetail);
        var irrigationTapFee = cwService.AddIrrigationTapFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(irrigationCapacityFee)
            .AddAppliedPayment(irrigationCapacityFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve water paid when irrigation capacity fee is paid but not irrigation tap fee");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTask_WhenIrrigationCapacityFeeIsPaidAndThenIrrigationTapFee()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.AddWaterCapacityFee(caseDetail, amount: 0);
        cwService.AddWaterTapFee(caseDetail, amount: 0);
        var irrigationCapacityFee = cwService.AddIrrigationCapacityFee(caseDetail);
        var irrigationTapFee = cwService.AddIrrigationTapFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(irrigationCapacityFee)
            .AddAppliedPayment(irrigationCapacityFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var eventData2 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(irrigationTapFee)
            .AddAppliedPayment(irrigationTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData2);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("WT IRRIG"), "Should not resolve water paid when irrigation capacity fee is paid and then irrigation tap fee is paid");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTask_WhenIrrigationAndWaterFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        var irrigationCapacityFee = cwService.AddIrrigationCapacityFee(caseDetail);
        var irrigationTapFee = cwService.AddIrrigationTapFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(irrigationCapacityFee)
            .AddAppliedPayment(irrigationCapacityFee.Amount, PaymentMethod.Values.Cash)
            .AddLine(irrigationTapFee)
            .AddAppliedPayment(irrigationTapFee.Amount, PaymentMethod.Values.Cash)
            .AddLine(waterCapacityFee)
            .AddAppliedPayment(waterCapacityFee.Amount, PaymentMethod.Values.Cash)
            .AddLine(waterTapFee)
            .AddAppliedPayment(waterTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("WT WTR IRR"), "Should resolve water paid when irrigation and water fees are paid");
    }

    [Test]
    public async Task ShouldNotResolveWaterPaidTask_WhenIrrigationFeesArePaidButNotWaterFees()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        var irrigationCapacityFee = cwService.AddIrrigationCapacityFee(caseDetail);
        var irrigationTapFee = cwService.AddIrrigationTapFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(irrigationCapacityFee)
            .AddAppliedPayment(irrigationCapacityFee.Amount, PaymentMethod.Values.Cash)
            .AddLine(irrigationTapFee)
            .AddAppliedPayment(irrigationTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve water paid when irrigation fees are paid but not water fees");
    }

    [Test]
    public async Task ShouldNotResolveWaterPaidTask_WhenWaterFeesArePaidButNotIrrigationFees()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        var irrigationCapacityFee = cwService.AddIrrigationCapacityFee(caseDetail);
        var irrigationTapFee = cwService.AddIrrigationTapFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(waterCapacityFee)
            .AddAppliedPayment(waterCapacityFee.Amount, PaymentMethod.Values.Cash)
            .AddLine(waterTapFee)
            .AddAppliedPayment(waterTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve water paid when water fees are paid but not irrigation fees");
    }

    [Test]
    public async Task ShouldAddCaseReceipt()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 20);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(11, PaymentMethod.Values.Cash)
            .AddAppliedPayment(9, PaymentMethod.Values.Check)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        var receiptDetail = cwService.GetReceiptDetail(caseDetail);
        Assert.That(receiptDetail.Receipt.IsFound(), Is.True, "Should add receipt");
        Assert.That(receiptDetail.Receipt.TotalAmountTendered, Is.EqualTo(20), "Should add receipt");
    }

    [Test]
    public async Task ShouldUploadCaseReceiptFile()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var fee1 = cwService.AddCaseFee(caseDetail, "BUILDERFEE", 20);
        var eventData = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(fee1)
            .AddAppliedPayment(11, PaymentMethod.Values.Cash)
            .AddAppliedPayment(9, PaymentMethod.Values.Check)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData);
        await HandlePaymentTransactionCompleted(sp);
        caseDetail = cwService.GetCaseDetail(caseDetail);
        var receiptDetail = cwService.GetReceiptDetail(caseDetail);
        var receiptBytes = cwService.GetReceiptFile(receiptDetail);
        var receiptText = UTF8Encoding.UTF8.GetString(receiptBytes);
        Assert.That(receiptText, Is.EqualTo(FakePaymentTransactionService.Output(eventData.ID)), "Should upload receipt file");
    }

    [Test]
    public async Task ShouldResolveElectricPaidTaskAsPaidInsideCity_WhenElectricWasAlreadyPaidAndAllFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.LocationIsInsideTheCity(caseDetail);
        var gasFee = cwService.AddGasServiceFee(caseDetail);
        var electricFee = cwService.AddElectricFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, electricFee);
        var electricPaidTask = cwService.AddElectricPaidTask(caseDetail);
        cwService.AddGasPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(gasFee)
            .AddAppliedPayment(gasFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedElectricPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(electricPaidTask.ID).Task;
        Assert.That(updatedElectricPaidTask.ResultCode, Is.EqualTo("ELEC CITY"), "Should resolve electric paid task as paid when electric was already paid");
    }

    [Test]
    public async Task ShouldResolveElectricPaidTaskAsPaidOutsideCity_WhenElectricWasAlreadyPaidAndAllFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.LocationIsOutsideTheCity(caseDetail);
        var gasFee = cwService.AddGasServiceFee(caseDetail);
        var electricFee = cwService.AddElectricFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, electricFee);
        var electricPaidTask = cwService.AddElectricPaidTask(caseDetail);
        cwService.AddGasPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(gasFee)
            .AddAppliedPayment(gasFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedElectricPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(electricPaidTask.ID).Task;
        Assert.That(updatedElectricPaidTask.ResultCode, Is.EqualTo("ELEC CNTY"), "Should resolve electric paid task as paid when electric was already paid");
    }

    [Test]
    public async Task ShouldResolveElectricPaidTaskAsNotApplicable_WhenElectricFeeIsZeroAndOtherFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.AddElectricFee(caseDetail, amount: 0);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var electricPaidTask = cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(waterCapacityFee)
            .AddAppliedPayment(waterCapacityFee.Amount, PaymentMethod.Values.Cash)
            .AddLine(waterTapFee)
            .AddAppliedPayment(waterTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedElectricPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(electricPaidTask.ID).Task;
        Assert.That(updatedElectricPaidTask.ResultCode, Is.EqualTo("NOT APP"), "Should resolve electric paid task as not applicable when electric fee is zero and other fees are paid");
    }

    [Test]
    public async Task ShouldNotResolveElectricPaidTaskAsNotApplicable_WhenElectricFeeIsZeroAndOtherFeesAreNotPaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.AddElectricFee(caseDetail, amount: 0);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        var gasFee = cwService.AddGasServiceFee(caseDetail);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        var electricPaidTask = cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(waterCapacityFee)
            .AddAppliedPayment(waterCapacityFee.Amount, PaymentMethod.Values.Cash)
            .AddLine(waterTapFee)
            .AddAppliedPayment(waterTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedElectricPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(electricPaidTask.ID).Task;
        Assert.That(updatedElectricPaidTask.ResultCode, Is.EqualTo(""), "Should not resolve electric paid task as not applicable when electric fee is zero and other fees are not paid");
    }

    [Test]
    public async Task ShouldResolveElectricPaidTaskAsNotApplicable_WhenElectricFeeIsZeroAndOtherFeesArePaidLater()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.AddElectricFee(caseDetail, amount: 0);
        var gasFee = cwService.AddGasServiceFee(caseDetail);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        cwService.AddWaterPaidTask(caseDetail);
        cwService.AddGasPaidTask(caseDetail);
        var electricPaidTask = cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(waterCapacityFee)
            .AddAppliedPayment(waterCapacityFee.Amount, PaymentMethod.Values.Cash)
            .AddLine(waterTapFee)
            .AddAppliedPayment(waterTapFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var eventData2 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(gasFee)
            .AddAppliedPayment(gasFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData2);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedElectricPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(electricPaidTask.ID).Task;
        Assert.That(updatedElectricPaidTask.ResultCode, Is.EqualTo("NOT APP"), "Should resolve electric paid task as not applicable when electric fee is zero and other fees are paid later");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTaskAsWaterPaid_WhenWaterWasAlreadyPaidAndAllFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.LocationIsInsideTheCity(caseDetail);
        var gasFee = cwService.AddGasServiceFee(caseDetail);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, waterCapacityFee);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, waterTapFee);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        cwService.AddGasPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(gasFee)
            .AddAppliedPayment(gasFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("WT WATER"), "Should resolve water paid task as paid when water was already paid");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTaskAsIrrigationPaid_WhenWaterWasAlreadyPaidAndAllFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.LocationIsInsideTheCity(caseDetail);
        var gasFee = cwService.AddGasServiceFee(caseDetail);
        var irrigationCapacityFee = cwService.AddIrrigationCapacityFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, irrigationCapacityFee);
        var irrigationTapFee = cwService.AddIrrigationTapFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, irrigationTapFee);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        cwService.AddGasPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(gasFee)
            .AddAppliedPayment(gasFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("WT IRRIG"), "Should resolve water paid task as paid when water was already paid");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTaskAsWaterAndIrrigationPaid_WhenWaterWasAlreadyPaidAndAllFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        cwService.LocationIsInsideTheCity(caseDetail);
        var gasFee = cwService.AddGasServiceFee(caseDetail);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, waterCapacityFee);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, waterTapFee);
        var irrigationCapacityFee = cwService.AddIrrigationCapacityFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, irrigationCapacityFee);
        var irrigationTapFee = cwService.AddIrrigationTapFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, irrigationTapFee);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        cwService.AddGasPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(gasFee)
            .AddAppliedPayment(gasFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("WT WTR IRR"), "Should resolve water paid task as paid when water was already paid");
    }

    [Test]
    public async Task ShouldResolveWaterPaidTaskAsNotApplicable_WhenWaterFeeIsZeroAndOtherFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var waterCapacityFee = cwService.AddWaterCapacityFee(caseDetail, amount: 0);
        var waterTapFee = cwService.AddWaterTapFee(caseDetail, amount: 0);
        var waterPaidTask = cwService.AddWaterPaidTask(caseDetail);
        cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee)
            .AddAppliedPayment(electricFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedWaterPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(waterPaidTask.ID).Task;
        Assert.That(updatedWaterPaidTask.ResultCode, Is.EqualTo("NOT APP"), "Should resolve water paid task as not applicable when water fee is zero and other fees are paid");
    }

    [Test]
    public async Task ShouldResolveHydrantPaidTaskAsPaid_WhenHydrantWasAlreadyPaidAndAllFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var hydrantFee = cwService.AddFireHydrantFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, hydrantFee);
        var hydrantPaidTask = cwService.AddFireHydrantPaidTask(caseDetail);
        cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee)
            .AddAppliedPayment(electricFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedHydrantPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(hydrantPaidTask.ID).Task;
        Assert.That(updatedHydrantPaidTask.ResultCode, Is.EqualTo("PAIDACTIVE"), "Should resolve hydrant paid task as paid when hydrant was already paid");
    }

    [Test]
    public async Task ShouldResolveHydrantPaidTaskAsNotApplicable_WhenHydrantFeeIsZeroAndOtherFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var hydrantFee = cwService.AddFireHydrantFee(caseDetail, amount: 0);
        var hydrantPaidTask = cwService.AddFireHydrantPaidTask(caseDetail);
        cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee)
            .AddAppliedPayment(electricFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedHydrantPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(hydrantPaidTask.ID).Task;
        Assert.That(updatedHydrantPaidTask.ResultCode, Is.EqualTo("NOT APP"), "Should resolve hydrant paid task as not applicable when hydrant fee is zero and other fees are paid");
    }

    [Test]
    public async Task ShouldResolveGasPaidTaskAsNotApplicable_WhenGasFeeIsZeroAndAllFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var gasFee = cwService.AddGasServiceFee(caseDetail, amount: 0);
        var gasPaidTask = cwService.AddGasPaidTask(caseDetail);
        cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee)
            .AddAppliedPayment(electricFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedGasPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(gasPaidTask.ID).Task;
        Assert.That(updatedGasPaidTask.ResultCode, Is.EqualTo("NOT APP"), "Should resolve gas paid task as not applicable when gas fee is zero and other fees are paid");
    }

    [Test]
    public async Task ShouldResolveGasPaidTaskAsPaid_WhenGasWasAlreadyPaidAndAllFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var gasFee = cwService.AddGasServiceFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, gasFee);
        var gasPaidTask = cwService.AddGasPaidTask(caseDetail);
        cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee)
            .AddAppliedPayment(electricFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedGasPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(gasPaidTask.ID).Task;
        Assert.That(updatedGasPaidTask.ResultCode, Is.EqualTo("PAIDACTIVE"), "Should resolve gas paid task as paid when gas was already paid");
    }

    [Test]
    public async Task ShouldResolveSewerPaidTaskAsNotApplicable_WhenSewerFeeIsZeroAndAllFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var sewerCapacityFee = cwService.AddSewerCapacityFee(caseDetail, amount: 0);
        var sewerTapFee = cwService.AddSewerTapFee(caseDetail, amount: 0);
        var sewerPaidTask = cwService.AddSewerPaidTask(caseDetail);
        cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee)
            .AddAppliedPayment(electricFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedSewerPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(sewerPaidTask.ID).Task;
        Assert.That(updatedSewerPaidTask.ResultCode, Is.EqualTo("NOT APP"), "Should resolve sewer paid task as not applicable when sewer fee is zero and other fees are paid");
    }

    [Test]
    public async Task ShouldResolveSewerPaidTaskAsPaid_WhenSewerWasAlreadyPaidAndAllFeesArePaid()
    {
        var sp = await Setup();
        var cwService = sp.GetRequiredService<FakeCityworksService>();
        var caseDetail = AddCaseDetail(cwService);
        var electricFee = cwService.AddElectricFee(caseDetail);
        var sewerCapacityFee = cwService.AddSewerCapacityFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, sewerCapacityFee);
        var sewerTapFee = cwService.AddSewerTapFee(caseDetail);
        await AddCasePayment(cwService, caseDetail, sewerTapFee);
        var sewerPaidTask = cwService.AddSewerPaidTask(caseDetail);
        cwService.AddElectricPaidTask(caseDetail);
        var eventData1 = new PaymentTransactionEventDataBuilder(caseDetail)
            .AddLine(electricFee)
            .AddAppliedPayment(electricFee.Amount, PaymentMethod.Values.Cash)
            .Build();
        await NotifyPaymentTransactionCompleted(sp, eventData1);
        await HandlePaymentTransactionCompleted(sp);
        var updatedCaseDetail = GetCaseDetail(sp, caseDetail);
        var updatedSewerPaidTask = updatedCaseDetail.GetTaskDetailOrDefault(sewerPaidTask.ID).Task;
        Assert.That(updatedSewerPaidTask.ResultCode, Is.EqualTo("PAIDACTIVE"), "Should resolve sewer paid task as paid when sewer was already paid");
    }

    private Task<IServiceProvider> Setup(string envName = "Development")
    {
        var host = new CityworksOfficeTestHost();
        return host.Setup(envName);
    }

    private static CaseDetailModel AddCaseDetail(FakeCityworksService cwService) =>
        cwService.AddInstallServiceCase
        (
            location: "101 Somewhere Ln, Greer, SC 29652",
            accountType: "Residential",
            companyName: "Developer",
            billingAddress: new Address(new StreetAddress("101", "Arrakis Ln"), "Greer", "SC", new ZipCode(29652)),
            coords: new CwCoordinates()
        );

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

    private static Task AddCasePayment(FakeCityworksService cwService, CaseDetailModel caseDetail, CaseFeeModel fee) =>
        cwService.AddCasePayment
        (
            new AddCasePaymentRequest
            {
                CaseID = caseDetail.Case.ID,
                CaseFeeID = fee.ID,
                AmountPaid = fee.Amount,
                TenderTypeID = 1,
                TimePaid = DateTimeOffset.Now
            },
            default
        );

}