using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using XTI_Core;
using XTI_Core.Extensions;
using XTI_DB;

namespace CPW_ExpandedCityworksDB.SqlServer;

public static class Extensions
{
    public static void AddExpandedCityworksDbContextForSqlServer(this IServiceCollection services)
    {
        services.AddConfigurationOptions<DbOptions>(DbOptions.DB);
        services.AddDbContextFactory<ExpandedCityworksDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<DbOptions>();
            var xtiEnv = sp.GetRequiredService<XtiEnvironment>();
            var connectionString = new XtiConnectionString(dbOptions, new XtiDbName(xtiEnv.EnvironmentName, "ExpandedCityworks")).Value();
            options.UseSqlServer
            (
                connectionString,
                b => b.MigrationsAssembly("CPW_ExpandedCityworksDB.SqlServer")
            );
            if (xtiEnv.IsDevelopmentOrTest())
            {
                options.EnableSensitiveDataLogging();
            }
            else
            {
                options.EnableSensitiveDataLogging(false);
            }
        });
    }
}