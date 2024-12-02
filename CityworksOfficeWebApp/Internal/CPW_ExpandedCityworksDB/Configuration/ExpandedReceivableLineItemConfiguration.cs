using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CPW_ExpandedCityworksDB.Configuration;

internal sealed class ExpandedReceivableLineItemConfiguration : IEntityTypeConfiguration<ExpandedReceivableLineItemEntity>
{
    public void Configure(EntityTypeBuilder<ExpandedReceivableLineItemEntity> builder)
    {
        builder.HasKey(rli => rli.LineItemID);
        builder.Property(rli => rli.CaseID).HasPrecision(10, 0).HasConversion<decimal>();
        builder.Property(rli => rli.FeeID).HasPrecision(10, 0).HasConversion<decimal>();
        builder.Property(rli => rli.AmountDue).HasPrecision(12, 2);
        builder.ToView("ExpandedReceivableLineItems");
    }
}
