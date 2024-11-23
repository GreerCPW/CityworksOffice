using CityworksOfficeServiceApp.Services;
using Microsoft.Extensions.Caching.Memory;
using XTI_Core;
using XTI_Jobs;
using XTI_Jobs.Abstractions;

namespace CPW_HandlePaymentTransactionCompleted;

public sealed class HandlePaymentTransactionCompletedActionFactory : IJobActionFactory
{
    private readonly IMemoryCache cache;
    private readonly ICityworksService cwService;
    private readonly IClock clock;

    public HandlePaymentTransactionCompletedActionFactory(IMemoryCache cache, ICityworksService cwService, IClock clock)
    {
        this.cache = cache;
        this.cwService = cwService;
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
        else
        {
            throw new NotSupportedException($"Task '{jobTask.TaskKey.DisplayText}' is not supported");
        }
        return action;
    }

    public NextTaskModel[] FirstTasks(string taskData) =>
        [new NextTaskModel(HandlePaymentTransactionCompletedInfo.LoadCaseDetail, taskData)];

}
