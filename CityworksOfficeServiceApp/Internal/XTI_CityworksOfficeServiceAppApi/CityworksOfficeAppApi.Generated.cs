using XTI_CityworksOfficeServiceAppApi.Jobs;
using XTI_CityworksOfficeServiceAppApi.Receivables;

// Generated Code
#nullable enable
namespace XTI_CityworksOfficeServiceAppApi;
public sealed partial class CityworksOfficeAppApi : AppApiWrapper
{
    internal CityworksOfficeAppApi(AppApi source, CityworksOfficeAppApiBuilder builder) : base(source)
    {
        Jobs = builder.Jobs.Build();
        Receivables = builder.Receivables.Build();
        Configure();
    }

    partial void Configure();
    public JobsGroup Jobs { get; }
    public ReceivablesGroup Receivables { get; }
}