namespace CPW_ExpandedCityworksDB;

public sealed class ExpandedFeeEntity
{
    public long FeeID { get; set; }
    public long CaseID { get; set; }
    public string CaseNumber { get; set; } = "";
    public string CaseStatus { get; set; } = "";
    public bool IsFeeRegistered { get; set; }
    public string FeeCode { get; set; } = "";
    public string FeeDescription { get; set; } = "";
    public decimal FeeAmount { get; set; }
    public string GlAccountNumber { get; set; } = "";
    public decimal PaymentAmount { get; set; }
}
