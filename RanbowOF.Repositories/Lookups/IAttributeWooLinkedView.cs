using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Integrations;
using RainbowOF.ViewModels.Lookups;

namespace RainbowOF.Repositories.Lookups
{
    /// <summary>
    /// Inherits Interface for general model for grid CRUD linked to Woo
    /// </summary>
    public interface IItemWooLinkedView : IWooLinkedView<ItemAttributeLookup, ItemAttributeLookupView, WooProductAttributeMap>
    {
        // nothing here inherit it all
    }
}

