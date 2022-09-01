using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Lookups;

namespace RainbowOF.Data.SQL.EntityConfigs.Lookups
{
    internal class ItemAttributeLookupModelConfig : IEntityTypeConfiguration<ItemAttributeLookup>
    {
        public void Configure(EntityTypeBuilder<ItemAttributeLookup> itemAttributeModelBuilder)
        {
            itemAttributeModelBuilder.HasIndex(ia => ia.AttributeName)
                .IsUnique();
            itemAttributeModelBuilder.HasIndex(ia => new { ia.OrderBy, ia.AttributeName });   // so the orderby is quicker.
            //itemAttributeModelBuilder.HasIndex(i => i.SKU)
            //    
        }
    }
}
