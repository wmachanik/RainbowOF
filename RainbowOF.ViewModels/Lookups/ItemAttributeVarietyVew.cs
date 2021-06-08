using System;
using System.Collections.Generic;
using System.Text;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;

namespace RainbowOF.ViewModels.Lookups
{
    public class ItemAttributeVarietyLookupView : ItemAttributeVarietyLookup
    {
        // inherits all the item AttributeVariety Lookup values the ItemAttributeVarietyId used for reference
        public bool? CanUpdateWooMap { get; set; }
        public bool HasWooAttributeVarietyMap
        {
            get { return (CanUpdateWooMap != null); }
        }

        //public ItemDisplayColour ItemColour { get; set; }
    }
}
