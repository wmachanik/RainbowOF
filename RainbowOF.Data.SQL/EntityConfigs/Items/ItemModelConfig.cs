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
            itemModelBuilder.Property(i => i.SKU).IsRequired(false);
            //itemModelBuilder.HasIndex(i => i.SKU).IsUnique(); -> can do this due to the fact that item can be nullable
            //    .IsUnique();
            //itemModelBuilder.Property(i => i.IsEnabled)
            //    .HasDefaultValue(true);
            itemModelBuilder.HasOne(i => i.ParentItem)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            itemModelBuilder.HasOne(i => i.ReplacementItem)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            // list mappings
            itemModelBuilder.HasMany(i => i.ItemCategories)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);
            itemModelBuilder.HasMany(i => i.ItemAttributes)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);
            itemModelBuilder.HasMany(i => i.ItemCategories)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}