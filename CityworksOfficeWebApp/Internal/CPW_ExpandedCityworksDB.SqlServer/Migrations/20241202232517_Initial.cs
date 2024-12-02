using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPW_ExpandedCityworksDB.SqlServer.Migrations;

public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(V002.ExpandedFeesView.Sql(EnvironmentSettings.GetCityworksDatabaseName()));
        migrationBuilder.Sql(V002.ExpandedReceivableLineItems.Sql(EnvironmentSettings.GetPaymentDatabaseName()));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
