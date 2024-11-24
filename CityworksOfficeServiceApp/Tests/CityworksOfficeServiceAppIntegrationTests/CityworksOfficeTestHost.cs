using CityworksOfficeServiceApp.Implementations;
using CityworksOfficeServiceApp.Services;
using CPW_HandlePaymentTransactionCompleted;
using DinkToPdf;
using DinkToPdf.Contracts;
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
using XTI_Jobs;
using XTI_Jobs.Abstractions;
using XTI_ScheduledJobsAppClient;
using XTI_Secrets.Extensions;
using XTI_TempLog;
using XTI_TempLog.Fakes;
using XTI_WebApp.Api;

namespace CityworksOfficeServiceAppIntegrationTests;

internal sealed class CityworksOfficeTestHost
{
    public async Task<IServiceProvider> Setup(string envName, Action<IServiceCollection>? configure = null)
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", envName);
        var xtiEnv = XtiEnvironment.Parse(envName);
        var builder = new XtiHostBuilder(xtiEnv, CityworksOfficeInfo.AppKey.Name.DisplayText, CityworksOfficeInfo.AppKey.Type.DisplayText, []);
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
        builder.Services.AddSingleton(sp => sp.GetRequiredService<XtiFolder>().AppDataFolder(CityworksOfficeInfo.AppKey));
        builder.Services.AddSingleton(_ => CityworksOfficeInfo.AppKey);
        builder.Services.AddSingleton(_ => AppVersionKey.Current);
        builder.Services.AddCityworksOfficeAppApiServices();
        builder.Services.AddScoped<CityworksOfficeAppApiFactory>();
        builder.Services.AddScoped<AppApiFactory>(sp => sp.GetRequiredService<CityworksOfficeAppApiFactory>());
        builder.Services.AddScoped(sp => sp.GetRequiredService<AppApiFactory>().Create(sp.GetRequiredService<IAppApiUser>()));
        builder.Services.AddScoped(sp => (CityworksOfficeAppApi)sp.GetRequiredService<IAppApi>());
        builder.Services.AddScoped<IAppContext>(sp => sp.GetRequiredService<FakeAppContext>());
        builder.Services.AddScoped<ICurrentUserName>(sp => sp.GetRequiredService<FakeCurrentUserName>());
        builder.Services.AddScoped<IUserContext>(sp => sp.GetRequiredService<FakeUserContext>());
        builder.Services.AddScheduledJobsAppClient();
        builder.Services.AddScoped<IJobDb, SjcJobDb>();
        builder.Services.AddScoped<EventMonitorBuilder>();
        builder.Services.AddScoped<JobRegistrationBuilder>();
        builder.Services.AddScoped<CityworksOfficeJobSetup>();
        builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        builder.Services.AddScoped<HandlePaymentTransactionCompletedActionFactory>();
        builder.Services.AddScoped<IReceiptWriterFactory, PdfReceiptWriterFactory>();
        if (configure != null)
        {
            configure(builder.Services);
        }
        var sp = builder.Build().Scope();
        var apiFactory = sp.GetRequiredService<AppApiFactory>();
        var template = apiFactory.CreateTemplate();
        var appContext = sp.GetRequiredService<FakeAppContext>();
        var app = appContext.AddApp(template.ToModel());
        appContext.SetCurrentApp(app);
        var userContext = (FakeUserContext)sp.GetRequiredService<ISourceUserContext>();
        var userName = new AppUserName("admin.user");
        userContext.AddUser(userName);
        userContext.SetCurrentUser(userName);
        userContext.SetUserRoles(AppRoleName.Admin);
        var jobSetup = sp.GetRequiredService<CityworksOfficeJobSetup>();
        await jobSetup.Run();
        return sp;
    }
}