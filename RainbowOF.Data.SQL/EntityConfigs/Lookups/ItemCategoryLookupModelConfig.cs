using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Lookups;
using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Data.SQL.EntityConfigs.Lookups
{
    internal class ItemCategoryLookupModelConfig : IEntityTypeConfiguration<ItemCategoryLookup>
    {
        public void Configure(EntityTypeBuilder<ItemCategoryLookup> ItemCategoryLookupModelBuilder)
        {
            ItemCategoryLookupModelBuilder.HasIndex(ic => ic.CategoryName)
                .IsUnique();
            ItemCategoryLookupModelBuilder.HasOne(ic => ic.ParentCategory)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            //ItemCategoryLookupModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
