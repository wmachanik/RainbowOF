using Blazorise.DataGrid;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Items;
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
    public class AttributeVarietyWooLinkedViewRepository : WooLinkedView<ItemAttributeVarietyLookup, ItemAttributeVarietyLookupView, WooProductAttributeTermMap>, IAttributeVarietyWooLinkedView
    {
        private Guid _ParentItemAttributeLookupId = Guid.Empty;
        private int _WooParentAttributeId = -1;
        public AttributeVarietyWooLinkedViewRepository(ILoggerManager logger, IAppUnitOfWork appUnitOfWork, GridSettings gridSettings, Guid sourceParentItemAttributeLookupId) : base (logger, appUnitOfWork, gridSettings)
        {
            _ParentItemAttributeLookupId = sourceParentItemAttributeLookupId;
        }
        /// <summary>
        /// If the Parent ID changes and the grid needs a refresh then you need to change this value, otherwise the data will reflect the old Parent ID of the Attribute
        /// </summary>
        /// <param name="sourceParentAttributeId">The new ParentAttributeId</param>
        public void SetParentAttributeId(Guid sourceParentAttributeId)
        {
            if (!sourceParentAttributeId.Equals(_ParentItemAttributeLookupId))
            {
                _ParentItemAttributeLookupId = sourceParentAttributeId;
                _WooParentAttributeId = -1;  //  force a reload of the parent id from the database.
            }
        }
        private async Task<int> GetAttributeTermParentId()
        {
            if (_WooParentAttributeId == -1)
            {
                IAppRepository<WooProductAttributeMap> _wooAttributeMapRepository = _appUnitOfWork.Repository<WooProductAttributeMap>();
                WooProductAttributeMap wooProductAttributeMap = await _wooAttributeMapRepository.FindFirstAsync(wcm => wcm.ItemAttributeLookupId == _ParentItemAttributeLookupId);
                if (wooProductAttributeMap != null)
                    _WooParentAttributeId = wooProductAttributeMap.WooProductAttributeId;
                else
                    _WooParentAttributeId = 0;   // to say it has been read
            }
            return _WooParentAttributeId;
        }

        public override async Task DeleteRowAsync(ItemAttributeVarietyLookupView deleteViewEntity)
        {
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _appUnitOfWork.Repository<ItemAttributeVarietyLookup>();

            var _recsDelete = await _ItemAttributeVarietyLookupRepository.DeleteByIdAsync(deleteViewEntity.ItemAttributeVarietyLookupId);     //DeleteByAsync(iavl => iavl.ItemAttributeVarietyLookupId == SelectedItemAttributeVarietyLookup.ItemAttributeVarietyLookupId);

            if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Variety: {deleteViewEntity.VarietyName} is no longer found, was it deleted?");
            else
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute Variety: {deleteViewEntity.VarietyName} was it deleted?");
        }
        async Task<int> UpdateWooProductAttributeTermMap(ItemAttributeVarietyLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooProductAttributeTermMap _updateWooProductAttributeTermMap = await GetWooMappedItemAsync(updatedViewEntity.ItemAttributeVarietyLookupId);
            if (_updateWooProductAttributeTermMap != null)
            {
                if (_updateWooProductAttributeTermMap.CanUpdate == updatedViewEntity.CanUpdateWooMap)
                {
                }
                else
                {
                    _updateWooProductAttributeTermMap.CanUpdate = (bool)updatedViewEntity.CanUpdateWooMap;
                    IAppRepository<WooProductAttributeTermMap> WooProductAttributeTermRepository = _appUnitOfWork.Repository<WooProductAttributeTermMap>();
                    _recsUpdated = await WooProductAttributeTermRepository.UpdateAsync(_updateWooProductAttributeTermMap);
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute Variety: {updatedViewEntity.VarietyName} was updated.");
                }
            }
            else
            {
                // nothing was found, so this probably means they now want to add. We should had a Pop up to check
                int _result = await AddWooItemAndMapAsync(GetItemFromView(updatedViewEntity));
                if (_result != AppUnitOfWork.CONST_WASERROR)
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute Variety {updatedViewEntity.VarietyName} has been added to woo?");
            }
            return _recsUpdated;
        }
        public override async Task<int> DoGroupActionAsync(ItemAttributeVarietyLookupView toVeiwEntity, BulkAction selectedAction)
        {
            if (selectedAction == BulkAction.AllowWooSync)
                toVeiwEntity.CanUpdateWooMap = true;
            else if (selectedAction == BulkAction.DisallowWooSync)
                toVeiwEntity.CanUpdateWooMap = false;
            return await UpdateWooProductAttributeTermMap(toVeiwEntity);
        }
        //public override async Task<List<ItemAttributeVarietyLookup>> GetAllItemsAsync()
        //{
        //    IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _appUnitOfWork.Repository<ItemAttributeVarietyLookup>();
        //    List<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookups = (await _ItemAttributeVarietyLookupRepository.GetAllEagerAsync(iavl => iavl.ItemAttributeVarietyVarietyLookups))
        //        .OrderBy(iavl => iavl.VarietyName).ToList();

        //    return _ItemAttributeVarietyLookups;
        //}
        // used to the Push and Pop
        List<ItemAttributeVarietyLookupView> _oldSelectedItems = null;
        public override void PushSelectedItems(List<ItemAttributeVarietyLookupView> currentSelectedItems)
        {
            if (currentSelectedItems != null)
            {
                _oldSelectedItems = new List<ItemAttributeVarietyLookupView>(currentSelectedItems);
                currentSelectedItems.Clear(); // the refresh does this
            }
            else if (_oldSelectedItems != null)  // current selection is null
                _oldSelectedItems.Clear();  // note that nothing was selected
        }

        public override List<ItemAttributeVarietyLookupView> PopSelectedItems(List<ItemAttributeVarietyLookupView> modelViewItems)
        {
            List<ItemAttributeVarietyLookupView> _popList = null;
            if (_oldSelectedItems != null)
            {
                _popList = new List<ItemAttributeVarietyLookupView>();
                foreach (var item in _oldSelectedItems)
                {
                    var _oldSelectdItem = modelViewItems.Where(iavl => iavl.ItemAttributeVarietyLookupId == item.ItemAttributeVarietyLookupId).FirstOrDefault();
                    if (_oldSelectdItem != null)  // if it was deleted this will be the case
                        _popList.Add(_oldSelectdItem);
                }
            }
            return _popList;
        }
        /// <summary>
        /// Return a single WooMpaped Item using the MappedWooEnityIP
        /// </summary>
        /// <param name="mapWooEntityID">the ID of the Woo Item</param>
        /// <returns></returns>
        public override async Task<WooProductAttributeTermMap> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            IAppRepository<WooProductAttributeTermMap> _wooAttributeVarietyMapRepository = _appUnitOfWork.Repository<WooProductAttributeTermMap>();

            return await _wooAttributeVarietyMapRepository.FindFirstAsync(wcm => wcm.ItemAttributeVarietyLookupId == mapWooEntityID);
        }
        /// <summary>
        /// Get all the IDs associated to the woo mapped ID passed it
        /// </summary>
        /// <param name="mapWooEntityIDs"></param>
        /// <returns></returns>
        public override async Task<List<WooProductAttributeTermMap>> GetWooMappedItemsAsync(List<Guid> mapWooEntityIDs)
        {
            IAppRepository<WooProductAttributeTermMap> _WooProductAttributeTermMapRepository = _appUnitOfWork.Repository<WooProductAttributeTermMap>();

            var _WooMappedItems = await _WooProductAttributeTermMapRepository.GetByAsync(wam => mapWooEntityIDs.Contains(wam.ItemAttributeVarietyLookupId));   // ItemAttributeVarietyLookupId
            return _WooMappedItems.ToList();   
        }
        public override async Task<bool> IsDuplicate(ItemAttributeVarietyLookup targetEntity)
        {
            // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _appUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            var _exists = (await _ItemAttributeVarietyLookupRepository.GetByAsync(ml => ml.VarietyName == targetEntity.VarietyName)).ToList();
            return ((_exists != null) && (_exists.Count > 1));
        }
        public override async Task<List<ItemAttributeVarietyLookup>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters)    //int startPage, int currentPageSize)
        {
            IItemAttributeVarietyLookupRepository _ItemAttributeVarietyLookupRepository = _appUnitOfWork.itemAttributeVarietyLookupRepository();
            //_gridSettings.TotalItems = await _ItemAttributeVarietyLookupRepository.CountAsync();  // get the total number of items to use for paging.
            DataGridItems<ItemAttributeVarietyLookup> _dataGridItems = await _ItemAttributeVarietyLookupRepository.GetPagedDataEagerWithFilterAndOrderByAsync(currentDataGridParameters, _ParentItemAttributeLookupId);
            List<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookups = _dataGridItems.Entities.ToList();
            _gridSettings.TotalItems = _dataGridItems.TotalRecordCount;


            return _ItemAttributeVarietyLookups;
        }
        public override DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<ItemAttributeVarietyLookupView> inputDataGridReadData, string inputCustomerFilter)
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
        public override async Task<List<ItemAttributeVarietyLookupView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters)
        {
            List<ItemAttributeVarietyLookup> _itemAttributeVarietyLookups = await GetPagedItemsAsync(currentDataGridParameters);
            List<ItemAttributeVarietyLookupView> _itemAttributeVarietyViewLookups = new List<ItemAttributeVarietyLookupView>();
            // Get a list of all the AttributeVariety maps that exists
            List<Guid> _ItemAttributeVarietyIds = _itemAttributeVarietyLookups.Where(it => it.ItemAttributeVarietyLookupId != Guid.Empty).Select(it => it.ItemAttributeVarietyLookupId).ToList();   // get all the ids selected
            // Get all related ids
            List<WooProductAttributeTermMap> WooProductAttributeTermMaps = await GetWooMappedItemsAsync(_ItemAttributeVarietyIds);
            // now get all the Units of Measure as a lazy load
            List<Guid?> _ItemAttributeVarietyUomIds = _itemAttributeVarietyLookups.Where(it => (it.UoMId != null) && (it.UoMId != Guid.Empty)).Select(it => it.UoMId).Distinct().ToList();   // get all the ids selected should not return nulls
            List<ItemUoM> itemUoMs = await GetItemUoMsAsync(_ItemAttributeVarietyUomIds); 

            // Map Items to Woo AttributeVarietyMap
            foreach (var itemAttributeVariety in _itemAttributeVarietyLookups)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                WooProductAttributeTermMap _WooProductAttributeTermMap = WooProductAttributeTermMaps.Where(wam => wam.ItemAttributeVarietyLookupId == itemAttributeVariety.ItemAttributeVarietyLookupId).FirstOrDefault();    //  if retrieving / record await GetWooMappedItemAsync(itemCat.ItemAttributeVarietyLookupId);

                _itemAttributeVarietyViewLookups.Add(new ItemAttributeVarietyLookupView
                {
                    ItemAttributeVarietyLookupId = itemAttributeVariety.ItemAttributeVarietyLookupId,
                    ItemAttributeLookupId = itemAttributeVariety.ItemAttributeLookupId,
                    VarietyName = itemAttributeVariety.VarietyName,
                    Symbol = itemAttributeVariety.Symbol,
                    SortOrder = itemAttributeVariety.SortOrder,
                    BGColour = itemAttributeVariety.BGColour,
                    FGColour = itemAttributeVariety.FGColour,
                    UoMId = itemAttributeVariety.UoMId,
                    Notes = itemAttributeVariety.Notes,
                    UoM = (((itemAttributeVariety.UoMId ?? Guid.Empty) == Guid.Empty)) ? null 
                            : (itemUoMs == null) ? null : itemUoMs.Where(uid => uid.ItemUoMId == itemAttributeVariety.UoMId).FirstOrDefault(),   // apply the "lazy" load to the item
                    CanUpdateWooMap = (_WooProductAttributeTermMap == null) ? null : _WooProductAttributeTermMap.CanUpdate
                }) ;
            }
            return _itemAttributeVarietyViewLookups;
        }
        public override bool IsValid(ItemAttributeVarietyLookup checkEntity)
        {
            return !string.IsNullOrWhiteSpace(checkEntity.VarietyName); // (checkEntity.ParentAttributeVarietyId != checkEntity.ItemAttributeVarietyLookupId);
        }
        public override ItemAttributeVarietyLookupView NewItemDefaultSetter(ItemAttributeVarietyLookupView newViewEntity)
        {
            if (newViewEntity == null)
                newViewEntity = new ItemAttributeVarietyLookupView();

            newViewEntity.VarietyName = "AttributeVariety (must be unique)";
            newViewEntity.ItemAttributeLookupId = _ParentItemAttributeLookupId;
            newViewEntity.UoMId = null;
            newViewEntity.Symbol = string.Empty;
            newViewEntity.BGColour = string.Empty;
            newViewEntity.FGColour = string.Empty;
            newViewEntity.SortOrder = 0;
            newViewEntity.Notes = $"Added {DateTime.Now.Date}";
            newViewEntity.CanUpdateWooMap = null;
            return newViewEntity;
        }

        public override ItemAttributeVarietyLookup GetItemFromView(ItemAttributeVarietyLookupView fromVeiwEntity)
        {
            ItemAttributeVarietyLookup _newItemAttributeVarietyLookup = new ItemAttributeVarietyLookup
            {
                ItemAttributeVarietyLookupId = fromVeiwEntity.ItemAttributeVarietyLookupId,
                VarietyName = fromVeiwEntity.VarietyName,
                ItemAttributeLookupId = fromVeiwEntity.ItemAttributeLookupId,
                UoMId = fromVeiwEntity.UoMId,
                Symbol = fromVeiwEntity.Symbol,
                BGColour = fromVeiwEntity.BGColour,
                FGColour = fromVeiwEntity.FGColour,
                SortOrder = fromVeiwEntity.SortOrder,
                UoM = fromVeiwEntity.UoM,
                //                ParentAttributeVarietyId = (fromVeiwEntity.ParentAttributeVarietyId == Guid.Empty) ? null : fromVeiwEntity.ParentAttributeVarietyId,
                Notes = fromVeiwEntity.Notes,
            };

            return _newItemAttributeVarietyLookup;
        }
        public override async Task<int> UpdateItemAsync(ItemAttributeVarietyLookupView updateViewItem)
        {
            int _recsUpdted = 0;
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _appUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            // first check it exists - it could have been deleted 
            ItemAttributeVarietyLookup _CurrentItemAttributeVarietyLookup = await _ItemAttributeVarietyLookupRepository.GetByIdAsync(updateViewItem.ItemAttributeVarietyLookupId);
            if (_CurrentItemAttributeVarietyLookup == null)
            {
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"AttributeVariety: {updateViewItem.VarietyName} is no longer found, was it deleted?");
                return AppUnitOfWork.CONST_WASERROR;
            }
            else
            {
                _CurrentItemAttributeVarietyLookup.VarietyName = updateViewItem.VarietyName;
                _CurrentItemAttributeVarietyLookup.UoMId  = ( (updateViewItem.UoMId?? Guid.Empty) == Guid.Empty) ? null : updateViewItem.UoMId;
                _CurrentItemAttributeVarietyLookup.Symbol = updateViewItem.Symbol;
                _CurrentItemAttributeVarietyLookup.BGColour = updateViewItem.BGColour;
                _CurrentItemAttributeVarietyLookup.FGColour = updateViewItem.FGColour;
                _CurrentItemAttributeVarietyLookup.SortOrder = updateViewItem.SortOrder;
                _CurrentItemAttributeVarietyLookup.Notes = updateViewItem.Notes;
                _recsUpdted = await _ItemAttributeVarietyLookupRepository.UpdateAsync(_CurrentItemAttributeVarietyLookup);
                if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.VarietyName} - {_appUnitOfWork.GetErrorMessage()}", "Error updating Attribute Variety");
                if (await UpdateWooItemAndMapping(updateViewItem) == AppUnitOfWork.CONST_WASERROR)
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.VarietyName} - {_appUnitOfWork.GetErrorMessage()}", "Error updating Attribute Variety Map");   // should we send a message here error = mapping not updated 
            }
            return _recsUpdted;
        }
        public override async Task InsertRowAsync(ItemAttributeVarietyLookupView newVeiwEntity)
        {
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _appUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            // first check we do not already have a AttributeVariety like this.
            if (await _ItemAttributeVarietyLookupRepository.FindFirstAsync(iavl => iavl.VarietyName == newVeiwEntity.VarietyName) == null)
            {
                ItemAttributeVarietyLookup _NewItemAttributeVarietyLookup = GetItemFromView(newVeiwEntity); // store this here since when it is added it will automatically update the id field
                int _recsAdded = await _ItemAttributeVarietyLookupRepository.AddAsync(_NewItemAttributeVarietyLookup);
                if (_recsAdded != AppUnitOfWork.CONST_WASERROR)
                {
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.VarietyName} - added", "Attribute Variety Added");
                    if (newVeiwEntity.CanUpdateWooMap ?? false)
                    {
                        // they selected to update woo so add to Woo
                        if (await AddWooItemAndMapAsync(_NewItemAttributeVarietyLookup) == AppUnitOfWork.CONST_WASERROR)   // add if they select to update
                            _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding {newVeiwEntity.VarietyName} to Woo - {_appUnitOfWork.GetErrorMessage()}", "Error adding Woo Attribute Variety");
                        else
                            _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.VarietyName} - added to Woo", "Woo Attribute Variety Added");
                    }
                }
                else
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.VarietyName} - {_appUnitOfWork.GetErrorMessage()}", "Error adding Attribute Variety");
            }
            else
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.VarietyName} already exists, so could not be added.");
            //-> done in parent       await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }

        public override async Task UpdateRowAsync(ItemAttributeVarietyLookupView updateVeiwEntity)
        {
            if (await IsDuplicate(updateVeiwEntity))
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Variety Name: {updateVeiwEntity.VarietyName} - already exists, cannot be updated", "Exists already");
            else
            {
                if (IsValid(updateVeiwEntity))
                {
                    // update and check for errors 
                    if (await UpdateItemAsync(updateVeiwEntity) == AppUnitOfWork.CONST_WASERROR)
                    {
                        _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating Attribute Variety: {updateVeiwEntity.VarietyName} -  {_appUnitOfWork.GetErrorMessage()}", "Updating AttributeVariety Error");
                    }
                    else
                        _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute Variety: {updateVeiwEntity.VarietyName} was updated.");
                }
                else
                {
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Variety Item {updateVeiwEntity.VarietyName} cannot be parent and child.", "Error updating");
                }
            }
            //-> done in parent await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }
        public override async Task<int> UpdateWooMappingAsync(ItemAttributeVarietyLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooProductAttributeTermMap updateWooProductAttributeTermMap = await GetWooMappedItemAsync(updatedViewEntity.ItemAttributeVarietyLookupId);
            if (updateWooProductAttributeTermMap != null)
            {
                if (updateWooProductAttributeTermMap.CanUpdate == updatedViewEntity.CanUpdateWooMap)
                {
                    // not necessary to display message.
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo AttributeVariety Map for AttributeVariety: {updatedViewEntity.VarietyName} has not changed, so was not updated?");
                }
                else
                {
                    updateWooProductAttributeTermMap.CanUpdate = (bool)updatedViewEntity.CanUpdateWooMap;
                    IAppRepository<WooProductAttributeTermMap> WooProductAttributeTermMapRepository = _appUnitOfWork.Repository<WooProductAttributeTermMap>();
                    _recsUpdated = await WooProductAttributeTermMapRepository.UpdateAsync(updateWooProductAttributeTermMap);
                    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute Variety: {updatedViewEntity.VarietyName} was updated.");
                }
            }
            else
                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute Variety Map for AttributeVariety: {updatedViewEntity.VarietyName} is no longer found, was it deleted?");

            await LoadAllViewItemsAsync();
            return _recsUpdated;
        }

        private async Task<WooProductAttributeTermMap> GetWooProductAttributeTermMapFromID(Guid? sourceWooEntityId)
        {
            if (sourceWooEntityId == null)
                return null;

            IAppRepository<WooProductAttributeTermMap> _WooProductAttributeTermMapRepo = _appUnitOfWork.Repository<WooProductAttributeTermMap>();
            WooProductAttributeTermMap _WooProductAttributeTermMap = await _WooProductAttributeTermMapRepo.FindFirstAsync(wam => wam.ItemAttributeVarietyLookupId == sourceWooEntityId);
            return _WooProductAttributeTermMap;
        }
        private async Task<int> DeleteWooAttributeVarietyLink(WooProductAttributeTermMap deleteWooProductAttributeTermMap)
        {
            int _result = 0;
            IAppRepository<WooProductAttributeTermMap> _WooProductAttributeTermMapRepo = _appUnitOfWork.Repository<WooProductAttributeTermMap>();

            _result = await _WooProductAttributeTermMapRepo.DeleteByIdAsync(deleteWooProductAttributeTermMap.WooProductAttributeTermMapId);

            return _result;
        }
        private async Task<IWooProductAttributeTerm> GetIWooProductAttributeTerm()
        {
            WooAPISettings _wooAPISettings = await GetWooAPISettingsAsync();
            return new WooProductAttributeTerm(_wooAPISettings, _logger);
        }
        private async Task<int> DeleteWooAttributeVariety(WooProductAttributeTermMap deleteWooProductAttributeTermMap)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;  // if all goes well this will be change to the number of records returned

            IWooProductAttributeTerm _WooProductAttributeTermRepository = await GetIWooProductAttributeTerm();
            if (_WooProductAttributeTermRepository != null)
            {
                int _parentAtributeId = await GetAttributeTermParentId();
                string _TempWooCat = await _WooProductAttributeTermRepository.DeleteProductAttributeTermByIdAsync(deleteWooProductAttributeTermMap.WooProductAttributeTermId, _parentAtributeId );
                _result = (String.IsNullOrEmpty(_TempWooCat)) ? AppUnitOfWork.CONST_WASERROR : 1;  // return the id
            }
            return _result;
        }
        /// <summary>
        /// Delete a Woo AttributeVariety from the mapped table and Woo, if they ask us to 
        /// </summary>
        /// <param name="WooEntityId">Id to delete</param>
        /// <param name="deleteFromWoo">True of we want to delete from Woo too</param>
        /// <returns></returns>
        public override async Task<int> DeleteWooItemAsync(Guid deleteWooEntityId, bool deleteFromWoo)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;
            // delete the woo AttributeVariety
            WooProductAttributeTermMap _WooProductAttributeTermMap = await GetWooProductAttributeTermMapFromID(deleteWooEntityId);
            if (_WooProductAttributeTermMap == null)
                _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product AttributeVariety Id {deleteWooEntityId} was not found to have a Woo AttributeVariety Map.");
            else
            {
                if (deleteFromWoo)
                {
                    _result = await DeleteWooAttributeVariety(_WooProductAttributeTermMap); ///Delete the AttributeVariety in Woo
                    if (_result == AppUnitOfWork.CONST_WASERROR)
                        _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Variety Id {_WooProductAttributeTermMap.WooProductAttributeTermId} was not deleted from Woo categories. Error {_appUnitOfWork.GetErrorMessage()}");
                    else
                        _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Variety Id {_WooProductAttributeTermMap.WooProductAttributeTermId} was deleted from Woo categories.");
                }
                _result = await DeleteWooAttributeVarietyLink(_WooProductAttributeTermMap);   //Delete our link data, if there was an error should we?
                if (_result == AppUnitOfWork.CONST_WASERROR)
                    _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Variety Id {_WooProductAttributeTermMap.WooProductAttributeTermId} was not deleted from Woo linked categories. Error {_appUnitOfWork.GetErrorMessage()}");
                else
                    _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Variety Id {_WooProductAttributeTermMap.WooProductAttributeTermId} was deleted from Woo linked categories.");
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
        private async Task<ProductAttributeTerm> AddItemToWooOnlySync(ItemAttributeVarietyLookup addEntity)
        {
            IWooProductAttributeTerm _WooProductAttributeTermRepository = await GetIWooProductAttributeTerm();
            if (_WooProductAttributeTermRepository == null)
                return null;

            ProductAttributeTerm _WooProductAttributeTerm = new ProductAttributeTerm
            {
                name = addEntity.VarietyName,
                //                 order_by = ConvertToWooOrderBy(addEntity.OrderBy),
            };
            int _parentAttributeId = await GetAttributeTermParentId();
            return await _WooProductAttributeTermRepository.AddProductAttributeTermAsync(_WooProductAttributeTerm, _parentAttributeId);  // should return a new version with ID
        }

        private async Task<int> MapOurItemToWooItemSync(ProductAttributeTerm newWooProductAttributeTerm, ItemAttributeVarietyLookup addViewEntity)
        {
            // create a map to the woo AttributeVariety maps using the id and the AttributeVariety
            //
            IAppRepository<WooProductAttributeTermMap> _WooProductAttributeTermMapRepository = _appUnitOfWork.Repository<WooProductAttributeTermMap>();

            return await _WooProductAttributeTermMapRepository.AddAsync(new WooProductAttributeTermMap
            {
                ItemAttributeVarietyLookupId = addViewEntity.ItemAttributeVarietyLookupId,
                WooProductAttributeTermId = Convert.ToInt32(newWooProductAttributeTerm.id),
                CanUpdate = true,
            });
        }
        //private async Task<ProductAttributeVariety> GetWooProductAttributeTermByName(string VarietyName)
        //{
        //    IWooProductAttributeTerm _WooProductAttributeTermRepository = await GetIWooProductAttributeTerm();
        //    if (_WooProductAttributeTermRepository == null)
        //        return null;

        //    return _WooProductAttributeTermRepository.FindProductAttributeVarietyByName(VarietyName);
        //}

        /// <summary>
        /// Add the item to woo
        /// </summary>
        /// <param name="addEntity"></param>
        /// <returns></returns>
        public override async Task<int> AddWooItemAndMapAsync(ItemAttributeVarietyLookup addEntity)
        {
            // check it the item exists in woo !!!!!!(we did not do this as there is no such call;, if so get is id and return, otherwise add it and get its id

            //ProductAttributeVariety _WooProductAttributeTerm = await GetWooProductAttributeTermByName(addEntity.VarietyName);
            //if (_WooProductAttributeTerm == null)
            //{
            ProductAttributeTerm _WooProductAttributeTerm = await AddItemToWooOnlySync(addEntity);
            if (_WooProductAttributeTerm == null)
                return AppUnitOfWork.CONST_WASERROR;
            return await MapOurItemToWooItemSync(_WooProductAttributeTerm, addEntity);
            //}
            //else
            //    return AppUnitOfWork.CONST_WASERROR;
        }
        public override async Task<int> UpdateWooItemAsync(ItemAttributeVarietyLookupView updateViewEntity)
        {
            int _result = 0;  /// null or not found
            if ((updateViewEntity.HasWooAttributeVarietyMap) && ((bool)(updateViewEntity.CanUpdateWooMap)))
            {
                IWooProductAttributeTerm _WooProductAttributeTermRepository = await GetIWooProductAttributeTerm();
                if (_WooProductAttributeTermRepository != null)                     //  - > if it does not exist then what?
                {
                    WooProductAttributeTermMap _updateWooMapEntity = await GetWooProductAttributeTermMapFromID(updateViewEntity.ItemAttributeVarietyLookupId);
                    if (_updateWooMapEntity == null)
                    {
                        // need to add the AttributeVariety -> this is done later.
                        _result = await AddWooItemAndMapAsync(updateViewEntity);
                    }
                    else
                    {
                        int _parentAttributeId = await GetAttributeTermParentId();
                        ProductAttributeTerm _WooProductAttributeTerm = await _WooProductAttributeTermRepository.GetProductAttributeTermByIdAsync(_updateWooMapEntity.WooProductAttributeTermId, _parentAttributeId);
                        if (_WooProductAttributeTerm == null)
                        {
                            return AppUnitOfWork.CONST_WASERROR;  /// oops what happened >?
                        }
                        else
                        {
                            // only update if different
                            if (!_WooProductAttributeTerm.name.Equals(updateViewEntity.VarietyName)) //|| (!_WooProductAttributeTerm.order_by.Equals(updateViewEntity.OrderBy.ToString(), StringComparison.OrdinalIgnoreCase)))
                            {
                                _WooProductAttributeTerm.name = updateViewEntity.VarietyName;  // only update if necessary
                                                                                               //_WooProductAttributeTerm.order_by = ConvertToWooOrderBy(updateViewEntity.OrderBy);
                                var _res = ((await _WooProductAttributeTermRepository.UpdateProductAttributeTermAsync(_WooProductAttributeTerm, _parentAttributeId)));
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
        public override async Task<int> UpdateWooItemAndMapping(ItemAttributeVarietyLookupView updateViewEntity)
        {
            int _result = await UpdateWooItemAsync(updateViewEntity);
            if (_result > 0)
                _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Updated woo AttributeVariety {updateViewEntity.VarietyName}.");
            else if (_result == AppUnitOfWork.CONST_WASERROR)
                _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo AttributeVariety {updateViewEntity.VarietyName} update failed.");

            _result = await UpdateWooProductAttributeTermMap(updateViewEntity);
            return _result;
        }

        public async Task<List<ItemUoM>> GetItemUoMsAsync(List<Guid?> linkedItemUoMIDs)
        {
            if (linkedItemUoMIDs == null)
                return null;
             
            IAppRepository<ItemUoM> appRepository   = _appUnitOfWork.Repository<ItemUoM>();

            var _ItemUoMs = await appRepository.GetByAsync(iu =>  linkedItemUoMIDs.Contains(iu.ItemUoMId));   // ItemAttributeVarietyLookupId
            return _ItemUoMs.ToList();
        }
    }
}
