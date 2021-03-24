using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.ViewModels.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Lookups
{
    public interface ICategoryWooLinkedView : IWooLinkedView<ItemCategoryLookup, ItemCategoryLookupView, WooCategoryMap>
    {
        // here we can add any additional non generic Interfaces specific to Item Categories that are no covered in the generic view definition
    }
}
