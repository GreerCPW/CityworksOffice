using XTI_CityworksOfficeWebAppApi.Receivables;

namespace XTI_CityworksOfficeWebAppApi;

partial class CityworksOfficeAppApi
{
    private ReceivablesGroup? _Receivables;

    public ReceivablesGroup Receivables { get => _Receivables ?? throw new ArgumentNullException(nameof(_Receivables)); }

    partial void createReceivablesGroup(IServiceProvider sp)
    {
        _Receivables = new ReceivablesGroup
        (
            source.AddGroup(nameof(Receivables)),
            sp
        );
    }
}