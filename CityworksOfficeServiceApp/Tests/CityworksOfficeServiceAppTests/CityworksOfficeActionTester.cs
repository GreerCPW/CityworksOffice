using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using XTI_App.Abstractions;
using XTI_App.Api;
using XTI_App.Fakes;
using XTI_CityworksOfficeServiceAppApi;

namespace CityworksOfficeServiceAppTests;

internal static class CityworksOfficeActionTester
{
    public static CityworksOfficeActionTester<TModel, TResult> Create<TModel, TResult>(IServiceProvider services, Func<CityworksOfficeAppApi, AppApiAction<TModel, TResult>> getAction)
    {
        return new CityworksOfficeActionTester<TModel, TResult>(services, getAction);
    }
}

internal interface ICityworksOfficeActionTester
{
    IServiceProvider Services { get; }
    CityworksOfficeActionTester<TOtherModel, TOtherResult> Create<TOtherModel, TOtherResult>(Func<CityworksOfficeAppApi, AppApiAction<TOtherModel, TOtherResult>> getAction);
}

internal sealed class CityworksOfficeActionTester<TModel, TResult> : ICityworksOfficeActionTester
{
    private readonly Func<CityworksOfficeAppApi, AppApiAction<TModel, TResult>> getAction;

    public CityworksOfficeActionTester
    (
        IServiceProvider services,
        Func<CityworksOfficeAppApi, AppApiAction<TModel, TResult>> getAction
    )
    {
        Services = services;
        this.getAction = getAction;
    }

    public CityworksOfficeActionTester<TOtherModel, TOtherResult> Create<TOtherModel, TOtherResult>
    (
        Func<CityworksOfficeAppApi, AppApiAction<TOtherModel, TOtherResult>> getAction
    )
    {
        return CityworksOfficeActionTester.Create(Services, getAction);
    }

    public IServiceProvider Services { get; }

    public void Logout()
    {
        var currentUserName = Services.GetRequiredService<FakeCurrentUserName>();
        currentUserName.SetUserName(AppUserName.Anon);
    }

    public void LoginAsAdmin()
    {
        var currentUserName = Services.GetRequiredService<FakeCurrentUserName>();
        currentUserName.SetUserName(new AppUserName("admin.user"));
    }

    public void Login(params AppRoleName[]? roleNames) => Login(ModifierCategoryName.Default, ModifierKey.Default, roleNames);

    public void Login(ModifierCategoryName categoryName, ModifierKey modifier, params AppRoleName[]? roleNames)
    {
        var userContext = Services.GetRequiredService<FakeUserContext>();
        var userName = new AppUserName("loggedinUser");
        userContext.AddUser(userName);
        userContext.SetCurrentUser(userName);
        userContext.SetUserRoles(categoryName, modifier, roleNames ?? []);
    }

    public Task<TResult> Execute(TModel model) =>
        Execute(model, ModifierKey.Default);

    public async Task<TResult> Execute(TModel model, ModifierKey modKey)
    {
        var appContext = Services.GetRequiredService<IAppContext>();
        var appApiFactory = Services.GetRequiredService<AppApiFactory>();
        var apiForSuperUser = (CityworksOfficeAppApi)appApiFactory.CreateForSuperUser();
        var actionForSuperUser = getAction(apiForSuperUser);
        var appKey = Services.GetRequiredService<AppKey>();
        var userContext = Services.GetRequiredService<ISourceUserContext>();
        var modKeyAccessor = Services.GetRequiredService<FakeModifierKeyAccessor>();
        modKeyAccessor.SetValue(modKey);
        var currentUserName = Services.GetRequiredService<ICurrentUserName>();
        var currentUserAccess = new CurrentUserAccess(userContext, appContext, currentUserName);
        var apiUser = new AppApiUser(currentUserAccess, modKeyAccessor);
        var appApi = (CityworksOfficeAppApi)appApiFactory.Create(apiUser);
        var action = getAction(appApi);
        var result = await action.Invoke(model);
        return result;
    }
}