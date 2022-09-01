using AutoMapper;
using Blazorise.DataGrid;
using RainbowOF.Models.System;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.Tools.Services;
using RainbowOF.ViewModels.Common;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Integrations
{
    public class WooLinkedView<TEntity, TEntityView, TWooMapEntity> : IWooLinkedView<TEntity, TEntityView, TWooMapEntity>
                        where TEntity : class
                        where TEntityView : class
                        where TWooMapEntity : class
    {
        public ILoggerManager AppLoggerManager { get; set; }
        public IUnitOfWork AppUnitOfWork { get; set; }
        public WooLinkedGridSettings CurrWooLinkedGridSettings { get; set; } = new();
        public IMapper AppMapper { get; set; }

        public WooLinkedView(ILoggerManager sourceLogger,
                             IUnitOfWork sourceAppUnitOfWork,
                             //GridSettings sourceGridSettings,
                             IMapper sourceMapper)
        {
            AppLoggerManager = sourceLogger;
            AppUnitOfWork = sourceAppUnitOfWork;
            //_WooLinkedGridSettings = sourceGridSettings;
            AppMapper = sourceMapper;
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"WooLinkedView with entity type {typeof(TEntity).Name}, EntityView type {typeof(TEntityView).Name} and WooMapEntity type {typeof(TWooMapEntity).Name} initialised.");
        }

        public async Task<WooAPISettings> GetWooAPISettingsAsync()
        {
            IRepository<WooSettings> _WooPrefs = AppUnitOfWork.Repository<WooSettings>();

            WooSettings _wooSettings = await _WooPrefs.FindFirstAsync();
            if (_wooSettings == null)
                return null;
            return new WooAPISettings(_wooSettings);
        }

        public virtual async Task<bool> WooIsActiveAsync(ApplicationState currentApplicatioonState)
        {
            if (!currentApplicatioonState.HaveCheckState)
            {
                try
                {
                    WooAPISettings _wooAPISettings = await GetWooAPISettingsAsync();
                    if (_wooAPISettings == null)
                        currentApplicatioonState.SetWooIsActive(false);
                    else
                    {
                        WooProductCategory _wooProductCategory = new(_wooAPISettings, AppLoggerManager);
                        if (_wooProductCategory == null)
                            currentApplicatioonState.SetWooIsActive(false);
                        else
                            currentApplicatioonState.SetWooIsActive(await _wooProductCategory.CheckProductCategoryLinkAsync());
                    }
                }
                catch (Exception ex)
                {
                    AppLoggerManager.LogError($"Error running async tasks: {ex.Message}");
                    throw;
                }
            }

            return currentApplicatioonState.WooIsActive;
        }

        public virtual Task<TWooMapEntity> AddWooItemAndMapAsync(TEntity addEntity)
        {
            AppLoggerManager.LogError($"AddWooItemAndMapAsync for Entity: {addEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task DeleteRowAsync(TEntityView deleteViewEntity)
        {
            AppLoggerManager.LogError($"DeleteRowAsync for Entity: {deleteViewEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> DeleteWooItemAsync(Guid deleteWooEntityId, bool deleteFromWoo)
        {
            AppLoggerManager.LogError($"DeleteWooItemAsync for Entity: {deleteWooEntityId} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> DoGroupActionAsync(TEntityView toVeiwEntity, BulkAction selectedAction)
        {
            AppLoggerManager.LogError($"DoGroupActionAsync for Entity: {toVeiwEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntity>> GetAllItemsAsync()
        {
            AppLoggerManager.LogError($"GetAllItemsAsync for Entity not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual TEntity GetItemFromView(TEntityView fromVeiwEntity)
        {
            AppLoggerManager.LogError($"GetItemFromView for Entity: {fromVeiwEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<TWooMapEntity> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            AppLoggerManager.LogError($"GetWooMappedItemAsync for Entity: {mapWooEntityID} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }
        public virtual Task<List<TWooMapEntity>> GetWooMappedItemsAsync(List<Guid> mapWooEntityIDs)
        {
            AppLoggerManager.LogError($"GetWooMappedItemsAsync for Entity: {mapWooEntityIDs} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task InsertRowAsync(TEntityView newVeiwEntity)
        {
            AppLoggerManager.LogError($"InsertRowAsync for Entity: {newVeiwEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<bool> IsDuplicateAsync(TEntity checkEntity)
        {
            AppLoggerManager.LogError($"IsDuplicate for Entity: {checkEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual bool IsValid(TEntity checkEntity)
        {
            AppLoggerManager.LogError($"IsValid for Entity: {checkEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntityView>> LoadAllViewItemsAsync()
        {
            AppLoggerManager.LogError($"LoadAllViewItemsAsync for Entity not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual TEntityView NewItemDefaultSetter(TEntityView newViewEntity)
        {
            AppLoggerManager.LogError($"NewItemDefaultSetter for Entity: {newViewEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual List<TEntityView> PopSelectedItems(List<TEntityView> modelViewItems)
        {
            AppLoggerManager.LogError($"PopSelectedItems for Entity: {modelViewItems} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual void PushSelectedItems(List<TEntityView> currentSelectedItems)
        {
            AppLoggerManager.LogError($"PushSelectedItems for Entity: {currentSelectedItems} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateItemAsync(TEntityView updateItemView)
        {
            AppLoggerManager.LogError($"UpdateItemAsync for Entity: {updateItemView} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task UpdateRowAsync(TEntityView updateViewEntity)
        {
            AppLoggerManager.LogError($"UpdateRowAsync for Entity: {updateViewEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooItemAndMappingAsync(TEntityView updateViewEntity)
        {
            AppLoggerManager.LogError($"UpdateWooItemAndMapping for Entity: {updateViewEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooItemAsync(TEntityView updateViewEntity)
        {
            AppLoggerManager.LogError($"UpdateWooItemAsync for Entity: {updateViewEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooMappingAsync(TEntityView updateViewEntity)
        {
            AppLoggerManager.LogError($"UpdateWooMappingAsync for Entity: {updateViewEntity} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntity>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters) // int startPage, int currentPageSize)
        {
            AppLoggerManager.LogError($"GetPagedItemsAsync for Entity: {currentDataGridParameters} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<TEntityView> inputDataGridReadData, string inputCustomerFilter)
        {
            AppLoggerManager.LogError($"GetDataGridCurrent for Entity: {inputDataGridReadData} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }
        public virtual Task<List<TEntityView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters)
        {
            AppLoggerManager.LogError($"LoadViewItemsPaginatedAsync for Entity: {currentDataGridParameters} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }
    }
}
