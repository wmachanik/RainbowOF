using RainbowOF.Models.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RainbowOF.Models.Woo
{
    // Maps the ItemCategory to wooItemcategory
    class WooCategoryMap
    {
        public int WooCategoryMapID { get; set; }
        [Required]
        public int WooCategoryID { get; set; }
        [StringLength(255, MinimumLength = 1)]
        public string WooCategoryName { get; set; }
        [StringLength(255, MinimumLength = 1)]
        public string WooCategorySlug { get; set; }
        public int? WooCategoryParentID { get; set; }
        // what it  links to on this side.
        public int ItemCategoryID {get;set;}

        public ItemCategory ItemCategory { get; set; }
    }
}
