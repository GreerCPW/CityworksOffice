// Generated Code
namespace CityworksOfficeWebApp.ApiControllers;
[Authorize]
public sealed partial class UserController : Controller
{
    private readonly CityworksOfficeAppApi api;
    public UserController(CityworksOfficeAppApi api)
    {
        this.api = api;
    }

    [HttpPost]
    public Task<ResultContainer<ResourcePathAccess[]>> GetUserAccess([FromBody] ResourcePath[] model, CancellationToken ct)
    {
        return api.Group("User").Action<ResourcePath[], ResourcePathAccess[]>("GetUserAccess").Execute(model, ct);
    }

    public async Task<IActionResult> UserProfile(CancellationToken ct)
    {
        var result = await api.Group("User").Action<EmptyRequest, WebRedirectResult>("UserProfile").Execute(new EmptyRequest(), ct);
        return Redirect(result.Data!.Url);
    }

    [HttpPost]
    public Task<ResultContainer<LinkModel[]>> GetMenuLinks([FromBody] string model, CancellationToken ct)
    {
        return api.Group("User").Action<string, LinkModel[]>("GetMenuLinks").Execute(model, ct);
    }

    public async Task<IActionResult> Logout(LogoutRequest model, CancellationToken ct)
    {
        var result = await api.Group("User").Action<LogoutRequest, WebRedirectResult>("Logout").Execute(model, ct);
        return Redirect(result.Data!.Url);
    }
}