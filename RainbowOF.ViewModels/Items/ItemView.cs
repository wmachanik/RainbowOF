using System;
using System.Collections.Generic;
using System.Text;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.ViewModels.Common;

namespace RainbowOF.ViewModels.Items

{
    public class ItemView : Item
    {
        // inherits all the item Attribute Lookup values the ItemAttributeId used for reference
        public bool? CanUpdateWooMap { get; set; }
        public bool HasWooAttributeMap
        {
            get { return (CanUpdateWooMap != null); }
        }
    }
}
