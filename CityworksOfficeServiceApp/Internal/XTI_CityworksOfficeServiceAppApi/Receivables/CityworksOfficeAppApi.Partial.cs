using XTI_CityworksOfficeServiceAppApi.Receivables;

namespace XTI_CityworksOfficeServiceAppApi;

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