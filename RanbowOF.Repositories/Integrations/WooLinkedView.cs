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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Integrations
{
    public class WooLinkedView<TEntity, TEntityView, TWooMapEntity> : IWooLinkedView<TEntity, TEntityView, TWooMapEntity>
                        where TEntity : class
                        where TEntityView : class
                        where TWooMapEntity : class
    {
        public ILoggerManager appLoggerManager { get; set; }
        public IUnitOfWork appUnitOfWork { get; set; }
        public WooLinkedGridSettings _WooLinkedGridSettings { get; set; } = new();
        public IMapper _Mapper { get; set; }

        public WooLinkedView(ILoggerManager sourceLogger,
                             IUnitOfWork sourceAppUnitOfWork,
                              //GridSettings sourceGridSettings,
                             IMapper sourceMapper)
        {
            appLoggerManager = sourceLogger;
            appUnitOfWork = sourceAppUnitOfWork;
            //_WooLinkedGridSettings = sourceGridSettings;
            _Mapper = sourceMapper;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"WooLinkedView with entity type {typeof(TEntity).Name}, EntityView type {typeof(TEntityView).Name} and WooMapEntity type {typeof(TWooMapEntity).Name} initialised.");
        }

        public async Task<WooAPISettings> GetWooAPISettingsAsync()
        {
            IRepository<WooSettings> _WooPrefs = appUnitOfWork.Repository<WooSettings>();

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
                        WooProductCategory _wooProductCategory = new WooProductCategory(_wooAPISettings, appLoggerManager);
                        if (_wooProductCategory == null)
                            currentApplicatioonState.SetWooIsActive(false);
                        else
                            currentApplicatioonState.SetWooIsActive(await _wooProductCategory.CheckProductCategoryLinkAsync());
                    }
                }
                catch (Exception ex)
                {
                    appLoggerManager.LogError($"Error running async tasks: {ex.Message}");
                    throw;
                }
            }

            return currentApplicatioonState.WooIsActive;
        }

        public virtual Task<TWooMapEntity> AddWooItemAndMapAsync(TEntity addEntity)
        {
            appLoggerManager.LogError($"AddWooItemAndMapAsync for Entity: {addEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task DeleteRowAsync(TEntityView deleteViewEntity)
        {
            appLoggerManager.LogError($"DeleteRowAsync for Entity: {deleteViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> DeleteWooItemAsync(Guid deleteWooEntityId, bool deleteFromWoo)
        {
            appLoggerManager.LogError($"DeleteWooItemAsync for Entity: {deleteWooEntityId.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> DoGroupActionAsync(TEntityView toVeiwEntity, BulkAction selectedAction)
        {
            appLoggerManager.LogError($"DoGroupActionAsync for Entity: {toVeiwEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntity>> GetAllItemsAsync()
        {
            appLoggerManager.LogError($"GetAllItemsAsync for Entity not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual TEntity GetItemFromView(TEntityView fromVeiwEntity)
        {
            appLoggerManager.LogError($"GetItemFromView for Entity: {fromVeiwEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<TWooMapEntity> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            appLoggerManager.LogError($"GetWooMappedItemAsync for Entity: {mapWooEntityID.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }
        public virtual Task<List<TWooMapEntity>> GetWooMappedItemsAsync(List<Guid> mapWooEntityIDs)
        {
            appLoggerManager.LogError($"GetWooMappedItemsAsync for Entity: {mapWooEntityIDs.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task InsertRowAsync(TEntityView newVeiwEntity)
        {
            appLoggerManager.LogError($"InsertRowAsync for Entity: {newVeiwEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<bool> IsDuplicateAsync(TEntity checkEntity)
        {
            appLoggerManager.LogError($"IsDuplicate for Entity: {checkEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual bool IsValid(TEntity checkEntity)
        {
            appLoggerManager.LogError($"IsValid for Entity: {checkEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntityView>> LoadAllViewItemsAsync()
        {
            appLoggerManager.LogError($"LoadAllViewItemsAsync for Entity not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual TEntityView NewItemDefaultSetter(TEntityView newViewEntity)
        {
            appLoggerManager.LogError($"NewItemDefaultSetter for Entity: {newViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual List<TEntityView> PopSelectedItems(List<TEntityView> modelViewItems)
        {
            appLoggerManager.LogError($"PopSelectedItems for Entity: {modelViewItems.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual void PushSelectedItems(List<TEntityView> currentSelectedItems)
        {
            appLoggerManager.LogError($"PushSelectedItems for Entity: {currentSelectedItems.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateItemAsync(TEntityView updateItemView)
        {
            appLoggerManager.LogError($"UpdateItemAsync for Entity: {updateItemView.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task UpdateRowAsync(TEntityView updateViewEntity)
        {
            appLoggerManager.LogError($"UpdateRowAsync for Entity: {updateViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooItemAndMappingAsync(TEntityView updateViewEntity)
        {
            appLoggerManager.LogError($"UpdateWooItemAndMapping for Entity: {updateViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooItemAsync(TEntityView updateViewEntity)
        {
            appLoggerManager.LogError($"UpdateWooItemAsync for Entity: {updateViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooMappingAsync(TEntityView updateViewEntity)
        {
            appLoggerManager.LogError($"UpdateWooMappingAsync for Entity: {updateViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntity>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters) // int startPage, int currentPageSize)
        {
            appLoggerManager.LogError($"GetPagedItemsAsync for Entity: {currentDataGridParameters.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<TEntityView> inputDataGridReadData, string inputCustomerFilter)
        {
            appLoggerManager.LogError($"GetDataGridCurrent for Entity: {inputDataGridReadData.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }
        public virtual Task<List<TEntityView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters)
        {
            appLoggerManager.LogError($"LoadViewItemsPaginatedAsync for Entity: {currentDataGridParameters.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }
    }
}
