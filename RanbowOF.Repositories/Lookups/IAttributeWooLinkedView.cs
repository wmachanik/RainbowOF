using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.ViewModels.Lookups;

namespace RainbowOF.Repositories.Lookups
{
    /// <summary>
    /// Inherits Interface for general model for grid CRUD linked to Woo
    /// </summary>
    public interface ItemWooLinkedView : IWooLinkedView<ItemAttributeLookup, ItemAttributeLookupView, WooProductAttributeMap>
    {
        // nothing here inherit it all
    }
}

