using RainbowOF.Models.Items;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Integrations;
using RainbowOF.ViewModels.Items;

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

