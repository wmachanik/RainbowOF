using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;

namespace RainbowOF.Datsa.SQL.EntityConfigs.Items
{
    internal class ItemModelConfig : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> itemModelBuilder)
        {
            itemModelBuilder.HasIndex(i => i.ItemName)
                .IsUnique();
            itemModelBuilder.HasIndex(i => i.SKU)
                .IsUnique();
            //itemModelBuilder.Property(i => i.IsEnabled)
            //    .HasDefaultValue(true);
            itemModelBuilder.HasOne(i => i.ItemCategory)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
            itemModelBuilder.HasOne(i => i.ParentItem)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            itemModelBuilder.HasOne(i=>i.ReplacementItem)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}