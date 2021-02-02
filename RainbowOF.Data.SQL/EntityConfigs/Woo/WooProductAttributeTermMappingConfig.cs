using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;
using RainbowOF.Models.Woo;

namespace RainbowOF.Datsa.SQL.EntityConfigs.Items
{
    internal class WooProductAttributeTermMappingConfig : IEntityTypeConfiguration<WooProductAttributeTermMap>
    {
        public void Configure(EntityTypeBuilder<WooProductAttributeTermMap> WooProductAttributeTermMappingConfigModelBuilder)
        {
            WooProductAttributeTermMappingConfigModelBuilder.HasKey(wpat => wpat.WooProductAttributeTermMapId);
            WooProductAttributeTermMappingConfigModelBuilder.HasAlternateKey(wpat => new {wpat.ItemAttributeVarietyLookupId, wpat.WooProductAttributeTermId });
            //itemModelBuilder.Property(i => i.IsEnabled)
            //    .HasDefaultValue(true);
        }
    }
}