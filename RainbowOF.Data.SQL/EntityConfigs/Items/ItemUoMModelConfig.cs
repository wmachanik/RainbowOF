using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Lookups;

namespace RainbowOF.Datsa.SQL.EntityConfigs.Items
{
    internal class ItemUoMMappingConfig : IEntityTypeConfiguration<ItemUoMLookup>
    {
        public void Configure(EntityTypeBuilder<ItemUoMLookup> itemUoMModelBuilder)
        {
            itemUoMModelBuilder.HasIndex(iu => iu.UoMName)
                .IsUnique();
            itemUoMModelBuilder.HasIndex(iu => iu.UoMName)
                .IsUnique();
            //itemModelBuilder.Property(i => i.IsEnabled)
            //    .HasDefaultValue(true);
            itemUoMModelBuilder.HasOne(iu => iu.BaseUoM)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}