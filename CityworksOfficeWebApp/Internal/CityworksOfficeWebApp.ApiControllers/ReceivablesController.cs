// Generated Code
namespace CityworksOfficeWebApp.ApiControllers;
[Authorize]
public sealed partial class ReceivablesController : Controller
{
    private readonly CityworksOfficeAppApi api;
    public ReceivablesController(CityworksOfficeAppApi api)
    {
        this.api = api;
    }

    [HttpPost]
    public Task<ResultContainer<EmptyActionResult>> AddOrUpdateReceivables(CancellationToken ct)
    {
        return api.Group("Receivables").Action<EmptyRequest, EmptyActionResult>("AddOrUpdateReceivables").Execute(new EmptyRequest(), ct);
    }
}