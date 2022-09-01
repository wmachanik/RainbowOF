using Microsoft.EntityFrameworkCore;
using RainbowOF.Models.Items;
using RainbowOF.Models.Logs;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;

namespace RainbowOF.Data.SQL
{
    public interface IApplicationDbContext 
    {
        DbSet<ClosureDate> ClosureDates { get; set; }
        DbSet<ItemAttribute> ItemAttributes { get; set; }
        DbSet<ItemAttributeLookup> ItemAttributesLookups { get; set; }
        DbSet<ItemAttributeVariety> ItemAttributeVarieties { get; set; }
        DbSet<ItemAttributeVarietyLookup> ItemAttributeVarietiesLookups { get; set; }
        DbSet<ItemCategory> ItemCategories { get; set; }
        DbSet<ItemCategoryLookup> ItemCategoriesLookups { get; set; }
        DbSet<ItemImage> ItemImages { get; set; }
        DbSet<Item> Items { get; set; }
        DbSet<ItemUoMLookup> ItemUoMLookups { get; set; }
        DbSet<ItemVariantAssociatedLookup> ItemVariantAssociatedLookups { get; set; }
        DbSet<ItemVariant> ItemVariants { get; set; }
        DbSet<SysPrefs> SysPrefs { get; set; }
        DbSet<WooCategoryMap> WooCategoryMaps { get; set; }
        DbSet<WooProductAttributeMap> WooProductAttributeMappings { get; set; }
        DbSet<WooProductAttributeTermMap> WooProductAttributeTermMappings { get; set; }
        DbSet<WooProductMap> WooProductMaps { get; set; }
        DbSet<WooProductVariantMap> WooProductVariantMaps { get; set; }
        DbSet<WooSettings> WooSettings { get; set; }
        DbSet<WooSyncLog> WooSyncLogs { get; set; }
    }
}