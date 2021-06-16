using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Data.SQL.EntityConfigs.Items
{
    internal class ItemAttributeVarietyModelConfig : IEntityTypeConfiguration<ItemAttributeVariety>
    {
        public void Configure(EntityTypeBuilder<ItemAttributeVariety> activeItemAttributeVarietyModelBuilder)
        {
            //activeItemAttributeVarietyModelBuilder.HasIndex(aiav => aiav.ItemAttributeVarietyId )
            //    .IsUnique();
            //activeItemAttributeVarietyModelBuilder.HasIndex(aiav => new { aiav.ItemAttributeId, aiav.ItemAttributeVarietyLookupId })
            //    .IsUnique();
            activeItemAttributeVarietyModelBuilder.HasOne(aiav => aiav.ItemAttributeVarietyLookupDetail)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            activeItemAttributeVarietyModelBuilder.HasOne(aiav => aiav.UoM)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
