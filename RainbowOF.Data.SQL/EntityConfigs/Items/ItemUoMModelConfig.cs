using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;

namespace RainbowOF.Datsa.SQL.EntityConfigs.Items
{
    internal class ItemUoMMappingConfig : IEntityTypeConfiguration<ItemUoM>
    {
        public void Configure(EntityTypeBuilder<ItemUoM> itemUoMModelBuilder)
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