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
            itemAttributeModelBuilder.HasIndex(aia => aia.ItemAttributeId)
                .IsUnique();
            itemAttributeModelBuilder.HasOne(aia => aia.ItemAttributeDetail)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            itemAttributeModelBuilder.HasMany(aia => aia.ItemAttributeVarieties)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);
            //itemAttributeModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
