using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;
using RainbowOF.Models.Woo;

namespace RainbowOF.Datsa.SQL.EntityConfigs.Items
{
    internal class WooProductAttributeTermMappingConfig : IEntityTypeConfiguration<WooProductAttributeTermMapping>
    {
        public void Configure(EntityTypeBuilder<WooProductAttributeTermMapping> WooProductAttributeTermMappingConfigModelBuilder)
        {
            WooProductAttributeTermMappingConfigModelBuilder.HasKey(wpat => new {wpat.ItemAttributeVarietyId, wpat.WooProductAttributeTermId });
            //itemModelBuilder.Property(i => i.IsEnabled)
            //    .HasDefaultValue(true);
        }
    }
}