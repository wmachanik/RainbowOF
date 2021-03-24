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
    /// Interface that uses the general model for grid CRUD to implement the attribute variety
    /// </summary>
    public interface IAttributeVarietyWooLinkedView : IWooLinkedView<ItemAttributeVarietyLookup, ItemAttributeVarietyLookupView, WooProductAttributeTermMap>
    {
        //Task<List<ItemAttributeVarietyLookup>> GetAllItemsAsync();
        //Task<WooProductAttributeTermMap> GetWooMappedItemAsync(Guid mapWooEntityID);
        //Task<List<ItemAttributeVarietyLookupView>> LoadAllViewItems();
        //Task InsertRowAsync(ItemAttributeVarietyLookupView newVeiwEntity);
        //void NewItemDefaultSetterAsync(ItemAttributeVarietyLookupView newVeiwEntity);
        //Task<int> UpdateWooMappingAsync(ItemAttributeVarietyLookupView updateVeiwEntity);
        //Task<bool> IsDuplicate(ItemAttributeVarietyLookup checkEntity);
        //bool IsValid(ItemAttributeVarietyLookup checkEntity);
        //ItemAttributeVarietyLookup GetItemFromView(ItemAttributeVarietyLookupView fromVeiwEntity);
        //Task<int> UpdateItemAsync(ItemAttributeVarietyLookup updateEntity);
        //Task UpdateRowAsync(ItemAttributeVarietyLookupView updateVeiwEntity);
        //Task DeleteEntityAsync(ItemAttributeVarietyLookupView deleteVeiwEntity);
        //Task<int> DoGroupActionAsync(ItemAttributeVarietyLookupView toVeiwEntity, BulkAction selectedAction);
    }
}

