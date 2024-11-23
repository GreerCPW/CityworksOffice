using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using XTI_Jobs;

namespace CPW_HandlePaymentTransactionCompleted;

internal sealed class ResolveCaseTaskAction : JobAction<ResolveCaseTaskRequest>
{
    private readonly ICityworksService cwService;

    public ResolveCaseTaskAction(ICityworksService cwService, TriggeredJobTask task) : base(task)
    {
        this.cwService = cwService;
    }

    protected override Task Execute(CancellationToken stoppingToken, TriggeredJobTask task, JobActionResultBuilder next, ResolveCaseTaskRequest data) =>
        cwService.ResolveCaseTask(data, stoppingToken);
}
