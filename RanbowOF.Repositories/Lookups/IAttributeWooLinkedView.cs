using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Models.Woo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RainbowOF.ViewModels.Lookups;
using RainbowOF.ViewModels.Common;

namespace RainbowOF.Repositories.Lookups
{
    /// <summary>
    /// Interface that copies the a general model for grid CRUD
    /// </summary>
    public interface IAttributeWooLinkedView : IWooLinkedView<ItemAttributeLookup, ItemAttributeLookup, WooProductAttributeMap>
    {
        //Task<List<ItemAttributeLookup>> GetAllItemsAsync();
        //Task<WooProductAttributeMap> GetWooMappedItemAsync(Guid mapWooEntityID);
        //Task<List<ItemAttributeLookupView>> LoadAllViewItems();
        //Task InsertRowAsync(ItemAttributeLookupView newVeiwEntity);
        //void NewItemDefaultSetterAsync(ItemAttributeLookupView newVeiwEntity);
        //Task<int> UpdateWooMappingAsync(ItemAttributeLookupView updateVeiwEntity);
        //Task<bool> IsDuplicate(ItemCategoryLookup checkEntity);
        //bool IsValid(ItemAttributeLookup checkEntity);
        //ItemAttributeLookup GetItemFromView(ItemAttributeLookupView fromVeiwEntity);
        //Task<int> UpdateItemAsync(ItemAttributeLookup updateItem);
        //Task UpdateRowAsync(ItemAttributeLookupView updateVeiwEntity);
        //Task ConfirmDeleteAsync(ItemAttributeLookupView deleteVeiwEntity);
        //Task<int> DoGroupActionAsync(ItemAttributeLookupView toVeiwEntity, BulkAction selectedAction);
    }
}

