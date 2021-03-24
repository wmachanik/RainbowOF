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
    public class CategoryWooLinkedViewRepository : ICategoryWooLinkedView // IWooLinkedView<ItemCategoryLookup, ItemCategoryLookupView, WooCategoryMap>
    {

        private ILoggerManager _logger { get; set; }
        private IAppUnitOfWork _appUnitOfWork { get; set; }
        private GridSettings _gridSettings { get; set; }

        public CategoryWooLinkedViewRepository(ILoggerManager logger, IAppUnitOfWork appUnitOfWork, GridSettings gridSettings)    //: base(dbContext, logger, appUnitOfWork)
        {
            _logger = logger;
            _appUnitOfWork = appUnitOfWork;
            _gridSettings = gridSettings;
        }

        public async Task DeleteRowAsync(ItemCategoryLookupView deleteVeiwEntity)
        {
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _appUnitOfWork.Repository<ItemCategoryLookup>();

            var _recsDelete = await _ItemCategoryLookupRepository.DeleteByIdAsync(deleteVeiwEntity.ItemCategoryLookupId);     //DeleteByAsync(icl => icl.ItemCategoryLookupId == SelectedItemCategoryLookup.ItemCategoryLookupId);

            if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Category: {deleteVeiwEntity.CategoryName} is no longer found, was it deleted?");
            else
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category: {deleteVeiwEntity.CategoryName} was it deleted?");
        }
        async Task<int> UpdateWooCategoryMap(ItemCategoryLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooCategoryMap updateWooCategoryMap = await GetWooMappedItemAsync(updatedViewEntity.ItemCategoryLookupId);
            if (updateWooCategoryMap != null)
            {
                if (updateWooCategoryMap.CanUpdate == updatedViewEntity.CanUpdateWooMap)
                {
                    // not necessary to display message.
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo Category Map for category: {pUpdatedItem.CategoryName} has not changed, so was not updated?");
                }
                else
                {
                    updateWooCategoryMap.CanUpdate = (bool)updatedViewEntity.CanUpdateWooMap;
                    IAppRepository<WooCategoryMap> wooCategoryMapRepository = _appUnitOfWork.Repository<WooCategoryMap>();
                    _recsUpdated = await wooCategoryMapRepository.UpdateAsync(updateWooCategoryMap);
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category: {updatedViewEntity.CategoryName} was updated.");
                }
            }
            else
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Category Map for category: {updatedViewEntity.CategoryName} is no longer found, was it deleted?");

            return _recsUpdated;
        }
        public async Task<int> DoGroupActionAsync(ItemCategoryLookupView toVeiwEntity, BulkAction selectedAction)
        {
            if (selectedAction == BulkAction.AllowWooSync)
                toVeiwEntity.CanUpdateWooMap = true;
            else if (selectedAction == BulkAction.DisallowWooSync)
                toVeiwEntity.CanUpdateWooMap = false;
            return await UpdateWooCategoryMap(toVeiwEntity);

        }
        public async Task<List<ItemCategoryLookup>> GetAllItemsAsync()
        {
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _appUnitOfWork.Repository<ItemCategoryLookup>();
            List<ItemCategoryLookup> _ItemCategoryLookups = (await _ItemCategoryLookupRepository.GetAllEagerAsync(icl => icl.ParentCategory))
                .OrderBy(icl => icl.ParentCategoryId)
                .ThenBy(icl => icl.CategoryName).ToList();

            return _ItemCategoryLookups;
        }
        // used to the Push and Pop
        List<ItemCategoryLookupView> _oldSelectedItems = null;
        public void PushSelectedItems(List<ItemCategoryLookupView> currentSelectedItems)
        {
            if (currentSelectedItems != null)
            {
                _oldSelectedItems = new List<ItemCategoryLookupView>(currentSelectedItems);
                currentSelectedItems.Clear(); // the refresh does this
            }
            else if (_oldSelectedItems != null)  // current selection is null
                _oldSelectedItems.Clear();  // note that nothing was selected
        }

        public List<ItemCategoryLookupView> PopSelectedItems(List<ItemCategoryLookupView> modelViewItems)
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
        public async Task<WooCategoryMap> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _appUnitOfWork.Repository<WooCategoryMap>();

            return await _wooCategoryMapRepository.FindFirstAsync(wcm => wcm.ItemCategoryLookupId == mapWooEntityID);
        }

        public async Task<bool> IsDuplicate(ItemCategoryLookup targetEntity)
        {
            // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _appUnitOfWork.Repository<ItemCategoryLookup>();
            var _exists = (await _ItemCategoryLookupRepository.GetByAsync(ml => ml.CategoryName == targetEntity.CategoryName)).ToList();
            return ((_exists != null) && (_exists.Count > 1));
        }

        public async Task<List<ItemCategoryLookupView>> LoadAllViewItems()
        {
            List<ItemCategoryLookup> _itemCategoryLookups = await GetAllItemsAsync();

            List<ItemCategoryLookupView> _itemCategoryViewLookups = new List<ItemCategoryLookupView>();
            WooCategoryMap wooCategoryMap;
            // Map Items to Woo CategoryMap
            foreach (var itemCat in _itemCategoryLookups)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                wooCategoryMap = await GetWooMappedItemAsync(itemCat.ItemCategoryLookupId);

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
        public bool IsValid(ItemCategoryLookup checkEntity)
        {
            return (checkEntity.ParentCategoryId != checkEntity.ItemCategoryLookupId);
        }
        public void NewItemDefaultSetter(ItemCategoryLookupView newViewEntity)
        {
            if (newViewEntity == null)
                newViewEntity = new ItemCategoryLookupView();

            newViewEntity.CategoryName = "Cat name (must be unique)";
            newViewEntity.Notes = $"Added {DateTime.Now.Date}";
            newViewEntity.ParentCategoryId = Guid.Empty;
            newViewEntity.UsedForPrediction = true;
            newViewEntity.CanUpdateWooMap = false;

            //return _newVeiwEntity;
        }

        public ItemCategoryLookup GetItemFromView(ItemCategoryLookupView fromVeiwEntity)
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
        public async Task<int> UpdateItemAsync(ItemCategoryLookup updateItem)
        {
            int _recsUpdted = 0;
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _appUnitOfWork.Repository<ItemCategoryLookup>();
            // first check it exists - it could have been deleted 
            ItemCategoryLookup pUpdatedLookup = await _ItemCategoryLookupRepository.GetByIdAsync(updateItem.ItemCategoryLookupId);
            if (pUpdatedLookup == null)
            {
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Category: {updateItem.CategoryName} is no longer found, was it deleted?");
                return AppUnitOfWork.CONST_WASERROR;
            }
            else
            {
                pUpdatedLookup.CategoryName = updateItem.CategoryName;
                pUpdatedLookup.ParentCategoryId = (updateItem.ParentCategoryId == Guid.Empty) ? null : updateItem.ParentCategoryId;
                pUpdatedLookup.UsedForPrediction = updateItem.UsedForPrediction;
                pUpdatedLookup.Notes = updateItem.Notes;
                _recsUpdted = await _ItemCategoryLookupRepository.UpdateAsync(pUpdatedLookup);
                if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateItem.CategoryName} - {_appUnitOfWork.GetErrorMessage()}", "Error adding Category");
            }
            return _recsUpdted;
        }
        public async Task InsertRowAsync(ItemCategoryLookupView newVeiwEntity)
        {
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _appUnitOfWork.Repository<ItemCategoryLookup>();
            // first check we do not already have a category like this.
            if (await _ItemCategoryLookupRepository.FindFirstAsync(icl => icl.CategoryName == newVeiwEntity.CategoryName) == null)
            {
                ItemCategoryLookup _NewItemCategoryLookup = GetItemFromView(newVeiwEntity); // store this here since when it is added it will automatically update the id field
                int _recsAdded = await _ItemCategoryLookupRepository.AddAsync(_NewItemCategoryLookup);
                if (_recsAdded != AppUnitOfWork.CONST_WASERROR)
                {
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.CategoryName} - added", "Category Added");
                    if (newVeiwEntity.CanUpdateWooMap ?? false)
                    {
                        // they selected to update woo so add to Woo
                        if (await AddWooItemAsync(_NewItemCategoryLookup) == AppUnitOfWork.CONST_WASERROR)   // add if they select to update
                            _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding {newVeiwEntity.CategoryName} to Woo - {_appUnitOfWork.GetErrorMessage()}", "Error adding Woo Category");
                        else
                            _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.CategoryName} - added to Woo", "Woo Category Added");
                    }
                }
                else
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.CategoryName} - {_appUnitOfWork.GetErrorMessage()}", "Error adding Category");
            }
            else
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.CategoryName} already exists, so could not be added.");
            await LoadAllViewItems();   // reload the list so the latest item is displayed
        }

        public async Task UpdateRowAsync(ItemCategoryLookupView updateVeiwEntity)
        {
            if (await IsDuplicate(updateVeiwEntity))
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Category Name: {updateVeiwEntity.CategoryName} - already exists, cannot be updated", "Exists already");
            else
            {
                if (IsValid(updateVeiwEntity))
                {
                    ItemCategoryLookup updatedItemCategoryLookup = GetItemFromView(updateVeiwEntity);
                    // update and check for errors 
                    if (await UpdateItemAsync(updateVeiwEntity) != AppUnitOfWork.CONST_WASERROR)
                    {
                        if ((updateVeiwEntity.HasWooCategoryMap) && (await UpdateWooCategoryMap(updateVeiwEntity) == AppUnitOfWork.CONST_WASERROR))
                            _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"WooCategory map for Item: {updateVeiwEntity.CategoryName} - {_appUnitOfWork.GetErrorMessage()}", "Error updating");
                        //else
                        //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category: {updatedItem.CategoryName} was updated.");
                    }
                }
                else
                {
                    string pMessage = $"Category Item {updateVeiwEntity.CategoryName} cannot be parent and child.";
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, pMessage, "Error updating");
                }
            }
            await LoadAllViewItems();   // reload the list so the latest item is displayed
        }
        public async Task<int> UpdateWooMappingAsync(ItemCategoryLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooCategoryMap updateWooCategoryMap = await GetWooMappedItemAsync(updatedViewEntity.ItemCategoryLookupId);
            if (updateWooCategoryMap != null)
            {
                if (updateWooCategoryMap.CanUpdate == updatedViewEntity.CanUpdateWooMap)
                {
                    // not necessary to display message.
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo Category Map for category: {updatedViewEntity.CategoryName} has not changed, so was not updated?");
                }
                else
                {
                    updateWooCategoryMap.CanUpdate = (bool)updatedViewEntity.CanUpdateWooMap;
                    IAppRepository<WooCategoryMap> wooCategoryMapRepository = _appUnitOfWork.Repository<WooCategoryMap>();
                    _recsUpdated = await wooCategoryMapRepository.UpdateAsync(updateWooCategoryMap);
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category: {updatedViewEntity.CategoryName} was updated.");
                }
            }
            else
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Category Map for category: {updatedViewEntity.CategoryName} is no longer found, was it deleted?");

            return _recsUpdated;
        }

        private async Task<WooCategoryMap> GetWooCategoryMapFromID(Guid sourceWooEntityId)
        {
            IAppRepository<WooCategoryMap> _wooCategoryMapRepo = _appUnitOfWork.Repository<WooCategoryMap>();
            WooCategoryMap _wooCategoryMap = await _wooCategoryMapRepo.GetByIdAsync(sourceWooEntityId);
            return _wooCategoryMap;
        }
        private async Task<int> DeleteWooCategoryLink(WooCategoryMap deleteWooCategoryMap)
        {
            int _result = 0;
            IAppRepository<WooCategoryMap> _wooCategoryMapRepo = _appUnitOfWork.Repository<WooCategoryMap>();

            _result = await _wooCategoryMapRepo.DeleteByIdAsync(deleteWooCategoryMap.WooCategoryMapId);

            return _result;
        }
        private async Task<IWooProductCategory> GetIWooProductCategory()
        {
            IAppRepository<WooSettings> _WooPrefs = _appUnitOfWork.Repository<WooSettings>();

            WooSettings _wooSettings = await _WooPrefs.FindFirstAsync();
            if (_wooSettings == null)
                return null;
            WooAPISettings _wooAPISettings = new WooAPISettings(_wooSettings);
            return new WooProductCategory(_wooAPISettings, _logger);
        }
        private async Task<int> DeleteWooCategory(WooCategoryMap deleteWooCategoryMap)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;  // if all goes well this will be change to the number of records returned

            IWooProductCategory _WooProductCategoryRepository = await GetIWooProductCategory();
            if (_WooProductCategoryRepository != null)
            {
                _result = (int)(await _WooProductCategoryRepository.DeleteProductCategoryByIdAsync(deleteWooCategoryMap.WooCategoryId)).id;  // return the id
            }
            return _result;
        }
        public async Task<int> DeleteWooItemAsync(Guid deleteWooEntityId)
        {
/*
 * 
 * 
 * We need to rethink this. We need to delete from Woo first then from us, since the database integrity with the woolookuplink will delete the item first.
 * 
 * 
 */
            int _result = AppUnitOfWork.CONST_WASERROR;
            // delete the woo category
            WooCategoryMap _wooCategoryMap = await GetWooCategoryMapFromID(deleteWooEntityId);
            if (_wooCategoryMap == null)
                _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Category Id {deleteWooEntityId} was not found to have a Woo Category Map.");
            else
            {
                _result = await DeleteWooCategoryLink(_wooCategoryMap);
                if (_result == AppUnitOfWork.CONST_WASERROR)
                    _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was not deleted from Woo linked categories.");
                else
                {
                    _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was deleted from Woo linked categories.");
                    _result = await DeleteWooCategory(_wooCategoryMap);
                    if (_result == AppUnitOfWork.CONST_WASERROR)
                        _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was not deleted from Woo categories.");
                    else
                        _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Category Id {_wooCategoryMap.WooCategoryId} was deleted from Woo categories.");
                }
            }
            return _result;
        }
        private async Task<ProductCategory> AddItemToWoo(ItemCategoryLookup addEntity)
        {
            IWooProductCategory _WooProductCategoryRepository = await GetIWooProductCategory();
            if (_WooProductCategoryRepository == null)
                return null;

            ProductCategory _wooProductCategory = new ProductCategory
            {
                name = addEntity.CategoryName,
                description = addEntity.CategoryName
            };

            if ((addEntity.ParentCategoryId != null) && (addEntity.ParentCategoryId != Guid.Empty))
            {
                WooCategoryMap _WooParentCategoryMap = await GetWooCategoryMapFromID((Guid)addEntity.ParentCategoryId);    // if they have a parent get the mapped id
                if (_WooParentCategoryMap != null)
                    _wooProductCategory.parent = _WooParentCategoryMap.WooCategoryId;
            }

            return await _WooProductCategoryRepository.AddProductCategoryAsync(_wooProductCategory);  // should return a new version with ID

        }
        private async Task<int> MapOurItemToWooItem(int wooItemId, ItemCategoryLookup addViewEntity)
        {
            // create a map to the woo category maps using the id and the category
            //
            IAppRepository<WooCategoryMap> _WooCategoryMapRepository = _appUnitOfWork.Repository<WooCategoryMap>();

            return await _WooCategoryMapRepository.AddAsync(new WooCategoryMap
            {
                ItemCategoryLookupId = addViewEntity.ItemCategoryLookupId,
                WooCategoryId = wooItemId,
                WooCategoryName = addViewEntity.CategoryName,
                CanUpdate = true,
            });

        }

        /// <summary>
        /// Add the item to woo
        /// </summary>
        /// <param name="addEntity"></param>
        /// <returns></returns>
        public async Task<int> AddWooItemAsync(ItemCategoryLookup addEntity)
        {
            // check it the item exists in woo (we did not do this as there is no such call;, if so get is id and return, otherwise add it and get its id

            //ProductCategory _wooProductCategory = await GetWooProductCategoryByName(addViewEntity.CategoryName);
            //if (_wooProductCategory == null)
            //{
            ProductCategory _wooProductCategory = await AddItemToWoo(addEntity);
            if (_wooProductCategory == null)
                return AppUnitOfWork.CONST_WASERROR;
            //}
            return await MapOurItemToWooItem(_wooProductCategory.id ?? 0, addEntity);
        }

        //private async Task<ProductCategory> GetWooProductCategoryByName(string categoryName)
        //{
        //    IWooProductCategory _WooProductCategoryRepository = await GetIWooProductCategory();
        //    if (_WooProductCategoryRepository == null)
        //        return null;

        //    return _WooProductCategoryRepository.FindProductCategoryByName(categoryName);
        //}

        public async Task<int> UpdateWooItemAsync(ItemCategoryLookupView updateViewEntity, WooCategoryMap updateWooMapEntity)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;  /// oops what happened >?

            IWooProductCategory _WooProductCategoryRepository = await GetIWooProductCategory();
            if (_WooProductCategoryRepository != null)
            {
                //  - > if it does not exist then what?
                ProductCategory _WooProductCategory = await _WooProductCategoryRepository.GetProductCategoryByIdAsync(updateWooMapEntity.WooCategoryId);
                if (_WooProductCategory == null)
                {
                    return AppUnitOfWork.CONST_WASERROR;  /// oops what happened >?
                }
                else
                {
                    if (!_WooProductCategory.name.Equals(updateViewEntity.CategoryName))
                    {
                        _WooProductCategory.name = updateViewEntity.CategoryName;  // only update if necessary
                        _result = (int)((await _WooProductCategoryRepository.UpdateProductCategoryAsync(_WooProductCategory)).id);
                    }
                }
            }
            return _result;
        }
    }
}
