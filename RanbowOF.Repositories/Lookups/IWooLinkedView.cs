﻿using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Models.Woo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RainbowOF.ViewModels.Lookups;
using RainbowOF.ViewModels.Common;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Tools.Services;
using Blazorise.DataGrid;

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
        // Replaced by GetDataGridCurrent - Task<List<TEntity>> GetAllItemsAsync();
        Task<WooAPISettings> GetWooAPISettingsAsync();
        Task<bool> WooIsActive(ApplicationState currentApplicationState); 
        void PushSelectedItems(List<TEntityView> currentSelectedItems);
        List<TEntityView> PopSelectedItems(List<TEntityView> modelViewItems);
        //--- replaced by: GetWooMappedItemsAsync Task<TWooMapEntity> GetWooMappedItemAsync(Guid mapWooEntityID);
        Task<List<TWooMapEntity>> GetWooMappedItemsAsync(List<Guid> mapWooEntityID);
        // Only used for in memory Task<List<TEntityView>> LoadAllViewItemsAsync();
        // REplaced by GetDataGridCurrent - Task<List<TEntity>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters); // int startPage, int currentPageSize);
        DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<TEntityView> inputDataGridReadData, string inputCustomFilter);
        Task<List<TEntityView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters);
        Task InsertRowAsync(TEntityView newVeiwEntity);
        TEntityView NewItemDefaultSetter(TEntityView newViewEntity);
        Task<int> UpdateWooMappingAsync(TEntityView updateViewEntity);
        Task<bool> IsDuplicate(TEntity checkEntity);
        bool IsValid(TEntity checkEntity);
        TEntity GetItemFromView(TEntityView fromVeiwEntity);
        Task DeleteRowAsync(TEntityView deleteViewEntity);
        Task<int> DoGroupActionAsync(TEntityView toVeiwEntity, BulkAction selectedAction);
        Task<int> DeleteWooItemAsync(Guid deleteWooEntityId, bool deleteFromWoo);
        Task<int> AddWooItemAndMapAsync(TEntity addEntity);
        Task<int> UpdateWooItemAsync(TEntityView updateViewEntity);
        Task<int> UpdateWooItemAndMapping(TEntityView updateViewEntity);
        Task<int> UpdateItemAsync(TEntityView updateItem);
        Task UpdateRowAsync(TEntityView updateViewEntity);
    }
}

