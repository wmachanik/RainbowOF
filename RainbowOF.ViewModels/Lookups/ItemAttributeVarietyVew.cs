using RainbowOF.Models.Lookups;

namespace RainbowOF.ViewModels.Lookups
{
    public class ItemAttributeVarietyLookupView : ItemAttributeVarietyLookup
    {
        // inherits all the item AttributeVariety Lookup values the ItemAttributeVarietyId used for reference
        public bool? CanUpdateECommerceMap { get; set; }
        public bool HasECommerceAttributeVarietyMap
        {
            get { return (CanUpdateECommerceMap != null); }
        }

        //public ItemDisplayColour ItemColour { get; set; }
    }
}
