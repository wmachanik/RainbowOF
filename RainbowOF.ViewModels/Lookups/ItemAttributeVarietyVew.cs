using System;
using System.Collections.Generic;
using System.Text;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;

namespace RainbowOF.ViewModels.Lookups
{
    public class ItemAttributeLookupView : ItemAttributeLookup
    {
        // inherits all the item Attribute Lookup values the ItemAttributeId used for reference
        public bool? CanUpdateWooMap { get; set; }
        public bool HasWooAttributeMap
        {
            get { return (CanUpdateWooMap != null); }
        }
    }
}
