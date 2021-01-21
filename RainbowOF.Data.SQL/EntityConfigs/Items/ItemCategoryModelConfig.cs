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
            itemCategoryModelBuilder.HasIndex(ic => ic.ItemCategoryName)
                .IsUnique();
            itemCategoryModelBuilder.HasOne(ic => ic.ParentCategory)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            //itemCategoryModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
