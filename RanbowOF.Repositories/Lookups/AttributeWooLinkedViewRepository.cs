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
    public class AttributeWooLinkedViewRepository : WooLinkedView<ItemAttributeLookup, ItemAttributeLookupView, WooProductAttributeMap>, ItemWooLinkedView
    {
        public AttributeWooLinkedViewRepository(ILoggerManager logger, IAppUnitOfWork appUnitOfWork, GridSettings gridSettings) : base(logger, appUnitOfWork, gridSettings)
        {
            //_logger = logger;
            //_appUnitOfWork = appUnitOfWork;
            //_gridSettings = gridSettings;
        }

        public override async Task DeleteRowAsync(ItemAttributeLookupView deleteViewEntity)
        {
            IAppRepository<ItemAttributeLookup> _itemAttributeLookupRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();

            var _recsDelete = await _itemAttributeLookupRepository.DeleteByIdAsync(deleteViewEntity.ItemAttributeLookupId);     //DeleteByAsync(ial => ial.ItemAttributeLookupId == SelectedItemAttributeLookup.ItemAttributeLookupId);

            if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute: {deleteViewEntity.AttributeName} is no longer found, was it deleted?");
            else
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute: {deleteViewEntity.AttributeName} was it deleted?");
        }
        async Task<int> UpdateWooProductAttributeMap(ItemAttributeLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooProductAttributeMap _updateWooProductAttributeMap = await GetWooMappedItemAsync(updatedViewEntity.ItemAttributeLookupId);
            if (_updateWooProductAttributeMap != null)
            {
                if (_updateWooProductAttributeMap.CanUpdate == updatedViewEntity.CanUpdateWooMap)
                {
                }
                else
                {
                    _updateWooProductAttributeMap.CanUpdate = (bool)updatedViewEntity.CanUpdateWooMap;
                    IAppRepository<WooProductAttributeMap> WooProductAttributeRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();
                    _recsUpdated = await WooProductAttributeRepository.UpdateAsync(_updateWooProductAttributeMap);
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute: {updatedViewEntity.AttributeName} was updated.");
                }
            }
            else
            {
                // nothing was found, so this probably means they now want to add. We should had a Pop up to check
                int _result = await AddWooItemAndMapAsync(GetItemFromView(updatedViewEntity));
                if (_result != AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute {updatedViewEntity.AttributeName} has been added to woo?");
            }

            return _recsUpdated;
        }
        public override async Task<int> DoGroupActionAsync(ItemAttributeLookupView toVeiwEntity, BulkAction selectedAction)
        {
            if (selectedAction == BulkAction.AllowWooSync)
                toVeiwEntity.CanUpdateWooMap = true;
            else if (selectedAction == BulkAction.DisallowWooSync)
                toVeiwEntity.CanUpdateWooMap = false;
            return await UpdateWooProductAttributeMap(toVeiwEntity);
        }
        //public override async Task<List<ItemAttributeLookup>> GetAllItemsAsync()
        //{
        //    IAppRepository<ItemAttributeLookup> _ItemAttributeLookupRepository = _appUnitOfWork.Repository<ItemAttributeLookup>();
        //    List<ItemAttributeLookup> _ItemAttributeLookups = (await _ItemAttributeLookupRepository.GetAllEagerAsync(ial => ial.ItemAttributeVarietyLookups))
        //        .OrderBy(ial => ial.AttributeName).ToList();

        //    return _ItemAttributeLookups;
        //}
        // used to the Push and Pop
        List<ItemAttributeLookupView> _oldSelectedItems = null;
        public override void PushSelectedItems(List<ItemAttributeLookupView> currentSelectedItems)
        {
            if (currentSelectedItems != null)
            {
                _oldSelectedItems = new List<ItemAttributeLookupView>(currentSelectedItems);
                currentSelectedItems.Clear(); // the refresh does this
            }
            else if (_oldSelectedItems != null)  // current selection is null
                _oldSelectedItems.Clear();  // note that nothing was selected
        }

        public override List<ItemAttributeLookupView> PopSelectedItems(List<ItemAttributeLookupView> modelViewItems)
        {
            List<ItemAttributeLookupView> _popList = null;
            if (_oldSelectedItems != null)
            {
                _popList = new List<ItemAttributeLookupView>();
                foreach (var item in _oldSelectedItems)
                {
                    var _oldSelectdItem = modelViewItems.Where(ial => ial.ItemAttributeLookupId == item.ItemAttributeLookupId).FirstOrDefault();
                    if (_oldSelectdItem != null)  // if it was deleted this will be the case
                        _popList.Add(_oldSelectdItem);
                }
            }
            return _popList;
        }
        public override async Task<WooProductAttributeMap> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            IAppRepository<WooProductAttributeMap> _wooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            return await _wooAttributeMapRepository.FindFirstAsync(wcm => wcm.ItemAttributeLookupId == mapWooEntityID);
        }
        public override async Task<List<WooProductAttributeMap>> GetWooMappedItemsAsync(List<Guid> mapWooEntityIDs)
        {
            IAppRepository<WooProductAttributeMap> _wooProductAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            var _WooMappedItems = await _wooProductAttributeMapRepository.GetByAsync(wam => mapWooEntityIDs.Contains(wam.ItemAttributeLookupId));   // ItemAttributeLookupId
            return _WooMappedItems.ToList();   //  (await _wooProductAttributeMapRepository.GetByAsync(wam => mapWooEntityIDs.Contains(wam.ItemAttributeLookupId)));
        }
        public override async Task<bool> IsDuplicate(ItemAttributeLookup targetEntity)
        {
            // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
            IAppRepository<ItemAttributeLookup> _itemAttributeLookupRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            var _exists = (await _itemAttributeLookupRepository.GetByAsync(ml => ml.AttributeName == targetEntity.AttributeName)).ToList();
            return ((_exists != null) && (_exists.Count > 1));
        }
        public override async Task<List<ItemAttributeLookup>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters)    //int startPage, int currentPageSize)
        {
            IItemAttributeLookupRepository _itemAttributeLookupRepository = _AppUnitOfWork.itemAttributeLookupRepository();
            //_gridSettings.TotalItems = await _ItemAttributeLookupRepository.CountAsync();  // get the total number of items to use for paging.
            DataGridItems<ItemAttributeLookup> _dataGridItems = await _itemAttributeLookupRepository.GetPagedDataEagerWithFilterAndOrderByAsync(currentDataGridParameters);
            List<ItemAttributeLookup> _ItemAttributeLookups = _dataGridItems.Entities.ToList();
            _GridSettings.TotalItems = _dataGridItems.TotalRecordCount;


            return _ItemAttributeLookups;
        }
        public override DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<ItemAttributeLookupView> inputDataGridReadData, string inputCustomerFilter)
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
        public override async Task<List<ItemAttributeLookupView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters)
        {
            List<ItemAttributeLookup> _itemAttributeLookups = await GetPagedItemsAsync(currentDataGridParameters);
            List<ItemAttributeLookupView> _itemAttributeViewLookups = new List<ItemAttributeLookupView>();
            // Get a list of all the Attribute maps that exists
            List<Guid> _ItemAttribIds = _itemAttributeLookups.Where(it => it.ItemAttributeLookupId != Guid.Empty).Select(it => it.ItemAttributeLookupId).ToList();   // get all the ids selected
            // Get all related ids
            List<WooProductAttributeMap> WooProductAttributeMaps = await GetWooMappedItemsAsync(_ItemAttribIds);
            // Map Items to Woo AttributeMap
            foreach (var itemAttrib in _itemAttributeLookups)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                WooProductAttributeMap _wooProductAttributeMap = WooProductAttributeMaps.Where(wam => wam.ItemAttributeLookupId == itemAttrib.ItemAttributeLookupId).FirstOrDefault();    //  if retrieving / record await GetWooMappedItemAsync(itemCat.ItemAttributeLookupId);

                _itemAttributeViewLookups.Add(new ItemAttributeLookupView
                {
                    ItemAttributeLookupId = itemAttrib.ItemAttributeLookupId,
                    AttributeName = itemAttrib.AttributeName,
                    ItemAttributeVarietyLookups = itemAttrib.ItemAttributeVarietyLookups,
                    OrderBy = itemAttrib.OrderBy,
                    Notes = itemAttrib.Notes,

                    CanUpdateWooMap = (_wooProductAttributeMap == null) ? null : _wooProductAttributeMap.CanUpdate
                });
            }
            return _itemAttributeViewLookups;
        }
        public override bool IsValid(ItemAttributeLookup checkEntity)
        {
            return !string.IsNullOrWhiteSpace(checkEntity.AttributeName); // (checkEntity.ParentAttributeId != checkEntity.ItemAttributeLookupId);
        }
        public override ItemAttributeLookupView NewItemDefaultSetter(ItemAttributeLookupView newViewEntity)
        {
            if (newViewEntity == null)
                newViewEntity = new ItemAttributeLookupView();

            newViewEntity.AttributeName = "Attribute (must be unique)";
            newViewEntity.Notes = $"Added {DateTime.Now.Date}";
            newViewEntity.ItemAttributeVarietyLookups = new List<ItemAttributeVarietyLookup>();  // needs a blank one
            newViewEntity.CanUpdateWooMap = null;
            return newViewEntity;
        }

        public override ItemAttributeLookup GetItemFromView(ItemAttributeLookupView fromVeiwEntity)
        {
            ItemAttributeLookup _newItemAttributeLookup = new ItemAttributeLookup
            {
                ItemAttributeLookupId = fromVeiwEntity.ItemAttributeLookupId,
                AttributeName = fromVeiwEntity.AttributeName,
                OrderBy = fromVeiwEntity.OrderBy,
                ItemAttributeVarietyLookups = fromVeiwEntity.ItemAttributeVarietyLookups,
                //                ParentAttributeId = (fromVeiwEntity.ParentAttributeId == Guid.Empty) ? null : fromVeiwEntity.ParentAttributeId,
                Notes = fromVeiwEntity.Notes,
            };

            return _newItemAttributeLookup;
        }
        public override async Task<int> UpdateItemAsync(ItemAttributeLookupView updateViewItem)
        {
            int _recsUpdted = 0;
            IAppRepository<ItemAttributeLookup> _itemAttributeLookupRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            // first check it exists - it could have been deleted 
            ItemAttributeLookup _CurrentItemAttributeLookup = await _itemAttributeLookupRepository.GetByIdAsync(updateViewItem.ItemAttributeLookupId);
            if (_CurrentItemAttributeLookup == null)
            {
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute: {updateViewItem.AttributeName} is no longer found, was it deleted?");
                return AppUnitOfWork.CONST_WASERROR;
            }
            else
            {
                _CurrentItemAttributeLookup.AttributeName = updateViewItem.AttributeName;
                _CurrentItemAttributeLookup.ItemAttributeVarietyLookups = updateViewItem.ItemAttributeVarietyLookups;  // (updateViewItem.ParentAttributeId == Guid.Empty) ? null : updateViewItem.ParentAttributeId;
                _CurrentItemAttributeLookup.OrderBy = updateViewItem.OrderBy;
                _CurrentItemAttributeLookup.Notes = updateViewItem.Notes;
                _recsUpdted = await _itemAttributeLookupRepository.UpdateAsync(_CurrentItemAttributeLookup);
                if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.AttributeName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating Attribute");
                if (await UpdateWooItemAndMapping(updateViewItem) == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.AttributeName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating Attribute Map");   // should we send a message here error = mapping not updated 

            }
            return _recsUpdted;
        }
        public override async Task InsertRowAsync(ItemAttributeLookupView newVeiwEntity)
        {
            IAppRepository<ItemAttributeLookup> _itemAttributeLookupRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            // first check we do not already have a Attribute like this.
            if (await _itemAttributeLookupRepository.FindFirstAsync(ial => ial.AttributeName == newVeiwEntity.AttributeName) == null)
            {
                ItemAttributeLookup _NewItemAttributeLookup = GetItemFromView(newVeiwEntity); // store this here since when it is added it will automatically update the id field
                int _recsAdded = await _itemAttributeLookupRepository.AddAsync(_NewItemAttributeLookup);
                if (_recsAdded != AppUnitOfWork.CONST_WASERROR)
                {
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.AttributeName} - added", "Attribute Added");
                    if (newVeiwEntity.CanUpdateWooMap ?? false)
                    {
                        // they selected to update woo so add to Woo
                        if (await AddWooItemAndMapAsync(_NewItemAttributeLookup) == AppUnitOfWork.CONST_WASERROR)   // add if they select to update
                            _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding {newVeiwEntity.AttributeName} to Woo - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Woo Attribute");
                        else
                            _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.AttributeName} - added to Woo", "Woo Attribute Added");
                    }
                }
                else
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.AttributeName} - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Attribute");
            }
            else
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.AttributeName} already exists, so could not be added.");
            //-> done in parent       await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }

        public override async Task UpdateRowAsync(ItemAttributeLookupView updateVeiwEntity)
        {
            if (await IsDuplicate(updateVeiwEntity))
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Name: {updateVeiwEntity.AttributeName} - already exists, cannot be updated", "Exists already");
            else
            {
                if (IsValid(updateVeiwEntity))
                {
                    // update and check for errors 
                    if (await UpdateItemAsync(updateVeiwEntity) == AppUnitOfWork.CONST_WASERROR)
                    {
                        _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating Attribute: {updateVeiwEntity.AttributeName} -  {_AppUnitOfWork.GetErrorMessage()}", "Updating Attribute Error");
                    }
                    else
                        _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute: {updateVeiwEntity.AttributeName} was updated.");
                }
                else
                {
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Item {updateVeiwEntity.AttributeName} cannot be parent and child.", "Error updating");
                }
            }
            //-> done in parent await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }
        public override async Task<int> UpdateWooMappingAsync(ItemAttributeLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooProductAttributeMap updateWooProductAttributeMap = await GetWooMappedItemAsync(updatedViewEntity.ItemAttributeLookupId);
            if (updateWooProductAttributeMap != null)
            {
                if (updateWooProductAttributeMap.CanUpdate == updatedViewEntity.CanUpdateWooMap)
                {
                    // not necessary to display message.
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo Attribute Map for Attribute: {updatedViewEntity.AttributeName} has not changed, so was not updated?");
                }
                else
                {
                    updateWooProductAttributeMap.CanUpdate = (bool)updatedViewEntity.CanUpdateWooMap;
                    IAppRepository<WooProductAttributeMap> WooProductAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();
                    _recsUpdated = await WooProductAttributeMapRepository.UpdateAsync(updateWooProductAttributeMap);
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute: {updatedViewEntity.AttributeName} was updated.");
                }
            }
            else
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute Map for Attribute: {updatedViewEntity.AttributeName} is no longer found, was it deleted?");

            await LoadAllViewItemsAsync();
            return _recsUpdated;
        }

        private async Task<WooProductAttributeMap> GetWooProductAttributeMapFromID(Guid? sourceWooEntityId)
        {
            if (sourceWooEntityId == null)
                return null;

            IAppRepository<WooProductAttributeMap> _wooProductAttributeMapRepo = _AppUnitOfWork.Repository<WooProductAttributeMap>();
            WooProductAttributeMap _wooProductAttributeMap = await _wooProductAttributeMapRepo.FindFirstAsync(wam => wam.ItemAttributeLookupId == sourceWooEntityId);
            return _wooProductAttributeMap;
        }
        private async Task<int> DeleteWooAttributeLink(WooProductAttributeMap deleteWooProductAttributeMap)
        {
            int _result = 0;
            IAppRepository<WooProductAttributeMap> _wooProductAttributeMapRepo = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            _result = await _wooProductAttributeMapRepo.DeleteByIdAsync(deleteWooProductAttributeMap.WooProductAttributeMapId);

            return _result;
        }
        private async Task<IWooProductAttribute> GetIWooProductAttribute()
        {
            //IAppRepository<WooSettings> _WooPrefs = _appUnitOfWork.Repository<WooSettings>();

            //WooSettings _wooSettings = await _WooPrefs.FindFirstAsync();
            //if (_wooSettings == null)
            //    return null;
            //WooAPISettings _wooAPISettings = new WooAPISettings(_wooSettings);
            WooAPISettings _wooAPISettings = await GetWooAPISettingsAsync();
            return new WooProductAttribute(_wooAPISettings, _Logger);
        }
        private async Task<int> DeleteWooAttribute(WooProductAttributeMap deleteWooProductAttributeMap)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;  // if all goes well this will be change to the number of records returned

            IWooProductAttribute _wooProductAttributeRepository = await GetIWooProductAttribute();
            if (_wooProductAttributeRepository != null)
            {
                ProductAttribute _TempWooCat = await _wooProductAttributeRepository.DeleteProductAttributeByIdAsync(deleteWooProductAttributeMap.WooProductAttributeId);
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
            WooProductAttributeMap _wooProductAttributeMap = await GetWooProductAttributeMapFromID(deleteWooEntityId);
            if (_wooProductAttributeMap == null)
                _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {deleteWooEntityId} was not found to have a Woo Attribute Map.");
            else
            {
                if (deleteFromWoo)
                {
                    _result = await DeleteWooAttribute(_wooProductAttributeMap); ///Delete the Attribute in Woo
                    if (_result == AppUnitOfWork.CONST_WASERROR)
                        _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {_wooProductAttributeMap.WooProductAttributeId} was not deleted from Woo categories. Error {_AppUnitOfWork.GetErrorMessage()}");
                    else
                        _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Id {_wooProductAttributeMap.WooProductAttributeId} was deleted from Woo categories.");
                }
                _result = await DeleteWooAttributeLink(_wooProductAttributeMap);   //Delete our link data, if there was an error should we?
                if (_result == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {_wooProductAttributeMap.WooProductAttributeId} was not deleted from Woo linked categories. Error {_AppUnitOfWork.GetErrorMessage()}");
                else
                    _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Id {_wooProductAttributeMap.WooProductAttributeId} was deleted from Woo linked categories.");
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
        //string _wooOrderBy = "menu_order";
        //switch (v.ToLower())
        //{
        //    case "id":   // OrderBys.Id.ToString().ToLower():
        //        _wooOrderBy = "id";
        //        break;
        //    case "name":  //OrderBys.Name
        //        _wooOrderBy = "name";
        //        break;

        //    case "number":  //OrderBys.Number
        //        _wooOrderBy = "name_num";
        //        break;

        //    default:
        //        break;
        //}
        //return _wooOrderBy;
        private async Task<ProductAttribute> AddItemToWooOnlySync(ItemAttributeLookup addEntity)
        {
            IWooProductAttribute _wooProductAttributeRepository = await GetIWooProductAttribute();
            if (_wooProductAttributeRepository == null)
                return null;

            ProductAttribute _wooProductAttribute = new ProductAttribute
            {
                name = addEntity.AttributeName,
                order_by = ConvertToWooOrderBy(addEntity.OrderBy),
            };
            return await _wooProductAttributeRepository.AddProductAttributeAsync(_wooProductAttribute);  // should return a new version with ID
        }

        private async Task<int> MapOurItemToWooItemSync(ProductAttribute newWooProductAttribute, ItemAttributeLookup addViewEntity)
        {
            // create a map to the woo Attribute maps using the id and the Attribute
            //
            IAppRepository<WooProductAttributeMap> _wooProductAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            return await _wooProductAttributeMapRepository.AddAsync(new WooProductAttributeMap
            {
                ItemAttributeLookupId = addViewEntity.ItemAttributeLookupId,
                WooProductAttributeId = Convert.ToInt32(newWooProductAttribute.id),
                CanUpdate = true,
            });
        }
        //private async Task<ProductAttribute> GetWooProductAttributeByName(string AttributeName)
        //{
        //    IWooProductAttribute _wooProductAttributeRepository = await GetIWooProductAttribute();
        //    if (_wooProductAttributeRepository == null)
        //        return null;

        //    return _wooProductAttributeRepository.FindProductAttributeByName(AttributeName);
        //}

        /// <summary>
        /// Add the item to woo
        /// </summary>
        /// <param name="addEntity"></param>
        /// <returns></returns>
        public override async Task<int> AddWooItemAndMapAsync(ItemAttributeLookup addEntity)
        {
            // check it the item exists in woo !!!!!!(we did not do this as there is no such call;, if so get is id and return, otherwise add it and get its id

            //ProductAttribute _wooProductAttribute = await GetWooProductAttributeByName(addEntity.AttributeName);
            //if (_wooProductAttribute == null)
            //{
            ProductAttribute _wooProductAttribute = await AddItemToWooOnlySync(addEntity);
            if (_wooProductAttribute == null)
                return AppUnitOfWork.CONST_WASERROR;
            return await MapOurItemToWooItemSync(_wooProductAttribute, addEntity);
            //}
            //else
            //    return AppUnitOfWork.CONST_WASERROR;
        }
        public override async Task<int> UpdateWooItemAsync(ItemAttributeLookupView updateViewEntity)
        {
            int _result = 0;  /// null or not found
            if ((updateViewEntity.HasWooAttributeMap) && ((bool)(updateViewEntity.CanUpdateWooMap)))
            {
                IWooProductAttribute _wooProductAttributeRepository = await GetIWooProductAttribute();
                if (_wooProductAttributeRepository != null)                     //  - > if it does not exist then what?
                {
                    WooProductAttributeMap _updateWooMapEntity = await GetWooProductAttributeMapFromID(updateViewEntity.ItemAttributeLookupId);
                    if (_updateWooMapEntity == null)
                    {
                        // need to add the Attribute -> this is done later.
                        _result = await AddWooItemAndMapAsync(updateViewEntity);
                    }
                    else
                    {
                        ProductAttribute _wooProductAttribute = await _wooProductAttributeRepository.GetProductAttributeByIdAsync(_updateWooMapEntity.WooProductAttributeId);
                        if (_wooProductAttribute == null)
                        {
                            return AppUnitOfWork.CONST_WASERROR;  /// oops what happened >?
                        }
                        else
                        {
                            // only update if different
                            if ((!_wooProductAttribute.name.Equals(updateViewEntity.AttributeName)) || (!_wooProductAttribute.order_by.Equals(updateViewEntity.OrderBy.ToString(), StringComparison.OrdinalIgnoreCase)))
                            {
                                _wooProductAttribute.name = updateViewEntity.AttributeName;  // only update if necessary
                                _wooProductAttribute.order_by = ConvertToWooOrderBy(updateViewEntity.OrderBy);
                                var _res = ((await _wooProductAttributeRepository.UpdateProductAttributeAsync(_wooProductAttribute)));
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
        public override async Task<int> UpdateWooItemAndMapping(ItemAttributeLookupView updateViewEntity)
        {
            int _result = await UpdateWooItemAsync(updateViewEntity);
            if (_result > 0)
                _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Updated woo Attribute {updateViewEntity.AttributeName}.");
            else if (_result == AppUnitOfWork.CONST_WASERROR)
                _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute {updateViewEntity.AttributeName} update failed.");

            _result = await UpdateWooProductAttributeMap(updateViewEntity);
            return _result;
        }
    }
}
