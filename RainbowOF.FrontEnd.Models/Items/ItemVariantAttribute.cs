using RainbowOF.Models.Items;
using System;

namespace RainbowOF.Web.FrontEnd.Models.Items
{
    public class ItemVariantView
    {
        public ItemVariant ItemVariant { get; set; }
        public bool IsModified { get; set; } = false;
        public bool TabIsExpanded { get; set; } = false;
    }
}