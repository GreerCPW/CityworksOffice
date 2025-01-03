﻿using CityworksOfficeServiceApp.Services;
using Microsoft.Extensions.Caching.Memory;
using XTI_Core;
using XTI_Jobs;
using XTI_Jobs.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

public sealed class HandlePaymentTransactionCompletedActionFactory : IJobActionFactory
{
    private readonly IMemoryCache cache;
    private readonly ICityworksService cwService;
    private readonly IPaymentTransactionService payTranService;
    private readonly IClock clock;

    public HandlePaymentTransactionCompletedActionFactory(IMemoryCache cache, ICityworksService cwService, IPaymentTransactionService payTranService, IClock clock)
    {
        this.cache = cache;
        this.cwService = cwService;
        this.payTranService = payTranService;
        this.clock = clock;
    }

    public IJobAction CreateJobAction(TriggeredJobTask jobTask)
    {
        IJobAction action;
        if (jobTask.TaskKey.Equals(HandlePaymentTransactionCompletedInfo.LoadCaseDetail))
        {
            action = new LoadCaseDetailAction(cache, cwService, jobTask);
        }
        else if (jobTask.TaskKey.Equals(HandlePaymentTransactionCompletedInfo.AddCasePayment))
        {
            action = new AddCasePaymentAction(cwService, clock, jobTask);
        }
        else if (jobTask.TaskKey.Equals(HandlePaymentTransactionCompletedInfo.LoadTaskResolutions))
        {
            action = new LoadTaskResolutionsAction(cwService, jobTask);
        }
        else if (jobTask.TaskKey.Equals(HandlePaymentTransactionCompletedInfo.ResolveCaseTask))
        {
            action = new ResolveCaseTaskAction(cwService, jobTask);
        }
        else if (jobTask.TaskKey.Equals(HandlePaymentTransactionCompletedInfo.AddCaseReceipt))
        {
            action = new AddCaseReceiptAction(cwService, jobTask);
        }
        else if (jobTask.TaskKey.Equals(HandlePaymentTransactionCompletedInfo.UploadReceiptFile))
        {
            action = new UploadReceiptFileAction(cwService, payTranService, jobTask);
        }
        else
        {
            throw new NotSupportedException($"Task '{jobTask.TaskKey.DisplayText}' is not supported");
        }
        return action;
    }

    public NextTaskModel[] FirstTasks(string taskData) =>
        [new NextTaskModel(HandlePaymentTransactionCompletedInfo.LoadCaseDetail, taskData)];

}
