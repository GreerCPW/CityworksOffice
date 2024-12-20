namespace CPW_ExpandedCityworksDB.SqlServer.Migrations.V002;

internal static class ExpandedReceivableLineItems
{
    public static string Sql(string paymentDatabaseName) =>
        $"""
        create or alter view ExpandedReceivableLineItems
        as
        select LineItemID,  
            isnull(try_cast(LineItemSourceKey as decimal(10, 0)), 0) FeeID, 
            LineItemSourceCode FeeCode, LineItemSourceDescription FeeDescription, 
            AmountDue, replace(GlAccountNumber, '-000000', '') GlAccountNumber, ReceivableID, 
            isnull(try_cast(ReceivableSourceKey as decimal(10, 0)), 0) CaseID, 
            ReceivableSourceDescription CaseNumber
        from {paymentDatabaseName}.dbo.ExpandedReceivableLineItems
        where
            SourceAppCode = 'PLL'
        """;
}
