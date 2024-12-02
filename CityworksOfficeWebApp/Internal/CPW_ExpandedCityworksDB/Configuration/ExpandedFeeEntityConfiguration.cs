using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CPW_ExpandedCityworksDB.Configuration;

internal sealed class ExpandedFeeEntityConfiguration : IEntityTypeConfiguration<ExpandedFeeEntity>
{
    public void Configure(EntityTypeBuilder<ExpandedFeeEntity> builder)
    {
        builder.HasKey(f => f.FeeID);
        builder.Property(f => f.FeeID).HasPrecision(10, 0).HasConversion<decimal>();
        builder.Property(f => f.FeeAmount).HasPrecision(22, 4);
        builder.Property(f => f.CaseID).HasPrecision(10, 0).HasConversion<decimal>();
        builder.Property(f => f.PaymentAmount).HasPrecision(38, 2);
        builder.ToView("ExpandedFees");
    }
}
