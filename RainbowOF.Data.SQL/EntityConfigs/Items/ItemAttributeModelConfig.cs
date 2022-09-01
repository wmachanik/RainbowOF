using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;

namespace RainbowOF.Data.SQL.EntityConfigs.Items
{
    internal class ItemAttributeModelConfig : IEntityTypeConfiguration<ItemAttribute>
    {
        public void Configure(EntityTypeBuilder<ItemAttribute> itemAttributeModelBuilder)
        {
            itemAttributeModelBuilder.HasIndex(aia => aia.ItemAttributeId)
                .IsUnique();
            itemAttributeModelBuilder.HasOne(aia => aia.ItemAttributeDetail)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            itemAttributeModelBuilder.HasMany(aia => aia.ItemAttributeVarieties)
                .WithOne()
                .OnDelete(DeleteBehavior.ClientCascade);
            //itemAttributeModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
