using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Integrations;
using RainbowOF.Repositories.Lookups;
using RainbowOF.ViewModels.Items;
using RainbowOF.ViewModels.Lookups;

namespace RainbowOF.Repositories.Items
{
    /// <summary>
    /// Inherits Interface for general model for grid CRUD linked to Woo
    /// </summary>
    public interface IItemWooLinkedView : IWooLinkedView<Item, ItemView, WooProductMap>
    {
        // nothing here inherit it all
    }
}

