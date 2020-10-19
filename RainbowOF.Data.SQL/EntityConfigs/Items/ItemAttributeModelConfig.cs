using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Data.SQL.EntityConfigs.Items
{
    internal class ItemAttributeModelConfig : IEntityTypeConfiguration<ItemAttribute>
    {
        public void Configure(EntityTypeBuilder<ItemAttribute> itemAttributeModelBuilder)
        {
            itemAttributeModelBuilder.HasIndex(ia => ia.AttributeName)
                .IsUnique();
            //itemAttributeModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
