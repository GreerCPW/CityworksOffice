namespace CPW_ExpandedCityworksDB;

public sealed class ExpandedReceivableLineItemEntity
{
    public int LineItemID { get; set; }
    public int ReceivableID { get; set; }
    public long CaseID { get; set; }
    public string CaseNumber { get; set; } = "";
    public long FeeID { get; set; }
    public string FeeCode { get; set; } = "";
    public string FeeDescription { get; set; } = "";
    public decimal AmountDue { get; set; }
    public string GlAccountNumber { get; set; } = "";
}
