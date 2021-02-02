using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Data.SQL.EntityConfigs.Items
{
    internal class ItemCategoryModelConfig : IEntityTypeConfiguration<ItemCategory>
    {
        public void Configure(EntityTypeBuilder<ItemCategory> itemCategoryModelBuilder)
        {
            itemCategoryModelBuilder.HasIndex(aia => new { aia.ItemCategoryId, aia.ItemId })
                .IsUnique();
            //itemCategoryModelBuilder.HasOne(ia => ia.ItemAttributeDetail)
            //    .WithMany()
            //    .OnDelete(DeleteBehavior.NoAction);
            //itemAttributeModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
