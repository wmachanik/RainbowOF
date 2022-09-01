using RainbowOF.Models.Lookups;

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
