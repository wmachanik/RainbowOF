using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Woo;

namespace RainbowOF.Datsa.SQL.EntityConfigs.Items
{
    internal class WooProductAttributeTermMappingModelConfig : IEntityTypeConfiguration<WooProductAttributeTermMap>
    {
        public void Configure(EntityTypeBuilder<WooProductAttributeTermMap> WooProductAttributeTermMappingConfigModelBuilder)
        {
            WooProductAttributeTermMappingConfigModelBuilder.HasKey(wpat => wpat.WooProductAttributeTermMapId);
            //--> this causes issues if the terms change names to similar names
            //WooProductAttributeTermMappingConfigModelBuilder.HasAlternateKey(wpat => new {wpat.ItemAttributeVarietyLookupId, wpat.WooProductAttributeTermId });
            //itemModelBuilder.Property(i => i.IsEnabled)
            //    .HasDefaultValue(true);
        }
    }
}