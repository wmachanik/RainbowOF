using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;

namespace RainbowOF.Datsa.SQL.EntityConfigs.Items
{
    internal class ItemAttributeVarietyModelConfig : IEntityTypeConfiguration<ItemAttributeVariety>
    {
        public void Configure(EntityTypeBuilder<ItemAttributeVariety> ItemAttributeVarietyModelBuilder)
        {
            ItemAttributeVarietyModelBuilder.Property(iav => iav.ItemAttributeId)
                .IsRequired();
            ItemAttributeVarietyModelBuilder.HasIndex(iav => new { iav.VarietyName, iav.ItemAttributeId } )
                .IsUnique();

            //ItemAttributeVarietyModelBuilder.HasOne(i=>i.ReplacementItem)
            //    .WithMany()
            //    .OnDelete(DeleteBehavior.SetNull);
        }
    }
}