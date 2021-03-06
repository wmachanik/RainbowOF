﻿using System;
using System.Collections.Generic;
using System.Text;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;

namespace RainbowOF.ViewModels.Lookups
{
    public class ItemCategoryLookupView : ItemCategoryLookup
    {
        // inherits all the item Category Lookup values the ItemCategoryId used for reference
        public bool? CanUpdateECommerceMap { get; set; }
        public bool HasECommerceCategoryMap
        {
            get { return (CanUpdateECommerceMap != null); }
        }
    }
}
