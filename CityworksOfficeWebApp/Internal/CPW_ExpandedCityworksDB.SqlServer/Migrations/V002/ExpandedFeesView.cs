namespace CPW_ExpandedCityworksDB.SqlServer.Migrations.V002;

internal static class ExpandedFeesView
{
    public static string Sql(string cwDatabaseName) =>
        $"""
        create or alter view ExpandedFees
        as
        select f.CA_Fee_ID FeeID, f.CA_Object_ID CaseID, f.Case_Number CaseNumber, f.Case_Status CaseStatus,
            f.Amount FeeAmount, f.Fee_Code FeeCode, isnull(f.Fee_Desc, '') FeeDescription,
            isnull(f.Payment_Amount, 0) PaymentAmount, isnull(fs.Account_Code, '') GlAccountNumber,
            case when fs.Registered_Flag = 'Y' then 1 else 0 end IsFeeRegistered
        from {cwDatabaseName}.azteca.CA_Fees_Vw f
        left outer join {cwDatabaseName}.azteca.Fee_Setup fs
        on f.Fee_Setup_ID = fs.Fee_Setup_ID
        """;
}
