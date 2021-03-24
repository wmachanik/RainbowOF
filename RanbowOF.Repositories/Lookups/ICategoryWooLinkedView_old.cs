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
    public interface ICategoryWooLinkedView_old
    {
        Task<List<ItemCategoryLookup>> GetAllItemsAsync();
        void PushSelectedItems(List<ItemCategoryLookupView> currentSelectedItems);
        List<ItemCategoryLookupView> PopSelectedItems(List<ItemCategoryLookupView> modelViewItems);
        Task<WooCategoryMap> GetWooMappedItemAsync(Guid mapWooEntityID);
        Task<List<ItemCategoryLookupView>> LoadAllViewItems();
        Task InsertRowAsync(ItemCategoryLookupView newVeiwEntity);
        void NewItemDefaultSetter(ItemCategoryLookupView newViewEntity);
        Task<int> UpdateWooMappingAsync(ItemCategoryLookupView updateVeiwEntity);
        Task<bool> IsDuplicate(ItemCategoryLookup checkEntity);
        bool IsValid(ItemCategoryLookup checkEntity);
        ItemCategoryLookup GetItemFromView(ItemCategoryLookupView fromVeiwEntity);
        Task<int> UpdateItemAsync(ItemCategoryLookup updateItem);
        Task UpdateRowAsync(ItemCategoryLookupView updateVeiwEntity);
        Task DeleteEntityAsync(ItemCategoryLookupView deleteVeiwEntity);
        Task<int> DoGroupActionAsync(ItemCategoryLookupView toVeiwEntity, BulkAction selectedAction);
        Task DeleteWooItemAsync(Guid itemCategoryLookupId);
        Task AddWooItemAsync(ItemCategoryLookupView selectedItemCategoryLookup);
    }
}

