using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RainbowOF.Models.Woo
{
    // Maps the ItemCategoryLookup to wooItemCategoryLookup
    public class WooCategoryMap
    {
        public int WooCategoryMapId { get; set; }
        [Required]
        public int WooCategoryId { get; set; }
        [StringLength(255, MinimumLength = 1)]
        public string WooCategoryName { get; set; }
        [StringLength(255, MinimumLength = 1)]
        public string WooCategorySlug { get; set; }
        public int? WooCategoryParentId { get; set; }
        // what it  links to on this side.
        [DefaultValue(true)]
        [DisplayName("Can Update?")]
        public bool CanUpdate { get; set; }
        public Guid ItemCategoryLookupId {get;set;}

        public ItemCategoryLookup ItemCategoryLookup { get; set; }
    }
}
