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
            itemAttributeModelBuilder.HasIndex(aia => new { aia.ItemAttributeId, aia.ItemId })
                .IsUnique();
            itemAttributeModelBuilder.HasOne(ia => ia.ItemAttributeDetail)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            //itemAttributeModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
