using CPW_HandlePaymentTransactionCompleted;
using XTI_Jobs;

namespace XTI_CityworksOfficeServiceAppApi;

public sealed class CityworksOfficeJobSetup
{
    private readonly JobRegistrationBuilder jobRegistration;

    public CityworksOfficeJobSetup(JobRegistrationBuilder jobRegistration)
    {
        this.jobRegistration = jobRegistration;
    }

    public async Task Run()
    {
        await jobRegistration
            .AddJob(HandlePaymentTransactionCompletedInfo.JobKey)
                .TimeoutAfter(TimeSpan.FromHours(1))
                .AddTasks(HandlePaymentTransactionCompletedInfo.AllTasks)
            .Build()
            .Register();
    }
}
