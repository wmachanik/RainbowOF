﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;
using RainbowOF.Models.Woo;

namespace RainbowOF.Datsa.SQL.EntityConfigs.Items
{
    internal class WooProductAttributeMappingModelConfig : IEntityTypeConfiguration<WooProductAttributeMap>
    {
        public void Configure(EntityTypeBuilder<WooProductAttributeMap> WooProductAttributeMappingConfigModelBuilder)
        {
            WooProductAttributeMappingConfigModelBuilder.HasKey(wpat => wpat.WooProductAttributeMapId );
            WooProductAttributeMappingConfigModelBuilder.HasAlternateKey(wpat => new { wpat.ItemAttributeLookupId, wpat.WooProductAttributeId });
            //itemModelBuilder.Property(i => i.IsEnabled)
            //    .HasDefaultValue(true);
        }
    }
}