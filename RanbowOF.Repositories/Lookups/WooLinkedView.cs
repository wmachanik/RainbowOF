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

        public ILoggerManager _logger { get; set; }
        public IAppUnitOfWork _appUnitOfWork { get; set; }
        public GridSettings _gridSettings { get; set; }
        public WooLinkedView(ILoggerManager logger, IAppUnitOfWork appUnitOfWork, GridSettings gridSettings)
        {
            _logger = logger;
            _appUnitOfWork = appUnitOfWork;
            _gridSettings = gridSettings;
        }

        public async Task<WooAPISettings> GetWooAPISettingsAsync()
        {
            IAppRepository<WooSettings> _WooPrefs = _appUnitOfWork.Repository<WooSettings>();

            WooSettings _wooSettings = await _WooPrefs.FindFirstAsync();
            if (_wooSettings == null)
                return null;
            return new WooAPISettings(_wooSettings);
        }

        public virtual async Task<bool> WooIsActive(ApplicationState currentApplicatioonState)
        {
            if (!currentApplicatioonState.HaveCheckState)
            {
                try
                {
                    WooAPISettings _WooAPISettings = await GetWooAPISettingsAsync();
                    if (_WooAPISettings == null)
                        currentApplicatioonState.SetWooIsActive(false);
                    else
                    {
                        WooProductCategory _WooProductCategory = new WooProductCategory(_WooAPISettings, _logger);
                        if (_WooProductCategory == null)
                            currentApplicatioonState.SetWooIsActive(false);
                        else
                            currentApplicatioonState.SetWooIsActive(await _WooProductCategory.CheckProductCategoryLinkAsync());
                    }
                }
                catch ( Exception ex )
                {
                    _logger.LogError($"Error running async tasks: {ex.Message}");
                    throw;
                }
            }

            return currentApplicatioonState.WooIsActive;
        }


        public virtual Task<int> AddWooItemAndMapAsync(TEntity addEntity)
        {
            throw new NotImplementedException();
        }

        public virtual Task DeleteRowAsync(TEntityView deleteVeiwEntity)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> DeleteWooItemAsync(Guid deleteWooEntityId, bool deleteFromWoo)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> DoGroupActionAsync(TEntityView toVeiwEntity, BulkAction selectedAction)
        {
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntity>> GetAllItemsAsync()
        {
            throw new NotImplementedException();
        }

        public virtual TEntity GetItemFromView(TEntityView fromVeiwEntity)
        {
            throw new NotImplementedException();
        }

        public virtual Task<TWooMapEntity> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            throw new NotImplementedException();
        }
        public virtual Task<List<TWooMapEntity>> GetWooMappedItemsAsync(List<Guid> mapWooEntityIDs)
        {
            throw new NotImplementedException();
        }

        public virtual Task InsertRowAsync(TEntityView newVeiwEntity)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> IsDuplicate(TEntity checkEntity)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsValid(TEntity checkEntity)
        {
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntityView>> LoadAllViewItemsAsync()
        {
            throw new NotImplementedException();
        }

        public virtual TEntityView NewItemDefaultSetter(TEntityView newViewEntity)
        {
            throw new NotImplementedException();
        }

        public virtual List<TEntityView> PopSelectedItems(List<TEntityView> modelViewItems)
        {
            throw new NotImplementedException();
        }

        public virtual void PushSelectedItems(List<TEntityView> currentSelectedItems)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateItemAsync(TEntityView updateItem)
        {
            throw new NotImplementedException();
        }

        public virtual Task UpdateRowAsync(TEntityView updateVeiwEntity)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooItemAndMapping(TEntityView updateViewEntity)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooItemAsync(TEntityView updateViewEntity)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateWooMappingAsync(TEntityView updateVeiwEntity)
        {
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntity>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters) // int startPage, int currentPageSize)
        {
            throw new NotImplementedException();
        }

        public virtual DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<TEntityView> inputDataGridReadData, string inputCustomerFilter)
        {
            throw new NotImplementedException();
        }
        public virtual Task<List<TEntityView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters)
        {
            throw new NotImplementedException();
        }
    }
}
