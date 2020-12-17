using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Data.SQL.EntityConfigs.Items
{
    internal class ActiveItemAttributeVarietyModelConfig : IEntityTypeConfiguration<ActiveItemAttributeVariety>
    {
        public void Configure(EntityTypeBuilder<ActiveItemAttributeVariety> activeItemAttributeVarietyModelBuilder)
        {
            activeItemAttributeVarietyModelBuilder.HasIndex(aiav => new { aiav.ItemId, aiav.ActiveItemAttributeVarietyId })
                .IsUnique();
            activeItemAttributeVarietyModelBuilder.HasOne(aiav => aiav.ItemUoM)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
