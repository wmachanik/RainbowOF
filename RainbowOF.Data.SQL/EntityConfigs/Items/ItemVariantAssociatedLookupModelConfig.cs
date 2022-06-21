using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Data.SQL.EntityConfigs.Items
{
    internal class ItemVariantAssociatedLookupModelConfig : IEntityTypeConfiguration<ItemVariantAssociatedLookup>
    {
        public void Configure(EntityTypeBuilder<ItemVariantAssociatedLookup> itemVariantAssociatedLookupModelBuilder)
        {
            itemVariantAssociatedLookupModelBuilder.HasIndex(ival => ival.AssociatedAttributeLookupId);
            itemVariantAssociatedLookupModelBuilder.HasOne(ival => ival.AssociatedAttributeLookup)
                .WithMany()
                .HasForeignKey(ival => ival.AssociatedAttributeLookupId)
                .OnDelete(DeleteBehavior.NoAction);
            //itemVariantAssociatedLookupModelBuilder.HasOne(ival => ival.AssociatedAttributeLookup)
            //    .WithMany()
            //    .OnDelete(DeleteBehavior.NoAction);
            itemVariantAssociatedLookupModelBuilder.HasOne(ival => ival.AssociatedAttributeVarietyLookup)
                .WithMany()
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

// not sure why this was created
//   FK_ItemVariantAssociatedLookups_ItemAttributeVarietiesLookups_AssociatedAttributeVarietyLookupId
