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

namespace RainbowOF.Repositories.Lookups
{
    public class WooLinkedView<TEntity, TEntityView, TWooMapEntity> : IWooLinkedView<TEntity, TEntityView, TWooMapEntity>
        where TEntity : class
        where TEntityView : class
        where TWooMapEntity : class
    {
        public ILoggerManager _Logger { get; set; }
        public IAppUnitOfWork _AppUnitOfWork { get; set; }
        public GridSettings _GridSettings { get; set; }
        public IMapper _Mapper;


        public WooLinkedView(ILoggerManager sourceLogger, 
                                IAppUnitOfWork sourceAppUnitOfWork, 
                                GridSettings sourceGridSettings,
                                IMapper sourceMapper)
        {
            _Logger = sourceLogger;
            _AppUnitOfWork = sourceAppUnitOfWork;
            _GridSettings = sourceGridSettings;
            _Mapper = sourceMapper;
        }

        public async Task<WooAPISettings> GetWooAPISettingsAsync()
        {
            IAppRepository<WooSettings> _WooPrefs = _AppUnitOfWork.Repository<WooSettings>();

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
                        WooProductCategory _wooProductCategory = new WooProductCategory(_wooAPISettings, _Logger);
                        if (_wooProductCategory == null)
                            currentApplicatioonState.SetWooIsActive(false);
                        else
                            currentApplicatioonState.SetWooIsActive(await _wooProductCategory.CheckProductCategoryLinkAsync());
                    }
                }
                catch ( Exception ex )
                {
                    _Logger.LogError($"Error running async tasks: {ex.Message}");
                    throw;
                }
            }

            return currentApplicatioonState.WooIsActive;
        }


        public virtual Task<int> AddWooItemAndMapAsync(TEntity addEntity)
        {
            _Logger.LogError($"AddWooItemAndMapAsync for Entity: {addEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task DeleteRowAsync(TEntityView deleteViewEntity)
        {
            _Logger.LogError($"DeleteRowAsync for Entity: {deleteViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> DeleteWooItemAsync(Guid deleteWooEntityId, bool deleteFromWoo)
        {
            _Logger.LogError($"DeleteWooItemAsync for Entity: {deleteWooEntityId.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> DoGroupActionAsync(TEntityView toVeiwEntity, BulkAction selectedAction)
        {
            _Logger.LogError($"DoGroupActionAsync for Entity: {toVeiwEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntity>> GetAllItemsAsync()
        {
            _Logger.LogError($"GetAllItemsAsync for Entity not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual TEntity GetItemFromView(TEntityView fromVeiwEntity)
        {
            _Logger.LogError($"GetItemFromView for Entity: {fromVeiwEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<TWooMapEntity> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            _Logger.LogError($"GetWooMappedItemAsync for Entity: {mapWooEntityID.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }
        public virtual Task<List<TWooMapEntity>> GetWooMappedItemsAsync(List<Guid> mapWooEntityIDs)
        {
            _Logger.LogError($"GetWooMappedItemsAsync for Entity: {mapWooEntityIDs.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task InsertRowAsync(TEntityView newVeiwEntity)
        {
            _Logger.LogError($"InsertRowAsync for Entity: {newVeiwEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<bool> IsDuplicateAsync(TEntity checkEntity)
        {
            _Logger.LogError($"IsDuplicate for Entity: {checkEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual bool IsValid(TEntity checkEntity)
        {
            _Logger.LogError($"IsValid for Entity: {checkEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntityView>> LoadAllViewItemsAsync()
        {
            _Logger.LogError($"LoadAllViewItemsAsync for Entity not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual TEntityView NewItemDefaultSetter(TEntityView newViewEntity)
        {
            _Logger.LogError($"NewItemDefaultSetter for Entity: {newViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual List<TEntityView> PopSelectedItems(List<TEntityView> modelViewItems)
        {
            _Logger.LogError($"PopSelectedItems for Entity: {modelViewItems.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual void PushSelectedItems(List<TEntityView> currentSelectedItems)
        {
            _Logger.LogError($"PushSelectedItems for Entity: {currentSelectedItems.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateItemAsync(TEntityView updateItem)
        {
            _Logger.LogError($"UpdateItemAsync for Entity: {updateItem.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task UpdateRowAsync(TEntityView updateViewEntity)
        {
            _Logger.LogError($"UpdateRowAsync for Entity: {updateViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooItemAndMappingAsync(TEntityView updateViewEntity)
        {
            _Logger.LogError($"UpdateWooItemAndMapping for Entity: {updateViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooItemAsync(TEntityView updateViewEntity)
        {
            _Logger.LogError($"UpdateWooItemAsync for Entity: {updateViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooMappingAsync(TEntityView updateViewEntity)
        {
            _Logger.LogError($"UpdateWooMappingAsync for Entity: {updateViewEntity.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntity>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters) // int startPage, int currentPageSize)
        {
            _Logger.LogError($"GetPagedItemsAsync for Entity: {currentDataGridParameters.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }

        public virtual DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<TEntityView> inputDataGridReadData, string inputCustomerFilter)
        {
            _Logger.LogError($"GetDataGridCurrent for Entity: {inputDataGridReadData.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }
        public virtual Task<List<TEntityView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters)
        {
            _Logger.LogError($"LoadViewItemsPaginatedAsync for Entity: {currentDataGridParameters.ToString()} not implemented, place holder executed. Please implement.");
            throw new NotImplementedException();
        }
    }
}
