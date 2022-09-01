using RainbowOF.Models.Lookups;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RainbowOF.Models.Woo
{
    // Maps the ItemCategoryLookup to wooItemCategoryLookup
    public class WooCategoryMap
    {
        public int WooCategoryMapId { get; set; }
        [Required]
        public uint WooCategoryId { get; set; }
        [StringLength(255, MinimumLength = 1)]
        public string WooCategoryName { get; set; }
        [StringLength(255, MinimumLength = 1)]
        public string WooCategorySlug { get; set; }
        public uint? WooCategoryParentId { get; set; }
        // what it  links to on this side.
        [DefaultValue(true)]
        [DisplayName("Can Update?")]
        public bool CanUpdate { get; set; }
        public Guid ItemCategoryLookupId { get; set; }

        public ItemCategoryLookup ItemCategoryLookup { get; set; }
    }
}
