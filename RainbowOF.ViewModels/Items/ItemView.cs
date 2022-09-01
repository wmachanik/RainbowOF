using RainbowOF.Models.Items;

namespace RainbowOF.ViewModels.Items

{
    public class ItemView : Item
    {
        // inherits all the item Attribute Lookup values the ItemAttributeId used for reference
        public bool? CanUpdateECommerceMap { get; set; }
        public bool HasECommerceAttributeMap
        {
            get { return (CanUpdateECommerceMap != null); }
        }
        /// public Item SourceItem { get; set; } rather inherit other wise have issues with grid
    }
}
