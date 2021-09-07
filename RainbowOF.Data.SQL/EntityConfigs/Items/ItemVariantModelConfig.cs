using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Data.SQL.EntityConfigs.Items
{
    internal class ItemVariantModelConfig : IEntityTypeConfiguration<ItemVariant>
    {
        public void Configure(EntityTypeBuilder<ItemVariant> itemVariantModelBuilder)
        {
            itemVariantModelBuilder.HasIndex(iv => new { iv.ItemVariantId, iv.ItemId })
                .IsUnique();
///--> this keeps creating a new column
            //itemVariantModelBuilder.HasOne(iv => iv.Item)
            //    .WithMany()
            //    .HasForeignKey(i => i.ItemId)
            //    .OnDelete(DeleteBehavior.NoAction);
            //itemAttributeModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
