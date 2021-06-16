using Blazorise.DataGrid;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Common;
using RainbowOF.ViewModels.Lookups;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Repositories.Lookups
{
    public class CategoryWooLinkedViewRepository : WooLinkedView<ItemCategoryLookup, ItemCategoryLookupView, WooCategoryMap>, ICategoryWooLinkedView
    {
        //private ILoggerManager _logger { get; set; }
        //private IAppUnitOfWork _appUnitOfWork { get; set; }
        //private GridSettings _gridSettings { get; set; }

        public CategoryWooLinkedViewRepository(ILoggerManager logger, IAppUnitOfWork appUnitOfWork, GridSettings gridSettings) : base(logger, appUnitOfWork, gridSettings)
        {
            //_logger = logger;
            //_appUnitOfWork = appUnitOfWork;
            //_gridSettings = gridSettings;
        }

        public override async Task DeleteRowAsync(ItemCategoryLookupView deleteViewEntity)
        {
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

            var _recsDelete = await _itemCategoryLookupRepository.DeleteByIdAsync(deleteViewEntity.ItemCategoryLookupId);     //DeleteByAsync(icl => icl.ItemCategoryLookupId == SelectedItemCategoryLookup.ItemCategoryLookupId);

            if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Category: {deleteViewEntity.CategoryName} is no longer found, was it deleted?");
            else
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category: {deleteViewEntity.CategoryName} was it deleted?");
        }
        async Task<int> UpdateWooCategoryMap(ItemCategoryLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooCategoryMap updateWooCategoryMap = await GetWooMappedItemAsync(updatedViewEntity.ItemCategoryLookupId);
            if (updateWooCategoryMap != null)
            {
                if (updateWooCategoryMap.CanUpdate == updatedViewEntity.CanUpdateWooMap)
                {
                }
                else
                {
                    updateWooCategoryMap.CanUpdate = (bool)updatedViewEntity.CanUpdateWooMap;
                    IAppRepository<WooCategoryMap> wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();
                    _recsUpdated = await wooCategoryMapRepository.UpdateAsync(updateWooCategoryMap);
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category: {updatedViewEntity.CategoryName} was updated.");
                }
            }
            else
            {
                // nothing was found, so this probably means they now want to add. We should had a Pop up to check
                int _result = await AddWooItemAndMapAsync(GetItemFromView(updatedViewEntity));
                if (_result != AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category {updatedViewEntity.CategoryName} has been added to woo?");
            }

            return _recsUpdated;
        }
        public override async Task<int> DoGroupActionAsync(ItemCategoryLookupView toVeiwEntity, BulkAction selectedAction)
        {
            if (selectedAction == BulkAction.AllowWooSync)
                toVeiwEntity.CanUpdateWooMap = true;
            else if (selectedAction == BulkAction.DisallowWooSync)
                toVeiwEntity.CanUpdateWooMap = false;
            return await UpdateWooCategoryMap(toVeiwEntity);

        }
        public override async Task<List<ItemCategoryLookup>> GetAllItemsAsync()
        {
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            List<ItemCategoryLookup> _itemCategoryLookups = (await _itemCategoryLookupRepository.GetAllEagerAsync(icl => icl.ParentCategory))
                .OrderBy(icl => icl.ParentCategoryId)
                .ThenBy(icl => icl.CategoryName).ToList();

            return _itemCategoryLookups;
        }
        // used to the Push and Pop
        List<ItemCategoryLookupView> _oldSelectedItems = null;
        public override void PushSelectedItems(List<ItemCategoryLookupView> currentSelectedItems)
        {
            if (currentSelectedItems != null)
            {
                _oldSelectedItems = new List<ItemCategoryLookupView>(currentSelectedItems);
                currentSelectedItems.Clear(); // the refresh does this
            }
            else if (_oldSelectedItems != null)  // current selection is null
                _oldSelectedItems.Clear();  // note that nothing was selected
        }

        public override List<ItemCategoryLookupView> PopSelectedItems(List<ItemCategoryLookupView> modelViewItems)
        {
            List<ItemCategoryLookupView> _popList = null;
            if (_oldSelectedItems != null)
            {
                _popList = new List<ItemCategoryLookupView>();
                foreach (var item in _oldSelectedItems)
                {
                    var _oldSelectdItem = modelViewItems.Where(icl => icl.ItemCategoryLookupId == item.ItemCategoryLookupId).FirstOrDefault();
                    if (_oldSelectdItem != null)  // if it was deleted this will be the case
                        _popList.Add(_oldSelectdItem);
                }
            }
            return _popList;
        }
        public override async Task<List<WooCategoryMap>> GetWooMappedItemsAsync(List<Guid> mapWooEntityIDs)
        {
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            var _WooMappedItems = await _wooCategoryMapRepository.GetByAsync(wcm => mapWooEntityIDs.Contains(wcm.ItemCategoryLookupId));
            return _WooMappedItems.ToList();   //  (await _wooCategoryMapRepository.GetByAsync(wcm => mapWooEntityIDs.Contains(wcm.ItemCategoryLookupId)));
        }

        public override async Task<WooCategoryMap> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            return await _wooCategoryMapRepository.FindFirstAsync(wcm => wcm.ItemCategoryLookupId == mapWooEntityID);
        }

        public override async Task<bool> IsDuplicate(ItemCategoryLookup targetEntity)
        {
            // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            var _exists = (await _itemCategoryLookupRepository.GetByAsync(ml => ml.CategoryName == targetEntity.CategoryName)).ToList();
            return ((_exists != null) && (_exists.Count > 1));
        }

        public override async Task<List<ItemCategoryLookupView>> LoadAllViewItemsAsync()   ////-> only used for in memory
        {
            List<ItemCategoryLookup> _itemCategoryLookups = await GetAllItemsAsync();
            List<ItemCategoryLookupView> _itemCategoryViewLookups = new List<ItemCategoryLookupView>();
//            List<WooCategoryMap> wooCategoryMaps = GetWooMappedItemsAsync();

            // Map Items to Woo CategoryMap
            foreach (var itemCat in _itemCategoryLookups)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                var wooCategoryMap = await GetWooMappedItemAsync(itemCat.ItemCategoryLookupId);

                _itemCategoryViewLookups.Add(new ItemCategoryLookupView
                {
                    ItemCategoryLookupId = itemCat.ItemCategoryLookupId,
                    CategoryName = itemCat.CategoryName,
                    UsedForPrediction = itemCat.UsedForPrediction,
                    ParentCategoryId = itemCat.ParentCategoryId,
                    ParentCategory = itemCat.ParentCategory,
                    Notes = itemCat.Notes,
                    RowVersion = itemCat.RowVersion,

                    CanUpdateWooMap = (wooCategoryMap == null) ? null : wooCategoryMap.CanUpdate
                });
            }
            return _itemCategoryViewLookups;
        }
        public override async Task<List<ItemCategoryLookup>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters)    //int startPage, int currentPageSize)
        {
            IItemCategoryLookupRepository _itemCategoryLookupRepository = _AppUnitOfWork.itemCategoryLookupRepository();
            //_gridSettings.TotalItems = await _itemCategoryLookupRepository.CountAsync();  // get the total number of items to use for paging.
            DataGridItems<ItemCategoryLookup> _dataGridItems = await _itemCategoryLookupRepository.GetPagedDataEagerWithFilterAndOrderByAsync(currentDataGridParameters);
            List<ItemCategoryLookup> _itemCategoryLookups = _dataGridItems.Entities.ToList();
            _GridSettings.TotalItems = _dataGridItems.TotalRecordCount;

            return _itemCategoryLookups;
        }
        public override DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<ItemCategoryLookupView> inputDataGridReadData, string inputCustomerFilter)
        {
            DataGridParameters _dataGridParameters = new DataGridParameters
            {
                CurrentPage = inputDataGridReadData.Page,
                PageSize = inputDataGridReadData.PageSize,
                CustomFilter = inputCustomerFilter
            };

            if (inputDataGridReadData.Columns != null)
            {
                foreach (var col in inputDataGridReadData.Columns)
                {
                    if (col.Direction != Blazorise.SortDirection.None)
                    {
                        if (_dataGridParameters.SortParams == null)
                            _dataGridParameters.SortParams = new List<SortParam>();
                        _dataGridParameters.SortParams.Add(new SortParam
                        {
                            FieldName = col.Field,
                            Direction = col.Direction
                        });
                    }
                    if (!string.IsNullOrEmpty(col.SearchValue))
                    {
                        if (_dataGridParameters.FilterParams == null)
                            _dataGridParameters.FilterParams = new List<FilterParam>();
                        _dataGridParameters.FilterParams.Add(new FilterParam
                        {
                            FieldName = col.Field,
                            FilterBy = col.SearchValue
                        });
                    }
                }
            }
            return _dataGridParameters;
        }
        public override async Task<List<ItemCategoryLookupView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters)
        {
            List<ItemCategoryLookup> _itemCategoryLookups = await GetPagedItemsAsync(currentDataGridParameters);
            List<ItemCategoryLookupView> _itemCategoryViewLookups = new List<ItemCategoryLookupView>();
            // Get a list of all the category maps that exists
            List<Guid> _itemCatIds = _itemCategoryLookups.Where(it => it.ItemCategoryLookupId != Guid.Empty).Select(it => it.ItemCategoryLookupId).ToList();   // get all the ids selected
            // Get all related ids
            List<WooCategoryMap> wooCategoryMaps = await GetWooMappedItemsAsync(_itemCatIds);
            // Map Items to Woo CategoryMap
            foreach (var itemCat in _itemCategoryLookups)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                WooCategoryMap _wooCategoryMap = wooCategoryMaps.Where(wcm => wcm.ItemCategoryLookupId == itemCat.ItemCategoryLookupId).FirstOrDefault();    //  if retrieving / record await GetWooMappedItemAsync(itemCat.ItemCategoryLookupId);

                _itemCategoryViewLookups.Add(new ItemCategoryLookupView
                {
                    ItemCategoryLookupId = itemCat.ItemCategoryLookupId,
                    CategoryName = itemCat.CategoryName,
                    UsedForPrediction = itemCat.UsedForPrediction,
                    ParentCategoryId = itemCat.ParentCategoryId,
                    ParentCategory = itemCat.ParentCategory,
                    Notes = itemCat.Notes,
                    RowVersion = itemCat.RowVersion,

                    CanUpdateWooMap = (_wooCategoryMap == null) ? null : _wooCategoryMap.CanUpdate
                });
            }
            return _itemCategoryViewLookups;
        }
        public override bool IsValid(ItemCategoryLookup checkEntity)
        {
            return (checkEntity.ParentCategoryId != checkEntity.ItemCategoryLookupId);
        }
        public override ItemCategoryLookupView NewItemDefaultSetter(ItemCategoryLookupView newViewEntity)
        {
            if (newViewEntity == null)
                newViewEntity = new ItemCategoryLookupView();

            newViewEntity.CategoryName = "Cat name (must be unique)";
            newViewEntity.Notes = $"Added {DateTime.Now.Date}";
            newViewEntity.ParentCategoryId = Guid.Empty;
            newViewEntity.UsedForPrediction = true;
            newViewEntity.CanUpdateWooMap = null;

            return newViewEntity;
        }

        public override ItemCategoryLookup GetItemFromView(ItemCategoryLookupView fromVeiwEntity)
        {
            ItemCategoryLookup _newItemCategoryLookup = new ItemCategoryLookup
            {
                ItemCategoryLookupId = fromVeiwEntity.ItemCategoryLookupId,
                CategoryName = fromVeiwEntity.CategoryName,
                UsedForPrediction = fromVeiwEntity.UsedForPrediction,
                ParentCategoryId = (fromVeiwEntity.ParentCategoryId == Guid.Empty) ? null : fromVeiwEntity.ParentCategoryId,
                Notes = fromVeiwEntity.Notes,
            };

            return _newItemCategoryLookup;
        }
        public override async Task<int> UpdateItemAsync(ItemCategoryLookupView updateViewItem)
        {
            int _recsUpdted = 0;
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            // first check it exists - it could have been deleted 
            ItemCategoryLookup _currentItemCategoryLookup = await _itemCategoryLookupRepository.GetByIdAsync(updateViewItem.ItemCategoryLookupId);
            if (_currentItemCategoryLookup == null)
            {
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Category: {updateViewItem.CategoryName} is no longer found, was it deleted?");
                return AppUnitOfWork.CONST_WASERROR;
            }
            else
            {
                _currentItemCategoryLookup.CategoryName = updateViewItem.CategoryName;
                _currentItemCategoryLookup.ParentCategoryId = (updateViewItem.ParentCategoryId == Guid.Empty) ? null : updateViewItem.ParentCategoryId;
                _currentItemCategoryLookup.UsedForPrediction = updateViewItem.UsedForPrediction;
                _currentItemCategoryLookup.Notes = updateViewItem.Notes;
                _recsUpdted = await _itemCategoryLookupRepository.UpdateAsync(_currentItemCategoryLookup);
                if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.CategoryName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating Category");
                if (await UpdateWooItemAndMapping(updateViewItem) == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.CategoryName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating Category Map");   // should we send a message here error = mapping not updated 

            }
            return _recsUpdted;
        }
        public override async Task InsertRowAsync(ItemCategoryLookupView newVeiwEntity)
        {
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            // first check we do not already have a category like this.
            if (await _itemCategoryLookupRepository.FindFirstAsync(icl => icl.CategoryName == newVeiwEntity.CategoryName) == null)
            {
                ItemCategoryLookup _NewItemCategoryLookup = GetItemFromView(newVeiwEntity); // store this here since when it is added it will automatically update the id field
                int _recsAdded = await _itemCategoryLookupRepository.AddAsync(_NewItemCategoryLookup);
                if (_recsAdded != AppUnitOfWork.CONST_WASERROR)
                {
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.CategoryName} - added", "Category Added");
                    if (newVeiwEntity.CanUpdateWooMap ?? false)
                    {
                        // they selected to update woo so add to Woo
                        if (await AddWooItemAndMapAsync(_NewItemCategoryLookup) == AppUnitOfWork.CONST_WASERROR)   // add if they select to update
                            _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding {newVeiwEntity.CategoryName} to Woo - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Woo Category");
                        else
                            _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.CategoryName} - added to Woo", "Woo Category Added");
                    }
                }
                else
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.CategoryName} - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Category");
            }
            else
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.CategoryName} already exists, so could not be added.");
            //-> done in parent       await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }

        public override async Task UpdateRowAsync(ItemCategoryLookupView updateVeiwEntity)
        {
            if (await IsDuplicate(updateVeiwEntity))
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Category Name: {updateVeiwEntity.CategoryName} - already exists, cannot be updated", "Exists already");
            else
            {
                if (IsValid(updateVeiwEntity))
                {
                    // update and check for errors 
                    if (await UpdateItemAsync(updateVeiwEntity) == AppUnitOfWork.CONST_WASERROR)
                    {
                        _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating category: {updateVeiwEntity.CategoryName} -  {_AppUnitOfWork.GetErrorMessage()}", "Updating Category Error");
                    }
                    else
                        _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"category: {updateVeiwEntity.CategoryName} was updated.");
                }
                else
                {
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Category Item {updateVeiwEntity.CategoryName} cannot be parent and child.", "Error updating");
                }
            }
            //-> done in parent await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }
        public override async Task<int> UpdateWooMappingAsync(ItemCategoryLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooCategoryMap _updateWooCategoryMap = await GetWooMappedItemAsync(updatedViewEntity.ItemCategoryLookupId);
            if (_updateWooCategoryMap != null)
            {
                if (_updateWooCategoryMap.CanUpdate == updatedViewEntity.CanUpdateWooMap)
                {
                    // not necessary to display message.
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo Category Map for category: {updatedViewEntity.CategoryName} has not changed, so was not updated?");
                }
                else
                {
                    _updateWooCategoryMap.CanUpdate = (bool)updatedViewEntity.CanUpdateWooMap;
                    IAppRepository<WooCategoryMap> wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();
                    _recsUpdated = await wooCategoryMapRepository.UpdateAsync(_updateWooCategoryMap);
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category: {updatedViewEntity.CategoryName} was updated.");
                }
            }
            else
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Category Map for category: {updatedViewEntity.CategoryName} is no longer found, was it deleted?");

            await LoadAllViewItemsAsync();
            return _recsUpdated;
        }

        private async Task<WooCategoryMap> GetWooCategoryMapFromID(Guid? sourceWooEntityId)
        {
            if (sourceWooEntityId == null)
                return null;

            IAppRepository<WooCategoryMap> _wooCategoryMapRepo = _AppUnitOfWork.Repository<WooCategoryMap>();
            WooCategoryMap _wooCategoryMap = await _wooCategoryMapRepo.FindFirstAsync(wcm => wcm.ItemCategoryLookupId == sourceWooEntityId);
            return _wooCategoryMap;
        }
        private async Task<int> DeleteWooCategoryLink(WooCategoryMap deleteWooCategoryMap)
        {
            int _result = 0;
            IAppRepository<WooCategoryMap> _wooCategoryMapRepo = _AppUnitOfWork.Repository<WooCategoryMap>();

            _result = await _wooCategoryMapRepo.DeleteByIdAsync(deleteWooCategoryMap.WooCategoryMapId);

            return _result;
        }
        private async Task<IWooProductCategory> GetIWooProductCategory()
        {
            //IAppRepository<WooSettings> _WooPrefs = _appUnitOfWork.Repository<WooSettings>();

            //WooSettings _wooSettings = await _WooPrefs.FindFirstAsync();
            //if (_wooSettings == null)
            //    return null;
            //WooAPISettings _wooAPISettings = new WooAPISettings(_wooSettings);
            WooAPISettings _wooAPISettings = await GetWooAPISettingsAsync();
            return new WooProductCategory(_wooAPISettings, _Logger);
        }
        private async Task<int> DeleteWooCategory(WooCategoryMap deleteWooCategoryMap)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;  // if all goes well this will be change to the number of records returned

            IWooProductCategory _WooProductCategoryRepository = await GetIWooProductCategory();
            if (_WooProductCategoryRepository != null)
            {
                ProductCategory _tempWooCat = await _WooProductCategoryRepository.DeleteProductCategoryByIdAsync(deleteWooCategoryMap.WooCategoryId);
                _result =   (_tempWooCat == null) ? AppUnitOfWork.CONST_WASERROR : Convert.ToInt32(_tempWooCat.id);  // return the id
            }
            return _result;
        }
        /// <summary>
        /// Delete a Woo Category from the mapped table and Woo, if they ask us to 
        /// </summary>
        /// <param name="WooEntityId">Id to delete</param>
        /// <param name="deleteFromWoo">True of we want to delete from Woo too</param>
        /// <returns></returns>
        public override async Task<int> DeleteWooItemAsync(Guid deleteWooEntityId, bool deleteFromWoo)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;
            // delete the woo category
            WooCategoryMap _wooCategoryMap = await GetWooCategoryMapFromID(deleteWooEntityId);
            if (_wooCategoryMap == null)
                _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Category Id {deleteWooEntityId} was not found to have a Woo Category Map.");
            else
            {
                if (deleteFromWoo)
                {
                    _result = await DeleteWooCategory(_wooCategoryMap); ///Delete the category in Woo
                    if (_result == AppUnitOfWork.CONST_WASERROR)
                        _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was not deleted from Woo categories. Error {_AppUnitOfWork.GetErrorMessage()}");
                    else
                        _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was deleted from Woo categories.");
                }
                _result = await DeleteWooCategoryLink(_wooCategoryMap);   //Delete our link data, if there was an error should we?
                if (_result == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was not deleted from Woo linked categories. Error {_AppUnitOfWork.GetErrorMessage()}");
                else
                    _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was deleted from Woo linked categories.");
            }
            return _result;
        }
        private async Task<ProductCategory> AddItemToWooOnlySync(ItemCategoryLookup addEntity)
        {
            IWooProductCategory _wooProductCategoryRepository = await GetIWooProductCategory();
            if (_wooProductCategoryRepository == null)
                return null;

            ProductCategory _wooProductCategory = new ProductCategory
            {
                name = addEntity.CategoryName,
                description = addEntity.CategoryName
            };

            if ((addEntity.ParentCategoryId != null) && (addEntity.ParentCategoryId != Guid.Empty))
            {
                WooCategoryMap _WooParentCategoryMap = await GetWooCategoryMapFromID(addEntity.ParentCategoryId);    // if they have a parent get the mapped id
                if (_WooParentCategoryMap != null)
                    _wooProductCategory.parent = (uint)_WooParentCategoryMap.WooCategoryId;
            }

            return await _wooProductCategoryRepository.AddProductCategoryAsync(_wooProductCategory);  // should return a new version with ID
        }
        private async Task<int> MapOurItemToWooItemSync(ProductCategory newWooProductCategory, ItemCategoryLookup addViewEntity)
        {
            // create a map to the woo category maps using the id and the category
            //
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            return await _wooCategoryMapRepository.AddAsync(new WooCategoryMap
            {
                ItemCategoryLookupId = addViewEntity.ItemCategoryLookupId,
                WooCategoryId = (int)newWooProductCategory.id,
                WooCategoryName = addViewEntity.CategoryName,
                CanUpdate = true,
                WooCategoryParentId = (int)newWooProductCategory.parent
            });
        }

        /// <summary>
        /// Add the item to woo
        /// </summary>
        /// <param name="addEntity"></param>
        /// <returns></returns>
        public override async Task<int> AddWooItemAndMapAsync(ItemCategoryLookup addEntity)
        {
            // check it the item exists in woo (we did not do this as there is no such call;, if so get is id and return, otherwise add it and get its id
            //ProductCategory _wooProductCategory = await GetWooProductCategoryByName(addViewEntity.CategoryName);
            //if (_wooProductCategory == null)
            //{
            ProductCategory _wooProductCategory = await AddItemToWooOnlySync(addEntity);
            if (_wooProductCategory == null)
                return AppUnitOfWork.CONST_WASERROR;
            //}
            return await MapOurItemToWooItemSync(_wooProductCategory, addEntity);
        }

        //private async Task<ProductCategory> GetWooProductCategoryByName(string categoryName)
        //{
        //    IWooProductCategory _WooProductCategoryRepository = await GetIWooProductCategory();
        //    if (_WooProductCategoryRepository == null)
        //        return null;

        //    return _WooProductCategoryRepository.FindProductCategoryByName(categoryName);
        //}

        public override async Task<int> UpdateWooItemAsync(ItemCategoryLookupView updateViewEntity)
        {
            int _result = 0;  /// null or not found
            if ((updateViewEntity.HasWooCategoryMap) && ((bool)(updateViewEntity.CanUpdateWooMap)))
            {
                IWooProductCategory _wooProductCategoryRepository = await GetIWooProductCategory();
                if (_wooProductCategoryRepository != null)                     //  - > if it does not exist then what?
                {
                    WooCategoryMap _updateWooMapEntity = await GetWooCategoryMapFromID(updateViewEntity.ItemCategoryLookupId);
                    if (_updateWooMapEntity == null)
                    {
                        // need to add the category -> this is done later.
                    }
                    else
                    {
                        ProductCategory _wooProductCategory = await _wooProductCategoryRepository.GetProductCategoryByIdAsync(_updateWooMapEntity.WooCategoryId);
                        if (_wooProductCategory == null)
                        {
                            return AppUnitOfWork.CONST_WASERROR;  /// oops what happened >?
                        }
                        else
                        {
                            // get id of parent
                            WooCategoryMap _ParentWooCategoryMap = await GetWooCategoryMapFromID(updateViewEntity.ParentCategoryId);
                            int _newParentId = (_ParentWooCategoryMap == null) ? 0 : _ParentWooCategoryMap.WooCategoryId;
                            if ((!_wooProductCategory.name.Equals(updateViewEntity.CategoryName)) || ((_wooProductCategory.parent ?? 0) != _newParentId))
                            {
                                _wooProductCategory.name = updateViewEntity.CategoryName;  // only update if necessary
                                _wooProductCategory.parent = (uint)_newParentId;
                                var _res = ((await _wooProductCategoryRepository.UpdateProductCategoryAsync(_wooProductCategory)));
                                _result = ((_res == null) || (_res.id ==null)) ? AppUnitOfWork.CONST_WASERROR : (int)_res.id; // if null there is an issue
                            }
                        }
                    }
                }
            }
            return _result;
        }
        /// <summary>
        /// Checks if the any of the Woo link values where changed during an edit, if so update
        /// </summary>
        /// <param name="updateViewEntity">The Entity that is being updated</param>
        /// <returns>null if nothing changed or the new WooCategopryMap</returns>
        public override async Task<int> UpdateWooItemAndMapping(ItemCategoryLookupView updateViewEntity)
        {
            int _result = await UpdateWooItemAsync(updateViewEntity);
            if (_result > 0)
                _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Updated woo category {updateViewEntity.CategoryName}.");
            else if (_result == AppUnitOfWork.CONST_WASERROR)
                _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo category {updateViewEntity.CategoryName} update failed.");

            _result = await UpdateWooCategoryMap(updateViewEntity);
            return _result;
        }
    }
}
