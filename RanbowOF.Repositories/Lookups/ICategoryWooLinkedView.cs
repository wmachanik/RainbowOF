using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Integrations;
using RainbowOF.ViewModels.Lookups;

namespace RainbowOF.Repositories.Lookups
{
    /// <summary>
    /// Inherits Interface for general model for grid CRUD linked to Woo
    /// </summary>
    public interface ICategoryWooLinkedView : IWooLinkedView<ItemCategoryLookup, ItemCategoryLookupView, WooCategoryMap>
    {
        // here we can add any additional non generic Interfaces specific to Item Categories that are no covered in the generic view definition
    }
}
