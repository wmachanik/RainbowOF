using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Lookups;

namespace RainbowOF.Data.SQL.EntityConfigs.Lookups
{
    internal class ItemAttributeVarietyLookupModelConfig : IEntityTypeConfiguration<ItemAttributeVarietyLookup>
    {
        public void Configure(EntityTypeBuilder<ItemAttributeVarietyLookup> ItemAttributeVarietyModelBuilder)
        {
            ItemAttributeVarietyModelBuilder.Property(iav => iav.ItemAttributeLookupId)
                .IsRequired();
            ItemAttributeVarietyModelBuilder.HasIndex(iav => new { iav.VarietyName, iav.ItemAttributeLookupId })
                .IsUnique();
            ItemAttributeVarietyModelBuilder.HasIndex(iav => new { iav.VarietyName, iav.SortOrder});

            //ItemAttributeVarietyModelBuilder.HasOne(i=>i.ReplacementItem)
            //    .WithMany()
            //    .OnDelete(DeleteBehavior.SetNull);
        }
    }
}