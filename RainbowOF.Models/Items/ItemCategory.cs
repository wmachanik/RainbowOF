using RainbowOF.Models.Lookups;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Items
{
    public class ItemCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemCategoryId { get; set; }
        public Guid ItemId { get; set; }
        public Guid ItemCategoryLookupId { get; set; }
        public bool UsedForPrediction { get; set; }
        public Guid? UoMBaseId { get; set; }

        [ForeignKey("ItemCategoryLookupId")]
        public ItemCategoryLookup ItemCategoryDetail { get; set; }
        [ForeignKey("UoMBaseId")]
        public ItemUoMLookup ItemUoMBase { get; set; }
    }
}