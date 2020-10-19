using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Data.SQL.EntityConfigs.Items
{
    internal class ActiveItemAttributeModelConfig : IEntityTypeConfiguration<ActiveItemAttribute>
    {
        public void Configure(EntityTypeBuilder<ActiveItemAttribute> activeItemAttributeModelBuilder)
        {
            activeItemAttributeModelBuilder.HasIndex(aia => new { aia.ItemAttributeId, aia.ItemId })
                .IsUnique();
            //itemAttributeModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
