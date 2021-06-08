using Blazorise.DataGrid;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Lookups;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Common;
using RainbowOF.ViewModels.Items;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Repositories.Items
{
    public class ItemWooLinkedViewRepository : WooLinkedView<Item, ItemView, WooProductMap>, IItemWooLinkedView
    {
        public ItemWooLinkedViewRepository(ILoggerManager logger, IAppUnitOfWork appUnitOfWork, GridSettings gridSettings) : base(logger, appUnitOfWork, gridSettings)
        {
            //_logger = logger;
            //_appUnitOfWork = appUnitOfWork;
            //_gridSettings = gridSettings;
        }

        public override async Task DeleteRowAsync(ItemView deleteViewEntity)
        {
            IAppRepository<Item> _ItemRepository = _appUnitOfWork.Repository<Item>();

            var _recsDelete = await _ItemRepository.DeleteByIdAsync(deleteViewEntity.ItemId);

            if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Item: {deleteViewEntity.ItemName} is no longer found, was it deleted?");
            else
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {deleteViewEntity.ItemName} was it deleted?");
        }
        async Task<int> UpdateWooProductMap(ItemView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooProductMap _updateWooProductMap = await GetWooMappedItemAsync(updatedViewEntity.ItemId);
            if (_updateWooProductMap != null)
            {
                if (_updateWooProductMap.CanUpdate == updatedViewEntity.CanUpdateWooMap)
                {
                }
                else
                {
                    _updateWooProductMap.CanUpdate = (bool)updatedViewEntity.CanUpdateWooMap;
                    IAppRepository<WooProductMap> WooProductRepository = _appUnitOfWork.Repository<WooProductMap>();
                    _recsUpdated = await WooProductRepository.UpdateAsync(_updateWooProductMap);
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {updatedViewEntity.ItemName} was updated.");
                }
            }
            else
            {
                // nothing was found, so this probably means they now want to add. We should had a Pop up to check
                int _result = await AddWooItemAndMapAsync(GetItemFromView(updatedViewEntity));
                if (_result != AppUnitOfWork.CONST_WASERROR)
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute {updatedViewEntity.ItemName} has been added to woo?");
            }

            return _recsUpdated;
        }
        public override async Task<int> DoGroupActionAsync(ItemView toVeiwEntity, BulkAction selectedAction)
        {
            if (selectedAction == BulkAction.AllowWooSync)
                toVeiwEntity.CanUpdateWooMap = true;
            else if (selectedAction == BulkAction.DisallowWooSync)
                toVeiwEntity.CanUpdateWooMap = false;
            return await UpdateWooProductMap(toVeiwEntity);
        }

        List<ItemView> _oldSelectedItems = null;
        public override void PushSelectedItems(List<ItemView> currentSelectedItems)
        {
            if (currentSelectedItems != null)
            {
                _oldSelectedItems = new List<ItemView>(currentSelectedItems);
                currentSelectedItems.Clear(); // the refresh does this
            }
            else if (_oldSelectedItems != null)  // current selection is null
                _oldSelectedItems.Clear();  // note that nothing was selected
        }

        public override List<ItemView> PopSelectedItems(List<ItemView> modelViewItems)
        {
            List<ItemView> _popList = null;
            if (_oldSelectedItems != null)
            {
                _popList = new List<ItemView>();
                foreach (var item in _oldSelectedItems)
                {
                    var _oldSelectdItem = modelViewItems.Where(ial => ial.ItemId == item.ItemId).FirstOrDefault();
                    if (_oldSelectdItem != null)  // if it was deleted this will be the case
                        _popList.Add(_oldSelectdItem);
                }
            }
            return _popList;
        }
        public override async Task<WooProductMap> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            IAppRepository<WooProductMap> _wooAttributeMapRepository = _appUnitOfWork.Repository<WooProductMap>();

            return await _wooAttributeMapRepository.FindFirstAsync(wcm => wcm.ItemId == mapWooEntityID);
        }
        public override async Task<List<WooProductMap>> GetWooMappedItemsAsync(List<Guid> mapWooEntityIDs)
        {
            IAppRepository<WooProductMap> _WooProductMapRepository = _appUnitOfWork.Repository<WooProductMap>();

            var _WooMappedItems = await _WooProductMapRepository.GetByAsync(wam => mapWooEntityIDs.Contains(wam.ItemId));   // ItemId
            return _WooMappedItems.ToList();
        }
        public override async Task<bool> IsDuplicate(Item targetEntity)
        {
            // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
            IAppRepository<Item> _ItemRepository = _appUnitOfWork.Repository<Item>();
            var _exists = (await _ItemRepository.GetByAsync(itm => itm.ItemName == targetEntity.ItemName)).ToList();
            return ((_exists != null) && (_exists.Count > 1));
        }
        public override async Task<List<Item>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters)    //int startPage, int currentPageSize)
        {
            IItemRepository _ItemRepository = _appUnitOfWork.itemRepository();
            DataGridItems<Item> _dataGridItems = await _ItemRepository.GetPagedDataEagerWithFilterAndOrderByAsync(currentDataGridParameters);
            List<Item> _Items = _dataGridItems.Entities.ToList();
            _gridSettings.TotalItems = _dataGridItems.TotalRecordCount;
            return _Items;
        }
        public override DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<ItemView> inputDataGridReadData, string inputCustomerFilter)
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
        public override async Task<List<ItemView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters)
        {
            List<Item> _Items = await GetPagedItemsAsync(currentDataGridParameters);
            List<ItemView> _itemViewLookups = new List<ItemView>();
            // Get a list of all the Attribute maps that exists
            List<Guid> _ItemAttribIds = _Items.Where(it => it.ItemId != Guid.Empty).Select(it => it.ItemId).ToList();   // get all the ids selected
            // Get all related ids
            List<WooProductMap> WooProductMaps = await GetWooMappedItemsAsync(_ItemAttribIds);
            // Map Items to Woo AttributeMap
            foreach (var entity in _Items)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                WooProductMap _WooProductMap = WooProductMaps.Where(wam => wam.ItemId == entity.ItemId).FirstOrDefault();    //  if retrieving / record await GetWooMappedItemAsync(itemCat.ItemId);

                _itemViewLookups.Add(new ItemView
                {
                    ItemId = entity.ItemId,
                    ItemName = entity.ItemName,
                    SKU = entity.SKU,
                    IsEnabled = entity.IsEnabled,
                    ItemDetail = entity.ItemDetail,
                    PrimaryItemCategoryLookupId = ((entity.PrimaryItemCategoryLookupId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.PrimaryItemCategoryLookupId,
                    ParentItemId = ((entity.ParentItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.ParentItemId,
                    ReplacementItemId = ((entity.ReplacementItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.ReplacementItemId,
                    ItemAbbreviatedName = entity.ItemAbbreviatedName,
                    SortOrder = entity.SortOrder,
                    BasePrice = entity.BasePrice,
                    CanUpdateWooMap = (_WooProductMap == null) ? null : _WooProductMap.CanUpdate
                }); ;
            }
            return _itemViewLookups;
        }
        public override bool IsValid(Item checkEntity)
        {
            return !string.IsNullOrWhiteSpace(checkEntity.ItemName); // (checkEntity.ParentAttributeId != checkEntity.ItemId);
        }
        public override ItemView NewItemDefaultSetter(ItemView newViewEntity)
        {
            if (newViewEntity == null)
                newViewEntity = new ItemView();

            newViewEntity.ItemName = "Item (must be unique)";
            newViewEntity.ItemName = "Item name (must be unique)";
            newViewEntity.SKU = "SKU (must be unique)";
            newViewEntity.IsEnabled = true;
            newViewEntity.ItemDetail = "Item description / detail";
            newViewEntity.ItemAbbreviatedName = "Itm!";
            newViewEntity.SortOrder = 0;
            newViewEntity.BasePrice = decimal.Zero;
            newViewEntity.CanUpdateWooMap = null;
            return newViewEntity;
        }

        public override Item GetItemFromView(ItemView fromVeiwEntity)
        {
            Item _newItem = new Item
            {
                ItemId = fromVeiwEntity.ItemId,
                ItemName = fromVeiwEntity.ItemName,
                SKU = fromVeiwEntity.SKU,
                IsEnabled = fromVeiwEntity.IsEnabled,
                ItemDetail = fromVeiwEntity.ItemDetail,
                PrimaryItemCategoryLookupId = ((fromVeiwEntity.PrimaryItemCategoryLookupId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)fromVeiwEntity.PrimaryItemCategoryLookupId,
                ReplacementItemId = ((fromVeiwEntity.ReplacementItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)fromVeiwEntity.ReplacementItemId,
                ItemAbbreviatedName = fromVeiwEntity.ItemAbbreviatedName,
                SortOrder = fromVeiwEntity.SortOrder,
                BasePrice = fromVeiwEntity.BasePrice,
                ParentItemId = ((fromVeiwEntity.ParentItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)fromVeiwEntity.ParentItemId,
                ////????? Lists and other lazy loads?
            };

            return _newItem;
        }
        public override async Task<int> UpdateItemAsync(ItemView updateViewItem)
        {
            int _recsUpdted = 0;
            IAppRepository<Item> _ItemRepository = _appUnitOfWork.Repository<Item>();
            // first check it exists - it could have been deleted 
            Item _CurrentItem = await _ItemRepository.GetByIdAsync(updateViewItem.ItemId);
            if (_CurrentItem == null)
            {
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Item: {updateViewItem.ItemName} is no longer found, was it deleted?");
                return AppUnitOfWork.CONST_WASERROR;
            }
            else
            {
                _CurrentItem.ItemName = updateViewItem.ItemName;
                _CurrentItem.ItemName = updateViewItem.ItemName;
                _CurrentItem.SKU = updateViewItem.SKU;
                _CurrentItem.IsEnabled = updateViewItem.IsEnabled;
                _CurrentItem.ItemDetail = updateViewItem.ItemDetail;
                _CurrentItem.PrimaryItemCategoryLookupId = ((updateViewItem.PrimaryItemCategoryLookupId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)updateViewItem.PrimaryItemCategoryLookupId;
                _CurrentItem.ReplacementItemId = ((updateViewItem.ReplacementItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)updateViewItem.ReplacementItemId;
                _CurrentItem.ItemAbbreviatedName = updateViewItem.ItemAbbreviatedName;
                _CurrentItem.SortOrder = updateViewItem.SortOrder;
                _CurrentItem.BasePrice = updateViewItem.BasePrice;
                _CurrentItem.ParentItemId = ((updateViewItem.ParentItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)updateViewItem.ParentItemId;
                ////????? Lists and other lazy loads?

                _recsUpdted = await _ItemRepository.UpdateAsync(_CurrentItem);
                if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.ItemName} - {_appUnitOfWork.GetErrorMessage()}", "Error updating Attribute");
                if (await UpdateWooItemAndMapping(updateViewItem) == AppUnitOfWork.CONST_WASERROR)
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.ItemName} - {_appUnitOfWork.GetErrorMessage()}", "Error updating Attribute Map");   // should we send a message here error = mapping not updated 

            }
            return _recsUpdted;
        }
        public override async Task InsertRowAsync(ItemView newVeiwEntity)
        {
            IAppRepository<Item> _ItemRepository = _appUnitOfWork.Repository<Item>();
            // first check we do not already have a Attribute like this.
            if (await _ItemRepository.FindFirstAsync(ial => ial.ItemName == newVeiwEntity.ItemName) == null)
            {
                Item _NewItem = GetItemFromView(newVeiwEntity); // store this here since when it is added it will automatically update the id field
                int _recsAdded = await _ItemRepository.AddAsync(_NewItem);
                if (_recsAdded != AppUnitOfWork.CONST_WASERROR)
                {
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.ItemName} - added", "Attribute Added");
                    if (newVeiwEntity.CanUpdateWooMap ?? false)
                    {
                        // they selected to update woo so add to Woo
                        if (await AddWooItemAndMapAsync(_NewItem) == AppUnitOfWork.CONST_WASERROR)   // add if they select to update
                            _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding {newVeiwEntity.ItemName} to Woo - {_appUnitOfWork.GetErrorMessage()}", "Error adding Woo Attribute");
                        else
                            _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.ItemName} - added to Woo", "Woo Attribute Added");
                    }
                }
                else
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.ItemName} - {_appUnitOfWork.GetErrorMessage()}", "Error adding Attribute");
            }
            else
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.ItemName} already exists, so could not be added.");
            //-> done in parent       await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }

        public override async Task UpdateRowAsync(ItemView updateVeiwEntity)
        {
            if (await IsDuplicate(updateVeiwEntity))
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Name: {updateVeiwEntity.ItemName} - already exists, cannot be updated", "Exists already");
            else
            {
                if (IsValid(updateVeiwEntity))
                {
                    // update and check for errors 
                    if (await UpdateItemAsync(updateVeiwEntity) == AppUnitOfWork.CONST_WASERROR)
                    {
                        _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating Attribute: {updateVeiwEntity.ItemName} -  {_appUnitOfWork.GetErrorMessage()}", "Updating Attribute Error");
                    }
                    else
                        _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {updateVeiwEntity.ItemName} was updated.");
                }
                else
                {
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Item {updateVeiwEntity.ItemName} cannot be parent and child.", "Error updating");
                }
            }
            //-> done in parent await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }
        public override async Task<int> UpdateWooMappingAsync(ItemView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooProductMap updateWooProductMap = await GetWooMappedItemAsync(updatedViewEntity.ItemId);
            if (updateWooProductMap != null)
            {
                if (updateWooProductMap.CanUpdate == updatedViewEntity.CanUpdateWooMap)
                {
                    // not necessary to display message.
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo Attribute Map for Attribute: {updatedViewEntity.ItemName} has not changed, so was not updated?");
                }
                else
                {
                    updateWooProductMap.CanUpdate = (bool)updatedViewEntity.CanUpdateWooMap;
                    IAppRepository<WooProductMap> WooProductMapRepository = _appUnitOfWork.Repository<WooProductMap>();
                    _recsUpdated = await WooProductMapRepository.UpdateAsync(updateWooProductMap);
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {updatedViewEntity.ItemName} was updated.");
                }
            }
            else
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute Map for Attribute: {updatedViewEntity.ItemName} is no longer found, was it deleted?");

            await LoadAllViewItemsAsync();
            return _recsUpdated;
        }

        private async Task<WooProductMap> GetWooProductMapFromID(Guid? sourceWooEntityId)
        {
            if (sourceWooEntityId == null)
                return null;

            IAppRepository<WooProductMap> _WooProductMapRepo = _appUnitOfWork.Repository<WooProductMap>();
            WooProductMap _WooProductMap = await _WooProductMapRepo.FindFirstAsync(wam => wam.ItemId == sourceWooEntityId);
            return _WooProductMap;
        }
        private async Task<int> DeleteWooAttributeLink(WooProductMap deleteWooProductMap)
        {
            int _result = 0;
            IAppRepository<WooProductMap> _WooProductMapRepo = _appUnitOfWork.Repository<WooProductMap>();

            _result = await _WooProductMapRepo.DeleteByIdAsync(deleteWooProductMap.WooProductMapId);

            return _result;
        }
        private async Task<IWooProduct> GetIWooProduct()
        {
            WooAPISettings _wooAPISettings = await GetWooAPISettingsAsync();
            return new WooProduct(_wooAPISettings, _logger);
        }
        private async Task<int> DeleteWooAttribute(WooProductMap deleteWooProductMap)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;  // if all goes well this will be change to the number of records returned

            IWooProduct _WooProductRepository = await GetIWooProduct();
            if (_WooProductRepository != null)
            {
                Product _TempWooCat = await _WooProductRepository.DeleteProductByIdAsync(deleteWooProductMap.WooProductId);
                _result = (_TempWooCat == null) ? AppUnitOfWork.CONST_WASERROR : Convert.ToInt32(_TempWooCat.id);  // return the id
            }
            return _result;
        }
        /// <summary>
        /// Delete a Woo Attribute from the mapped table and Woo, if they ask us to 
        /// </summary>
        /// <param name="WooEntityId">Id to delete</param>
        /// <param name="deleteFromWoo">True of we want to delete from Woo too</param>
        /// <returns></returns>
        public override async Task<int> DeleteWooItemAsync(Guid deleteWooEntityId, bool deleteFromWoo)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;
            // delete the woo Attribute
            WooProductMap _WooProductMap = await GetWooProductMapFromID(deleteWooEntityId);
            if (_WooProductMap == null)
                _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {deleteWooEntityId} was not found to have a Woo Attribute Map.");
            else
            {
                if (deleteFromWoo)
                {
                    _result = await DeleteWooAttribute(_WooProductMap); ///Delete the Attribute in Woo
                    if (_result == AppUnitOfWork.CONST_WASERROR)
                        _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {_WooProductMap.WooProductId} was not deleted from Woo categories. Error {_appUnitOfWork.GetErrorMessage()}");
                    else
                        _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Id {_WooProductMap.WooProductId} was deleted from Woo categories.");
                }
                _result = await DeleteWooAttributeLink(_WooProductMap);   //Delete our link data, if there was an error should we?
                if (_result == AppUnitOfWork.CONST_WASERROR)
                    _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {_WooProductMap.WooProductId} was not deleted from Woo linked categories. Error {_appUnitOfWork.GetErrorMessage()}");
                else
                    _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Id {_WooProductMap.WooProductId} was deleted from Woo linked categories.");
            }
            return _result;
        }
        private Dictionary<OrderBys, string> AttriburteOrderByToWooOrerBy = new Dictionary<OrderBys, string>
        {
            {OrderBys.Id, "id" },
            {OrderBys.Name, "name" },
            {OrderBys.None,"menu_order" },
            {OrderBys.Number,"name_num" },
            {OrderBys.SortOrder,"menu_order"  }
        };
        private string ConvertToWooOrderBy(OrderBys inputValue)
        {
            return AttriburteOrderByToWooOrerBy[inputValue];  // return the string equivalent of the enum OrderBys
        }
        private async Task<Product> AddItemToWooOnlySync(Item addEntity)
        {
            IWooProduct _WooProductRepository = await GetIWooProduct();
            if (_WooProductRepository == null)
                return null;

            Product _wooProduct = new Product
            {
                name = addEntity.ItemName,
                //order_by = ConvertToWooOrderBy(addEntity.OrderBy),
            };
            return await _WooProductRepository.AddProductAsync(_wooProduct);  // should return a new version with ID
        }

        private async Task<int> MapOurItemToWooItemSync(Product newWooProduct, Item addViewEntity)
        {
            // create a map to the woo Attribute maps using the id and the Attribute
            //
            IAppRepository<WooProductMap> _WooProductMapRepository = _appUnitOfWork.Repository<WooProductMap>();

            return await _WooProductMapRepository.AddAsync(new WooProductMap
            {
                ItemId = addViewEntity.ItemId,
                WooProductId = Convert.ToInt32(newWooProduct.id),
                CanUpdate = true,
            });
        }
        //private async Task<Product> GetWooProductByName(string ItemName)
        //{
        //    IWooProduct _WooProductRepository = await GetIWooProduct();
        //    if (_WooProductRepository == null)
        //        return null;

        //    return _WooProductRepository.FindProductByName(ItemName);
        //}

        /// <summary>
        /// Add the item to woo
        /// </summary>
        /// <param name="addEntity"></param>
        /// <returns></returns>
        public override async Task<int> AddWooItemAndMapAsync(Item addEntity)
        {
            // check it the item exists in woo !!!!!!(we did not do this as there is no such call;, if so get is id and return, otherwise add it and get its id

            //Product _wooProduct = await GetWooProductByName(addEntity.ItemName);
            //if (_wooProduct == null)
            //{
            Product _wooProduct = await AddItemToWooOnlySync(addEntity);
            if (_wooProduct == null)
                return AppUnitOfWork.CONST_WASERROR;
            return await MapOurItemToWooItemSync(_wooProduct, addEntity);
            //}
            //else
            //    return AppUnitOfWork.CONST_WASERROR;
        }
        public override async Task<int> UpdateWooItemAsync(ItemView updateViewEntity)
        {
            int _result = 0;  /// null or not found
            if ((updateViewEntity.HasWooAttributeMap) && ((bool)(updateViewEntity.CanUpdateWooMap)))
            {
                IWooProduct _WooProductRepository = await GetIWooProduct();
                if (_WooProductRepository != null)                     //  - > if it does not exist then what?
                {
                    WooProductMap _updateWooMapEntity = await GetWooProductMapFromID(updateViewEntity.ItemId);
                    if (_updateWooMapEntity == null)
                    {
                        // need to add the Attribute -> this is done later.
                        _result = await AddWooItemAndMapAsync(updateViewEntity);
                    }
                    else
                    {
                        Product _WooProduct = await _WooProductRepository.GetProductByIdAsync(_updateWooMapEntity.WooProductId);
                        if (_WooProduct == null)
                        {
                            return AppUnitOfWork.CONST_WASERROR;  /// oops what happened >?
                        }
                        else
                        {
                            // only update if different
                            if ((!_WooProduct.name.Equals(updateViewEntity.ItemName))) //|| (!_WooProduct.order_by.Equals(updateViewEntity.OrderBy.ToString(), StringComparison.OrdinalIgnoreCase)))
                            {
                                _WooProduct.name = updateViewEntity.ItemName;  // only update if necessary
                                //_WooProduct.order_by = ConvertToWooOrderBy(updateViewEntity.OrderBy);
                                var _res = ((await _WooProductRepository.UpdateProductAsync(_WooProduct)));
                                _result = ((_res == null) || (_res.id == null)) ? AppUnitOfWork.CONST_WASERROR : (int)_res.id; // if null there is an issue
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
        public override async Task<int> UpdateWooItemAndMapping(ItemView updateViewEntity)
        {
            int _result = await UpdateWooItemAsync(updateViewEntity);
            if (_result > 0)
                _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Updated woo Attribute {updateViewEntity.ItemName}.");
            else if (_result == AppUnitOfWork.CONST_WASERROR)
                _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute {updateViewEntity.ItemName} update failed.");

            _result = await UpdateWooProductMap(updateViewEntity);
            return _result;
        }
    }
}
