using AutoMapper;
using Blazorise.DataGrid;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Integrations;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Common;
using RainbowOF.ViewModels.Lookups;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Repositories.Lookups
{
    public class CategoryWooLinkedViewRepository : WooLinkedView<ItemCategoryLookup, ItemCategoryLookupView, WooCategoryMap>, ICategoryWooLinkedView
    {
        #region Private vars
        //private ILoggerManager appLoggerManager { get; set; }
        //private IAppUnitOfWork AppUnitOfWork { get; set; }
        //private GridSettings _WooLinkedGridSettings { get; set; }
        #endregion
        #region Initialisation
        public CategoryWooLinkedViewRepository(ILoggerManager sourceLogger,
                                               IUnitOfWork sourceAppUnitOfWork,
                                               //GridSettings sourceGridSettings,
                                               IMapper sourceMapper) : base(sourceLogger, sourceAppUnitOfWork, /*sourceGridSettings, */sourceMapper)
        {
            //appLoggerManager = logger;
            //AppUnitOfWork = AppUnitOfWork;
            //_WooLinkedGridSettings = gridSettings;
            sourceLogger.LogDebug("CategoryWooLinkedViewRepository initialised.");
        }
        #endregion
        #region Interface routines
        public override async Task DeleteRowAsync(ItemCategoryLookupView deleteViewEntity)
        {
            IRepository<ItemCategoryLookup> _itemCategoryLookupRepository = AppUnitOfWork.Repository<ItemCategoryLookup>();
            // do we need to conver the class?
            var _recsDelete = await _itemCategoryLookupRepository.DeleteByEntityAsync((ItemCategoryLookup)deleteViewEntity);     //DeleteByAsync(icl => icl.ItemCategoryLookupId == SelectedItemCategoryLookup.ItemCategoryLookupId);

            if (_recsDelete == UnitOfWork.CONST_WASERROR)
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Category: {deleteViewEntity.CategoryName} is no longer found, was it deleted?");
            else
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Category: {deleteViewEntity.CategoryName} was it deleted?");
        }
        async Task<int> UpdateWooCategoryMapAsync(ItemCategoryLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooCategoryMap updateWooCategoryMap = await GetWooMappedItemAsync(updatedViewEntity.ItemCategoryLookupId);
            if (updateWooCategoryMap != null)
            {
                if (updateWooCategoryMap.CanUpdate == updatedViewEntity.CanUpdateECommerceMap)
                {
                }
                else
                {
                    updateWooCategoryMap.CanUpdate = (bool)updatedViewEntity.CanUpdateECommerceMap;
                    IRepository<WooCategoryMap> wooCategoryMapRepository = AppUnitOfWork.Repository<WooCategoryMap>();
                    _recsUpdated = await wooCategoryMapRepository.UpdateAsync(updateWooCategoryMap);
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Category: {updatedViewEntity.CategoryName} was updated.");
                }
            }
            else
            {
                // nothing was found, so this probably means they now want to add. We should had a Pop up to check
                var _result = await AddWooItemAndMapAsync(GetItemFromView(updatedViewEntity));
                if (_result != null)
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Category {updatedViewEntity.CategoryName} has been added to woo?");
            }

            return _recsUpdated;
        }
        public override async Task<int> DoGroupActionAsync(ItemCategoryLookupView toVeiwEntity, BulkAction selectedAction)
        {
            if (selectedAction == BulkAction.AllowWooSync)
                toVeiwEntity.CanUpdateECommerceMap = true;
            else if (selectedAction == BulkAction.DisallowWooSync)
                toVeiwEntity.CanUpdateECommerceMap = false;
            return await UpdateWooCategoryMapAsync(toVeiwEntity);

        }
        public override async Task<List<ItemCategoryLookup>> GetAllItemsAsync()
        {
            IRepository<ItemCategoryLookup> _itemCategoryLookupRepository = AppUnitOfWork.Repository<ItemCategoryLookup>();
            List<ItemCategoryLookup> _itemCategoryLookups = (await _itemCategoryLookupRepository.GetAllEagerAsync(icl => icl.ParentCategory))
                .OrderBy(icl => icl.ParentCategoryId)
                .ThenBy(icl => icl.CategoryName).ToList();

            return _itemCategoryLookups;
        }
        // used to the Push and Pop
        private List<ItemCategoryLookupView> _oldSelectedItems = null;
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
            IRepository<WooCategoryMap> _wooCategoryMapRepository = AppUnitOfWork.Repository<WooCategoryMap>();

            var _WooMappedItems = await _wooCategoryMapRepository.GetByAsync(wcm => mapWooEntityIDs.Contains(wcm.ItemCategoryLookupId));
            return _WooMappedItems.ToList();   //  (await _wooCategoryMapRepository.GetByAsync(wcm => mapWooEntityIDs.Contains(wcm.ItemCategoryLookupId)));
        }

        public override async Task<WooCategoryMap> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            IRepository<WooCategoryMap> _wooCategoryMapRepository = AppUnitOfWork.Repository<WooCategoryMap>();

            return await _wooCategoryMapRepository.GetByIdAsync(wcm => wcm.ItemCategoryLookupId == mapWooEntityID);
        }

        public override async Task<bool> IsDuplicateAsync(ItemCategoryLookup targetEntity)
        {
            // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
            IRepository<ItemCategoryLookup> _itemCategoryLookupRepository = AppUnitOfWork.Repository<ItemCategoryLookup>();
            var _exists = (await _itemCategoryLookupRepository.GetByAsync(ml => ml.CategoryName == targetEntity.CategoryName)).ToList();
            return ((_exists != null) && (_exists.Count > 1));
        }

        public override async Task<List<ItemCategoryLookupView>> LoadAllViewItemsAsync()   ////-> only used for in memory
        {
            List<ItemCategoryLookup> _itemCategoryLookups = await GetAllItemsAsync();
            List<ItemCategoryLookupView> _itemCategoryViewLookups = new();
            //            List<WooCategoryMap> wooCategoryMaps = GetWooMappedItemsAsync();

            // Map Items to Woo CategoryMap
            foreach (var itemCat in _itemCategoryLookups)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                var wooCategoryMap = await GetWooMappedItemAsync(itemCat.ItemCategoryLookupId);

                ItemCategoryLookupView _itemCategoryLookupView = new();
                AppMapper.Map(itemCat, _itemCategoryLookupView);
                _itemCategoryLookupView.CanUpdateECommerceMap = wooCategoryMap?.CanUpdate ?? null;
            }
            return _itemCategoryViewLookups;
        }
        public override async Task<List<ItemCategoryLookup>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters)    //int startPage, int currentPageSize)
        {
            //_WooLinkedGridSettings.TotalItems = await _itemCategoryLookupRepository.CountAsync();  // get the total number of items to use for paging.
            List<ItemCategoryLookup> _itemCategoryLookups = null;
            DataGridItems<ItemCategoryLookup> _dataGridItems = await AppUnitOfWork.ItemCategoryLookupRepository.GetPagedDataEagerWithFilterAndOrderByAsync(currentDataGridParameters);
            if (_dataGridItems != null)
            {
                _itemCategoryLookups = _dataGridItems.Entities.OrderBy(icl => icl.FullCategoryName).ToList();
                CurrWooLinkedGridSettings.TotalItems = _dataGridItems.TotalRecordCount;
            }
            return _itemCategoryLookups;
        }
        public override DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<ItemCategoryLookupView> inputDataGridReadData, string inputCustomerFilter)
        {
            DataGridParameters _dataGridParameters = new()
            {
                CurrentPage = inputDataGridReadData.Page,
                PageSize = inputDataGridReadData.PageSize,
                CustomFilter = inputCustomerFilter
            };

            if (inputDataGridReadData.Columns != null)
            {
                foreach (var col in inputDataGridReadData.Columns)
                {
                    if (col.SortDirection != Blazorise.SortDirection.Default)
                    {
                        _dataGridParameters.SortParams ??= new List<SortParam>();  // if null create a new one
                        _dataGridParameters.SortParams.Add(new SortParam
                        {
                            FieldName = col.Field,
                            Direction = col.SortDirection
                        });
                    }
                    if (!string.IsNullOrEmpty((string)col.SearchValue))
                    {
                        _dataGridParameters.FilterParams ??= new List<FilterParam>();  // if null create a new one
                        _dataGridParameters.FilterParams.Add(new FilterParam
                        {
                            FieldName = col.Field,
                            FilterBy = (string)col.SearchValue
                        });
                    }
                }
            }
            return _dataGridParameters;
        }
        public override async Task<List<ItemCategoryLookupView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters)
        {
            List<ItemCategoryLookup> _itemCategoryLookups = await GetPagedItemsAsync(currentDataGridParameters);
            List<ItemCategoryLookupView> _itemCategoryViewLookups = new();
            // Get a list of all the category maps that exists
            List<Guid> _itemCatIds = _itemCategoryLookups.Where(it => it.ItemCategoryLookupId != Guid.Empty).Select(it => it.ItemCategoryLookupId).ToList();   // get all the ids selected
            // Get all related ids
            List<WooCategoryMap> wooCategoryMaps = await GetWooMappedItemsAsync(_itemCatIds);
            // Map Items to Woo CategoryMap
            foreach (var itemCat in _itemCategoryLookups)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                WooCategoryMap _wooCategoryMap = wooCategoryMaps.Where(wcm => wcm.ItemCategoryLookupId == itemCat.ItemCategoryLookupId).FirstOrDefault();    //  if retrieving / record await GetWooMappedItemAsync(itemCat.ItemCategoryLookupId);

                ItemCategoryLookupView _itemCategoryLookupView = new();
                AppMapper.Map(itemCat, _itemCategoryLookupView);
                _itemCategoryLookupView.CanUpdateECommerceMap = _wooCategoryMap?.CanUpdate ?? null;
                _itemCategoryViewLookups.Add(_itemCategoryLookupView);
            }
            return _itemCategoryViewLookups;
        }
        public override bool IsValid(ItemCategoryLookup checkEntity)
        {
            return (checkEntity.ParentCategoryId != checkEntity.ItemCategoryLookupId);
        }
        public override ItemCategoryLookupView NewItemDefaultSetter(ItemCategoryLookupView newViewEntity)
        {
            newViewEntity ??= new ItemCategoryLookupView();  // if null create a new record
            newViewEntity.CategoryName = "Cat name (must be unique)";
            newViewEntity.Notes = $"Added {DateTime.Now.Date}";
            newViewEntity.ParentCategoryId = Guid.Empty;
            newViewEntity.UsedForPrediction = true;
            newViewEntity.CanUpdateECommerceMap = null;

            return newViewEntity;
        }

        public override ItemCategoryLookup GetItemFromView(ItemCategoryLookupView fromVeiwEntity)
        {
            ItemCategoryLookup _newItemCategoryLookup = new()
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
            IRepository<ItemCategoryLookup> _itemCategoryLookupRepository = AppUnitOfWork.Repository<ItemCategoryLookup>();
            // first check it exists - it could have been deleted 
            ItemCategoryLookup _currentItemCategoryLookup = await _itemCategoryLookupRepository.GetByIdAsync(updateViewItem.ItemCategoryLookupId);
            if (_currentItemCategoryLookup == null)
            {
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Category: {updateViewItem.CategoryName} is no longer found, was it deleted?");
                return UnitOfWork.CONST_WASERROR;
            }
            else
            {
                AppMapper.Map(updateViewItem, _currentItemCategoryLookup);
                //_currentItemCategoryLookup.CategoryName = updateViewItem.CategoryName;
                _currentItemCategoryLookup.ParentCategoryId = (updateViewItem.ParentCategoryId == Guid.Empty) ? null : updateViewItem.ParentCategoryId;
                //_currentItemCategoryLookup.UsedForPrediction = updateViewItem.UsedForPrediction;
                //_currentItemCategoryLookup.Notes = updateViewItem.Notes;
                int _recsUpdted = await _itemCategoryLookupRepository.UpdateAsync(_currentItemCategoryLookup);
                if (_recsUpdted == UnitOfWork.CONST_WASERROR)
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.CategoryName} - {AppUnitOfWork.GetErrorMessage()}", "Error updating Category");
                if (await UpdateWooItemAndMappingAsync(updateViewItem) == UnitOfWork.CONST_WASERROR)
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.CategoryName} - {AppUnitOfWork.GetErrorMessage()}", "Error updating Category Map");   // should we send a message here error = mapping not updated 

                return _recsUpdted;
            }
        }
        public override async Task InsertRowAsync(ItemCategoryLookupView newVeiwEntity)
        {
            IRepository<ItemCategoryLookup> _itemCategoryLookupRepository = AppUnitOfWork.Repository<ItemCategoryLookup>();
            // first check we do not already have a category like this.
            if (await _itemCategoryLookupRepository.GetByIdAsync(icl => icl.CategoryName == newVeiwEntity.CategoryName) == null)
            {
                ItemCategoryLookup _NewItemCategoryLookup = GetItemFromView(newVeiwEntity); // store this here since when it is added it will automatically update the id field
                var _recsAdded = await _itemCategoryLookupRepository.AddAsync(_NewItemCategoryLookup);
                if (_recsAdded != null)
                {
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.CategoryName} - added", "Category Added");
                    if (newVeiwEntity.CanUpdateECommerceMap ?? false)
                    {
                        // they selected to update woo so add to Woo
                        if (await AddWooItemAndMapAsync(_NewItemCategoryLookup) == null)   // add if they select to update
                            await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error adding {newVeiwEntity.CategoryName} to Woo - {AppUnitOfWork.GetErrorMessage()}", "Error adding Woo Category");
                        else
                            await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.CategoryName} - added to Woo", "Woo Category Added");
                    }
                }
                else
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.CategoryName} - {AppUnitOfWork.GetErrorMessage()}", "Error adding Category");
            }
            else
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.CategoryName} already exists, so could not be added.");
            //-> done in parent       await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }

        public override async Task UpdateRowAsync(ItemCategoryLookupView updateVeiwEntity)
        {
            if (await IsDuplicateAsync(updateVeiwEntity))
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Category Name: {updateVeiwEntity.CategoryName} - already exists, cannot be updated", "Exists already");
            else
            {
                if (IsValid(updateVeiwEntity))
                {
                    // update and check for errors 
                    if (await UpdateItemAsync(updateVeiwEntity) == UnitOfWork.CONST_WASERROR)
                    {
                        await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error updating category: {updateVeiwEntity.CategoryName} -  {AppUnitOfWork.GetErrorMessage()}", "Updating Category Error");
                    }
                    else
                        await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"category: {updateVeiwEntity.CategoryName} was updated.");
                }
                else
                {
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Category Item {updateVeiwEntity.CategoryName} cannot be parent and child.", "Error updating");
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
                if (_updateWooCategoryMap.CanUpdate == updatedViewEntity.CanUpdateECommerceMap)
                {
                    // not necessary to display message.
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo Category Map for category: {updatedViewEntity.CategoryName} has not changed, so was not updated?");
                }
                else
                {
                    _updateWooCategoryMap.CanUpdate = (bool)updatedViewEntity.CanUpdateECommerceMap;
                    IRepository<WooCategoryMap> wooCategoryMapRepository = AppUnitOfWork.Repository<WooCategoryMap>();
                    _recsUpdated = await wooCategoryMapRepository.UpdateAsync(_updateWooCategoryMap);
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Category: {updatedViewEntity.CategoryName} was updated.");
                }
            }
            else
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Category Map for category: {updatedViewEntity.CategoryName} is no longer found, was it deleted?");

            await LoadAllViewItemsAsync();
            return _recsUpdated;
        }

        private async Task<WooCategoryMap> GetWooCategoryMapFromIdAsync(Guid? sourceWooEntityId)
        {
            if (sourceWooEntityId == null)
                return null;

            IRepository<WooCategoryMap> _wooCategoryMapRepo = AppUnitOfWork.Repository<WooCategoryMap>();
            WooCategoryMap _wooCategoryMap = await _wooCategoryMapRepo.GetByIdAsync(wcm => wcm.ItemCategoryLookupId == sourceWooEntityId);
            return _wooCategoryMap;
        }
        private async Task<int> DeleteWooCategoryLink(WooCategoryMap deleteWooCategoryMap)
        {
            IRepository<WooCategoryMap> _wooCategoryMapRepo = AppUnitOfWork.Repository<WooCategoryMap>();
            int _result = await _wooCategoryMapRepo.DeleteByEntityAsync(deleteWooCategoryMap);
            return _result;
        }
        private async Task<IWooProductCategory> GetIWooProductCategoryAsync()
        {
            //IAppRepository<WooSettings> _WooPrefs = AppUnitOfWork.Repository<WooSettings>();

            //WooSettings _wooSettings = await _WooPrefs.FindFirstAsync();
            //if (_wooSettings == null)
            //    return null;
            //WooAPISettings _wooAPISettings = new WooAPISettings(_wooSettings);
            WooAPISettings _wooAPISettings = await GetWooAPISettingsAsync();
            return new WooProductCategory(_wooAPISettings, AppLoggerManager);
        }
        private async Task<int> DeleteWooCategoryAsync(WooCategoryMap deleteWooCategoryMap)
        {
            int _result = UnitOfWork.CONST_WASERROR;  // if all goes well this will be change to the number of records returned

            IWooProductCategory _WooProductCategoryRepository = await GetIWooProductCategoryAsync();
            if (_WooProductCategoryRepository != null)
            {
                ProductCategory _tempWooCat = await _WooProductCategoryRepository.DeleteProductCategoryByIdAsync(deleteWooCategoryMap.WooCategoryId);
                _result = (_tempWooCat == null) ? UnitOfWork.CONST_WASERROR : Convert.ToInt32(_tempWooCat.id);  // return the id
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
            int _result = UnitOfWork.CONST_WASERROR;
            // delete the woo category
            WooCategoryMap _wooCategoryMap = await GetWooCategoryMapFromIdAsync(deleteWooEntityId);
            if (_wooCategoryMap == null)
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Category Id {deleteWooEntityId} was not found to have a Woo Category Map.");
            else
            {
                if (deleteFromWoo)
                {
                    _result = await DeleteWooCategoryAsync(_wooCategoryMap); ///Delete the category in Woo
                    if (_result == UnitOfWork.CONST_WASERROR)
                        await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was not deleted from Woo categories. Error {AppUnitOfWork.GetErrorMessage()}");
                    else
                        await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was deleted from Woo categories.");
                }
                _result = await DeleteWooCategoryLink(_wooCategoryMap);   //Delete our link data, if there was an error should we?
                if (_result == UnitOfWork.CONST_WASERROR)
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was not deleted from Woo linked categories. Error {AppUnitOfWork.GetErrorMessage()}");
                else
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was deleted from Woo linked categories.");
            }
            return _result;
        }
        private async Task<ProductCategory> AddItemToWooOnlyAsync(ItemCategoryLookup addEntity)
        {
            IWooProductCategory _wooProductCategoryRepository = await GetIWooProductCategoryAsync();
            if (_wooProductCategoryRepository == null)
                return null;

            ProductCategory _wooProductCategory = new()
            {
                name = addEntity.CategoryName,
                description = addEntity.CategoryName
            };

            if ((addEntity.ParentCategoryId != null) && (addEntity.ParentCategoryId != Guid.Empty))
            {
                WooCategoryMap _WooParentCategoryMap = await GetWooCategoryMapFromIdAsync(addEntity.ParentCategoryId);    // if they have a parent get the mapped id
                if (_WooParentCategoryMap != null)
                    _wooProductCategory.parent = (uint)_WooParentCategoryMap.WooCategoryId;
            }

            return await _wooProductCategoryRepository.AddProductCategoryAsync(_wooProductCategory);  // should return a new version with ID
        }
        private async Task<WooCategoryMap> MapOurItemToWooItemAsync(ProductCategory newWooProductCategory, ItemCategoryLookup addViewEntity)
        {
            // create a map to the woo category maps using the id and the category
            //
            IRepository<WooCategoryMap> _wooCategoryMapRepository = AppUnitOfWork.Repository<WooCategoryMap>();

            return await _wooCategoryMapRepository.AddAsync(new WooCategoryMap
            {
                ItemCategoryLookupId = addViewEntity.ItemCategoryLookupId,
                WooCategoryId = (uint)newWooProductCategory.id,
                WooCategoryName = addViewEntity.CategoryName,
                CanUpdate = true,
                WooCategoryParentId = (uint)newWooProductCategory.parent
            });
        }

        /// <summary>
        /// Add the item to woo
        /// </summary>
        /// <param name="addEntity"></param>
        /// <returns></returns>
        public override async Task<WooCategoryMap> AddWooItemAndMapAsync(ItemCategoryLookup addEntity)
        {
            // check it the item exists in woo (we did not do this as there is no such call;, if so get is id and return, otherwise add it and get its id
            //ProductCategory _wooProductCategory = await GetWooProductCategoryByName(addViewEntity.CategoryName);
            //if (_wooProductCategory == null)
            //{
            ProductCategory _wooProductCategory = await AddItemToWooOnlyAsync(addEntity);
            if (_wooProductCategory == null)
                return null;
            //}
            return await MapOurItemToWooItemAsync(_wooProductCategory, addEntity);
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
            if ((updateViewEntity.HasECommerceCategoryMap) && ((bool)(updateViewEntity.CanUpdateECommerceMap)))
            {
                IWooProductCategory _wooProductCategoryRepository = await GetIWooProductCategoryAsync();
                if (_wooProductCategoryRepository != null)                     //  - > if it does not exist then what?
                {
                    WooCategoryMap _updateWooMapEntity = await GetWooCategoryMapFromIdAsync(updateViewEntity.ItemCategoryLookupId);
                    if (_updateWooMapEntity == null)
                    {
                        // need to add the category -> this is done later.
                    }
                    else
                    {
                        ProductCategory _wooProductCategory = await _wooProductCategoryRepository.GetProductCategoryByIdAsync(_updateWooMapEntity.WooCategoryId);
                        if (_wooProductCategory == null)
                        {
                            return UnitOfWork.CONST_WASERROR;  /// oops what happened >?
                        }
                        else
                        {
                            // get id of parent
                            WooCategoryMap _ParentWooCategoryMap = await GetWooCategoryMapFromIdAsync(updateViewEntity.ParentCategoryId);
                            uint _newParentId = (_ParentWooCategoryMap == null) ? 0 : _ParentWooCategoryMap.WooCategoryId;
                            if ((!_wooProductCategory.name.Equals(updateViewEntity.CategoryName)) || ((_wooProductCategory.parent ?? 0) != _newParentId))
                            {
                                _wooProductCategory.name = updateViewEntity.CategoryName;  // only update if necessary
                                _wooProductCategory.parent = _newParentId;
                                var _res = ((await _wooProductCategoryRepository.UpdateProductCategoryAsync(_wooProductCategory)));
                                _result = ((_res == null) || (_res.id == null)) ? UnitOfWork.CONST_WASERROR : (int)_res.id; // if null there is an issue
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
        public override async Task<int> UpdateWooItemAndMappingAsync(ItemCategoryLookupView updateViewEntity)
        {
            int _result = await UpdateWooItemAsync(updateViewEntity);
            if (_result > 0)
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Updated woo category {updateViewEntity.CategoryName}.");
            else if (_result == UnitOfWork.CONST_WASERROR)
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo category {updateViewEntity.CategoryName} update failed, check WordPress firewall settings.");

            _result = await UpdateWooCategoryMapAsync(updateViewEntity);
            return _result;
        }
        #endregion
    }
}
