using RainbowOF.Models.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RainbowOF.Models.Woo
{
    // Maps the ItemCategory to wooItemcategory
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
        public Guid ItemCategoryId {get;set;}

        public ItemCategory ItemCategory { get; set; }
    }
}
