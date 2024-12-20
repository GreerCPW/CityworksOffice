using XTI_CityworksOfficeServiceAppApi.Jobs;

namespace XTI_CityworksOfficeServiceAppApi;

partial class CityworksOfficeAppApi
{
    private JobsGroup? _Jobs;

    public JobsGroup Jobs { get => _Jobs ?? throw new ArgumentNullException(nameof(_Jobs)); }

    partial void createJobsGroup(IServiceProvider sp)
    {
        _Jobs = new JobsGroup
        (
            source.AddGroup(nameof(Jobs)),
            sp
        );
    }
}