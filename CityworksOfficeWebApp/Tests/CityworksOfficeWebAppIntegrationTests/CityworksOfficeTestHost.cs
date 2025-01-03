using CPW_ExpandedCityworksDB.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XTI_App.Abstractions;
using XTI_App.Api;
using XTI_App.Extensions;
using XTI_App.Fakes;
using XTI_Core;
using XTI_Core.Extensions;
using XTI_Core.Fakes;
using XTI_HubAppClient.Extensions;
using XTI_PaymentTransactionAppClient;
using XTI_Secrets.Extensions;
using XTI_TempLog.Abstractions;
using XTI_WebApp.Api;

namespace CityworksOfficeWebAppIntegrationTests;

internal sealed class CityworksOfficeTestHost
{
    public Task<IServiceProvider> Setup(string envName, Action<IServiceCollection>? configure = null)
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", envName);
        var xtiEnv = XtiEnvironment.Parse(envName);
        var appKey = CityworksOfficeAppKey.Value;
        var builder = new XtiHostBuilder(xtiEnv, appKey.Name.DisplayText, appKey.Type.DisplayText, []);
        builder.Services.AddAppServices();
        builder.Services.AddScoped<IAppEnvironmentContext, FakeAppEnvironmentContext>();
        builder.Services.AddConfigurationOptions<DefaultWebAppOptions>();
        builder.Services.AddSingleton(sp => sp.GetRequiredService<DefaultWebAppOptions>().HubClient);
        builder.Services.AddSingleton(sp => sp.GetRequiredService<DefaultWebAppOptions>().XtiToken);
        builder.Services.AddFakesForXtiApp();
        builder.Services.AddSingleton<IHostEnvironment>
        (
            _ => new FakeHostEnvironment { EnvironmentName = envName }
        );
        builder.Services.AddFileSecretCredentials(xtiEnv);
        builder.Services.AddHubClientServices();
        builder.Services.AddInstallationUserXtiToken();
        builder.Services.AddSystemUserXtiToken();
        builder.Services.AddXtiTokenAccessorFactory
        (
            (sp, config) =>
            {
                config.AddToken(() => sp.GetRequiredService<InstallationUserXtiToken>());
                config.AddToken(() => sp.GetRequiredService<SystemUserXtiToken>());
                config.UseDefaultToken<SystemUserXtiToken>();
            }
        );
        builder.Services.AddSingleton<XtiFolder>();
        builder.Services.AddSingleton(sp => sp.GetRequiredService<XtiFolder>().AppDataFolder(appKey));
        builder.Services.AddSingleton(_ => appKey);
        builder.Services.AddSingleton(_ => AppVersionKey.Current);
        builder.Services.AddCityworksOfficeAppApiServices();
        builder.Services.AddScoped<CityworksOfficeAppApiFactory>();
        builder.Services.AddScoped<AppApiFactory>(sp => sp.GetRequiredService<CityworksOfficeAppApiFactory>());
        builder.Services.AddScoped(sp => sp.GetRequiredService<AppApiFactory>().Create(sp.GetRequiredService<IAppApiUser>()));
        builder.Services.AddScoped(sp => (CityworksOfficeAppApi)sp.GetRequiredService<IAppApi>());
        builder.Services.AddScoped<IAppContext>(sp => sp.GetRequiredService<FakeAppContext>());
        builder.Services.AddScoped<ICurrentUserName>(sp => sp.GetRequiredService<FakeCurrentUserName>());
        builder.Services.AddScoped<IUserContext>(sp => sp.GetRequiredService<FakeUserContext>());
        builder.Services.AddExpandedCityworksDbContextForSqlServer();
        builder.Services.AddPaymentTransactionAppClient();
        if (configure != null)
        {
            configure(builder.Services);
        }
        var sp = builder.Build().Scope();
        var apiFactory = sp.GetRequiredService<AppApiFactory>();
        var template = apiFactory.CreateTemplate();
        var appContext = sp.GetRequiredService<FakeAppContext>();
        var app = appContext.RegisterApp(template.ToModel());
        appContext.SetCurrentApp(app);
        var userContext = (FakeUserContext)sp.GetRequiredService<ISourceUserContext>();
        var userName = new AppUserName("admin.user");
        userContext.AddUser(userName);
        userContext.SetCurrentUser(userName);
        userContext.SetUserRoles(AppRoleName.Admin);
        return Task.FromResult(sp);
    }
}