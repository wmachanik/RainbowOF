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
    /// Interface used handle the general model for grid CRUD where the Entity has a possible Woo link
    /// </summary>
    /// <typeparam name="TEntity">The model / class of the specific item being displayed</typeparam>
    /// <typeparam name="TEntityView">The model version of the class, this should include woo variations and any specifics to viewing the class </typeparam>
    /// <typeparam name="TWooMapEntity">The Woo Mapping class that links the systems item to the woo item</typeparam>

    public interface IWooLinkedView<TEntity, TEntityView, TWooMapEntity> where TEntity : class
                                                                         where TEntityView : class
                                                                         where TWooMapEntity : class
    {
        Task<List<TEntity>> GetAllItemsAsync();
        void PushSelectedItems(List<TEntityView> currentSelectedItems);
        List<TEntityView> PopSelectedItems(List<TEntityView> modelViewItems);
        Task<TWooMapEntity> GetWooMappedItemAsync(Guid mapWooEntityID);
        Task<List<TEntityView>> LoadAllViewItems();
        Task InsertRowAsync(TEntityView newVeiwEntity);
        void NewItemDefaultSetter(TEntityView newViewEntity);
        Task<int> UpdateWooMappingAsync(TEntityView updateVeiwEntity);
        Task<bool> IsDuplicate(TEntity checkEntity);
        bool IsValid(TEntity checkEntity);
        TEntity GetItemFromView(TEntityView fromVeiwEntity);
        Task<int> UpdateItemAsync(TEntity updateItem);
        Task UpdateRowAsync(TEntityView updateVeiwEntity);
        Task DeleteRowAsync(TEntityView deleteVeiwEntity);
        Task<int> DoGroupActionAsync(TEntityView toVeiwEntity, BulkAction selectedAction);
        Task<int> DeleteWooItemAsync(Guid deleteWooEntityId);
        Task<int> AddWooItemAsync(TEntity addEntity);
        Task<int> UpdateWooItemAsync(TEntityView updateViewEntity, TWooMapEntity updateWooMapEntity);
    }
}

