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
using RainbowOF.Woo.REST.Models;
using RainbowOF.Tools.Services;
using Blazorise.DataGrid;

namespace RainbowOF.Repositories.Integrations
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
        public WooLinkedGridSettings _WooLinkedGridSettings { get; set; }
        Task<WooAPISettings> GetWooAPISettingsAsync();
        Task<bool> WooIsActiveAsync(ApplicationState currentApplicationState); 
        void PushSelectedItems(List<TEntityView> currentSelectedItems);
        List<TEntityView> PopSelectedItems(List<TEntityView> modelViewItems);
        Task<List<TWooMapEntity>> GetWooMappedItemsAsync(List<Guid> mapWooEntityID);
        DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<TEntityView> inputDataGridReadData, string inputCustomFilter);
        TEntity GetItemFromView(TEntityView fromVeiwEntity);
        Task<List<TEntityView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters);
        TEntityView NewItemDefaultSetter(TEntityView newViewEntity);
        Task<bool> IsDuplicateAsync(TEntity checkEntity);
        bool IsValid(TEntity checkEntity);
        Task<int> DoGroupActionAsync(TEntityView toVeiwEntity, BulkAction selectedAction);
        Task<TWooMapEntity> AddWooItemAndMapAsync(TEntity addEntity);
        Task InsertRowAsync(TEntityView newVeiwEntity);
        Task<int> DeleteWooItemAsync(Guid deleteWooEntityId, bool deleteFromWoo);
        Task DeleteRowAsync(TEntityView deleteViewEntity);
        Task<int> UpdateWooMappingAsync(TEntityView updateViewEntity);
        Task<int> UpdateWooItemAsync(TEntityView updateViewEntity);
        Task<int> UpdateWooItemAndMappingAsync(TEntityView updateViewEntity);
        Task<int> UpdateItemAsync(TEntityView addEntity);    // (TEntityView updateItem);
        Task UpdateRowAsync(TEntityView updateViewEntity);
    }
}

