using XTI_CityworksOfficeServiceAppApi.Jobs;
using XTI_CityworksOfficeServiceAppApi.Receivables;

// Generated Code
#nullable enable
namespace XTI_CityworksOfficeServiceAppApi;
public sealed partial class CityworksOfficeAppApiBuilder
{
    private readonly AppApi source;
    private readonly IServiceProvider sp;
    public CityworksOfficeAppApiBuilder(IServiceProvider sp, IAppApiUser user)
    {
        source = new AppApi(sp, CityworksOfficeAppKey.Value, user);
        this.sp = sp;
        Jobs = new JobsGroupBuilder(source.AddGroup("Jobs"));
        Receivables = new ReceivablesGroupBuilder(source.AddGroup("Receivables"));
        Configure();
    }

    partial void Configure();
    public JobsGroupBuilder Jobs { get; }
    public ReceivablesGroupBuilder Receivables { get; }

    public CityworksOfficeAppApi Build() => new CityworksOfficeAppApi(source, this);
}