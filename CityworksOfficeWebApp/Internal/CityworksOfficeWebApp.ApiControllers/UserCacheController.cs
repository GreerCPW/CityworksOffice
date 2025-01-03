// Generated Code
namespace CityworksOfficeWebApp.ApiControllers;
[Authorize]
public sealed partial class UserCacheController : Controller
{
    private readonly CityworksOfficeAppApi api;
    public UserCacheController(CityworksOfficeAppApi api)
    {
        this.api = api;
    }

    [HttpPost]
    public Task<ResultContainer<EmptyActionResult>> ClearCache([FromBody] string requestData, CancellationToken ct)
    {
        return api.UserCache.ClearCache.Execute(requestData, ct);
    }
}