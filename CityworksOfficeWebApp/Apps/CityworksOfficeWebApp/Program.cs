using XTI_HubAppClient.WebApp.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using CityworksOfficeWebApp.ApiControllers;
using XTI_Core;
using XTI_CityworksOfficeWebAppApi;
using XTI_App.Api;
using CPW_ExpandedCityworksDB.SqlServer;
using XTI_PaymentTransactionAppClient;

var builder = XtiWebAppHost.CreateDefault(CityworksOfficeAppKey.Value, args);
var xtiEnv = XtiEnvironment.Parse(builder.Environment.EnvironmentName);
builder.Services.ConfigureXtiCookieAndTokenAuthentication(xtiEnv, builder.Configuration);
builder.Services.AddScoped<AppApiFactory, CityworksOfficeAppApiFactory>();
builder.Services.AddScoped(sp => (CityworksOfficeAppApi)sp.GetRequiredService<IAppApi>());
builder.Services.AddCityworksOfficeAppApiServices();
builder.Services.AddExpandedCityworksDbContextForSqlServer();
builder.Services.AddPaymentTransactionAppClient();
builder.Services
    .AddMvc()
    .AddJsonOptions(options =>
    {
        options.SetDefaultJsonOptions();
    })
    .AddMvcOptions(options =>
    {
        options.SetDefaultMvcOptions();
    });
builder.Services.AddControllersWithViews()
    .PartManager.ApplicationParts.Add
    (
        new AssemblyPart(typeof(HomeController).Assembly)
    );

var app = builder.Build();
app.UseXtiDefaults();
await app.RunAsync();