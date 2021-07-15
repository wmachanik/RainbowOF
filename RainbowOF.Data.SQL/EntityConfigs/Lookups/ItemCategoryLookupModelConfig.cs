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
            ItemCategoryLookupModelBuilder.HasIndex(icl => icl.CategoryName)
                .IsUnique();
            ItemCategoryLookupModelBuilder.HasOne(icl => icl.ParentCategory)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            //ItemCategoryLookupModelBuilder.Property(nameof(ItemCategoryLookup.FullCategoryName))
            //    .HasComputedColumnSql("");
                //ItemCategoryLookupModelBuilder.HasMany
            //    .HasMany(icl => icl.ChildItemCategories)
            //    .WithOne(icp => icp.ParentCategoryId)
            //    .OnDelete(DeleteBehavior.ClientCascade);
            //ItemCategoryLookupModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
