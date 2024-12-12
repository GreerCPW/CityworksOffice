using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XTI_App.Abstractions;
using XTI_App.Api;
using XTI_App.Extensions;
using XTI_App.Fakes;
using XTI_Core;
using XTI_Core.Extensions;
using XTI_Core.Fakes;
using XTI_CityworksOfficeServiceAppApi;
using XTI_WebApp.Fakes;
using XTI_Jobs.Abstractions;
using XTI_Jobs;
using XTI_JobsDB.EF;
using XTI_JobsDB.InMemory;
using CityworksOfficeServiceApp.Services;
using CPW_HandlePaymentTransactionCompleted;
using CPW_PaymentTransaction.Events;

namespace CityworksOfficeServiceAppTests;

internal sealed class CityworksOfficeTestHost
{
    public async Task<IServiceProvider> Setup(string envName, Action<IServiceCollection>? configure = null)
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", envName);
        var xtiEnv = XtiEnvironment.Parse(envName);
        var builder = new XtiHostBuilder(xtiEnv, CityworksOfficeInfo.AppKey.Name.DisplayText, CityworksOfficeInfo.AppKey.Type.DisplayText, new string[0]);
        builder.Services.AddSingleton<IHostEnvironment>
        (
            _ => new FakeHostEnvironment { EnvironmentName = envName }
        );
        builder.Services.AddFakesForXtiWebApp();
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
        builder.Services.AddJobDbContextForInMemory();
        builder.Services.AddScoped<IJobDb, EfJobDb>();
        builder.Services.AddScoped<EventRegistrationBuilder>();
        builder.Services.AddScoped<JobRegistrationBuilder>();
        builder.Services.AddScoped<IncomingEventFactory>();
        builder.Services.AddScoped<EventMonitorBuilder>();
        builder.Services.AddScoped<HandlePaymentTransactionCompletedActionFactory>();
        builder.Services.AddScoped<CityworksOfficeJobSetup>();
        builder.Services.AddScoped<FakeCityworksService>();
        builder.Services.AddScoped<ICityworksService>(sp => sp.GetRequiredService<FakeCityworksService>());
        builder.Services.AddScoped<FakePaymentTransactionService>();
        builder.Services.AddScoped<IPaymentTransactionService>(sp => sp.GetRequiredService<FakePaymentTransactionService>());
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
        var eventRegistration = sp.GetRequiredService<EventRegistrationBuilder>();
        await eventRegistration
            .AddEvent(PaymentTransactionEvents.PaymentTransactionCompleted)
                .ActiveFor(TimeSpan.FromDays(7))
                .IgnoreDuplicates().WhenSourceKeysOnlyAreEqual()
            .Build()
            .Register();
        var jobSetup = sp.GetRequiredService<CityworksOfficeJobSetup>();
        await jobSetup.Run();
        return sp;
    }
}