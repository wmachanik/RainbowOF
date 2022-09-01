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
    public class AttributeVarietyWooLinkedViewRepository : WooLinkedView<ItemAttributeVarietyLookup, ItemAttributeVarietyLookupView, WooProductAttributeTermMap>, IAttributeVarietyWooLinkedView
    {
        #region Private variables
        private Guid _parentItemAttributeLookupId = Guid.Empty;
        private int _wooParentAttributeId = UnitOfWork.CONST_INVALID_ID;
        #endregion
        #region Initialise
        public AttributeVarietyWooLinkedViewRepository(ILoggerManager logger,
                                                       IUnitOfWork sourceAppUnitOfWork,
                                                       //GridSettings sourceGridSettings,
                                                       IMapper sourceMapper,
                                                       Guid sourceParentItemAttributeLookupId) : base(logger, sourceAppUnitOfWork, /*sourceGridSettings,*/ sourceMapper)
        {
            _parentItemAttributeLookupId = sourceParentItemAttributeLookupId;
            logger.LogDebug("AttributeVarietyWooLinkedViewRepository initialised");
        }
        #endregion
        #region Public interfaces
        /// <summary>
        /// If the Parent ID changes and the grid needs a refresh then you need to change this value, otherwise the data will reflect the old Parent ID of the Attribute
        /// </summary>
        /// <param name="sourceParentAttributeId">The new ParentAttributeId</param>
        public void SetParentAttributeId(Guid sourceParentAttributeId)
        {
            if (!sourceParentAttributeId.Equals(_parentItemAttributeLookupId))
            {
                _parentItemAttributeLookupId = sourceParentAttributeId;
                _wooParentAttributeId = UnitOfWork.CONST_INVALID_ID;  //  force a reload of the parent id from the database.
            }
        }
        private async Task<int> GetAttributeTermParentIdAsync()
        {
            if (_wooParentAttributeId == UnitOfWork.CONST_INVALID_ID)
            {
                IRepository<WooProductAttributeMap> _wooAttributeMapRepository = AppUnitOfWork.Repository<WooProductAttributeMap>();
                WooProductAttributeMap wooProductAttributeMap = await _wooAttributeMapRepository.GetByIdAsync(wcm => wcm.ItemAttributeLookupId == _parentItemAttributeLookupId);
                if (wooProductAttributeMap != null)
                    _wooParentAttributeId = wooProductAttributeMap.WooProductAttributeId;
                else
                    _wooParentAttributeId = 0;   // to say it has been read
            }
            return _wooParentAttributeId;
        }
        public override async Task DeleteRowAsync(ItemAttributeVarietyLookupView deleteViewEntity)
        {
            IRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyLookupRepository = AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();

            //var ignore = await Task.Run(() => deleteViewEntity.BGColour);   // rubbish code so we don't do Async - Async Delete causing issues.
            //var _recsDelete = _itemAttributeVarietyLookupRepository.DeleteById(deleteViewEntity.ItemAttributeVarietyLookupId);   // await   DeleteByIdAsync(deleteViewEntity.ItemAttributeVarietyLookupId);     //DeleteByAsync(iavl => iavl.ItemAttributeVarietyLookupId == SelectedItemAttributeVarietyLookup.ItemAttributeVarietyLookupId);
            var _recsDelete = await _itemAttributeVarietyLookupRepository.DeleteByEntityAsync(deleteViewEntity);   // await   DeleteByIdAsync(deleteViewEntity.ItemAttributeVarietyLookupId);     //DeleteByAsync(iavl => iavl.ItemAttributeVarietyLookupId == SelectedItemAttributeVarietyLookup.ItemAttributeVarietyLookupId);

            if (_recsDelete == UnitOfWork.CONST_WASERROR)
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Attribute Variety: {deleteViewEntity.VarietyName} is no longer found, was it deleted?");
            else
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Attribute Variety: {deleteViewEntity.VarietyName} was it deleted?");
        }
        async Task<int> UpdateWooProductAttributeTermMapAsync(ItemAttributeVarietyLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooProductAttributeTermMap _updateWooProductAttributeTermMap = await GetWooMappedItemAsync(updatedViewEntity.ItemAttributeVarietyLookupId);
            if (_updateWooProductAttributeTermMap != null)
            {
                if (_updateWooProductAttributeTermMap.CanUpdate == updatedViewEntity.CanUpdateECommerceMap)
                {
                }
                else
                {
                    _updateWooProductAttributeTermMap.CanUpdate = (bool)updatedViewEntity.CanUpdateECommerceMap;
                    IRepository<WooProductAttributeTermMap> WooProductAttributeTermRepository = AppUnitOfWork.Repository<WooProductAttributeTermMap>();
                    _recsUpdated = await WooProductAttributeTermRepository.UpdateAsync(_updateWooProductAttributeTermMap);
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Attribute Variety: {updatedViewEntity.VarietyName} was updated.");
                }
            }
            else
            {
                // nothing was found, so this probably means they now want to add. We should had a Pop up to check
                var _result = await AddWooItemAndMapAsync(GetItemFromView(updatedViewEntity));
                if (_result != null)
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Attribute Variety {updatedViewEntity.VarietyName} has been added to woo?");
            }
            return _recsUpdated;
        }
        public override async Task<int> DoGroupActionAsync(ItemAttributeVarietyLookupView toVeiwEntity, BulkAction selectedAction)
        {
            if (selectedAction == BulkAction.AllowWooSync)
                toVeiwEntity.CanUpdateECommerceMap = true;
            else if (selectedAction == BulkAction.DisallowWooSync)
                toVeiwEntity.CanUpdateECommerceMap = false;
            return await UpdateWooProductAttributeTermMapAsync(toVeiwEntity);
        }
        //public override async Task<List<ItemAttributeVarietyLookup>> GetAllItemsAsync()
        //{
        //    IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
        //    List<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookups = (await _ItemAttributeVarietyLookupRepository.GetAllEagerAsync(iavl => iavl.ItemAttributeVarietyVarietyLookups))
        //        .OrderBy(iavl => iavl.VarietyName).ToList();

        //    return _ItemAttributeVarietyLookups;
        //}
        // used to the Push and Pop
        private List<ItemAttributeVarietyLookupView> _oldSelectedItems = null;
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
            IRepository<WooProductAttributeTermMap> _wooAttributeVarietyMapRepository = AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            return await _wooAttributeVarietyMapRepository.GetByIdAsync(wcm => wcm.ItemAttributeVarietyLookupId == mapWooEntityID);
        }
        /// <summary>
        /// Get all the IDs associated to the woo mapped ID passed it
        /// </summary>
        /// <param name="mapWooEntityIDs"></param>
        /// <returns></returns>
        public override async Task<List<WooProductAttributeTermMap>> GetWooMappedItemsAsync(List<Guid> mapWooEntityIDs)
        {
            IRepository<WooProductAttributeTermMap> _wooProductAttributeTermMapRepository = AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            var _WooMappedItems = await _wooProductAttributeTermMapRepository.GetByAsync(wam => mapWooEntityIDs.Contains(wam.ItemAttributeVarietyLookupId));   // ItemAttributeVarietyLookupId
            return _WooMappedItems.ToList();
        }
        public override async Task<bool> IsDuplicateAsync(ItemAttributeVarietyLookup targetEntity)
        {
            // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
            IRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyLookupRepository = AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            var _exists = (await _itemAttributeVarietyLookupRepository.GetByAsync(ml => ml.VarietyName == targetEntity.VarietyName)).ToList();
            return ((_exists != null) && (_exists.Count > 1));
        }
        public override async Task<List<ItemAttributeVarietyLookup>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters)    //int startPage, int currentPageSize)
        {
            //_WooLinkedGridSettings.TotalItems = await _ItemAttributeVarietyLookupRepository.CountAsync();  // get the total number of items to use for paging.
            DataGridItems<ItemAttributeVarietyLookup> _dataGridItems = await AppUnitOfWork.ItemAttributeVarietyLookupRepository.GetPagedDataEagerWithFilterAndOrderByAsync(currentDataGridParameters, _parentItemAttributeLookupId);
            List<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookups = _dataGridItems.Entities.ToList();
            CurrWooLinkedGridSettings.TotalItems = _dataGridItems.TotalRecordCount;

            return _ItemAttributeVarietyLookups;
        }
        public override DataGridParameters GetDataGridCurrent(DataGridReadDataEventArgs<ItemAttributeVarietyLookupView> inputDataGridReadData, string inputCustomerFilter)
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
                        _dataGridParameters.SortParams ??= new List<SortParam>();   // if null create a new record
                        _dataGridParameters.SortParams.Add(new SortParam
                        {
                            FieldName = col.Field,
                            Direction = col.SortDirection
                        });
                    }
                    if (!string.IsNullOrEmpty((string)col.SearchValue)) // changed this in v 4
                    {
                        _dataGridParameters.FilterParams ??= new List<FilterParam>();   // if null create a new record
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
        private bool _taskIsBusy = false;
        public override async Task<List<ItemAttributeVarietyLookupView>> LoadViewItemsPaginatedAsync(DataGridParameters currentDataGridParameters)
        {
            if (_taskIsBusy)
                return null;

            _taskIsBusy = true;
            List<ItemAttributeVarietyLookup> _itemAttributeVarietyLookups = await GetPagedItemsAsync(currentDataGridParameters);
            List<ItemAttributeVarietyLookupView> _itemAttributeVarietyViewLookups = new();
            // Get a list of all the AttributeVariety maps that exists
            List<Guid> _ItemAttributeVarietyIds = _itemAttributeVarietyLookups.Where(it => it.ItemAttributeVarietyLookupId != Guid.Empty).Select(it => it.ItemAttributeVarietyLookupId).ToList();   // get all the ids selected
            // Get all related ids
            List<WooProductAttributeTermMap> WooProductAttributeTermMaps = await GetWooMappedItemsAsync(_ItemAttributeVarietyIds);
            // now get all the Units of Measure as a lazy load
            List<Guid?> _itemAttributeVarietyUomIds = _itemAttributeVarietyLookups.Where(it => (it.UoMId != null) && (it.UoMId != Guid.Empty)).Select(it => it.UoMId).Distinct().ToList();   // get all the ids selected should not return nulls
            List<ItemUoMLookup> itemUoMs = await GetItemUoMsAsync(_itemAttributeVarietyUomIds);
            // Map Items to Woo AttributeVarietyMap
            foreach (var itemAttributeVariety in _itemAttributeVarietyLookups)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                WooProductAttributeTermMap _wooProductAttributeTermMap = WooProductAttributeTermMaps.Where(wam => wam.ItemAttributeVarietyLookupId == itemAttributeVariety.ItemAttributeVarietyLookupId).FirstOrDefault();    //  if retrieving / record await GetWooMappedItemAsync(itemCat.ItemAttributeVarietyLookupId);
                ItemAttributeVarietyLookupView _itemAttributeVarietyLookupView = new();
                AppMapper.Map(itemAttributeVariety, _itemAttributeVarietyLookupView);
                _itemAttributeVarietyLookupView.UoM = (((itemAttributeVariety.UoMId ?? Guid.Empty) == Guid.Empty)) ? null
                        : itemUoMs?.Where(uid => uid.ItemUoMLookupId == itemAttributeVariety.UoMId).FirstOrDefault();   // apply the "lazy" load to the item   /// : (itemUoMs == null) ? null : itemUoMs.Where(uid => uid.ItemUoMId == itemAttributeVariety.UoMId).FirstOrDefault(),
                _itemAttributeVarietyLookupView.CanUpdateECommerceMap = _wooProductAttributeTermMap?.CanUpdate;
                _itemAttributeVarietyViewLookups.Add(_itemAttributeVarietyLookupView);
            }
            _taskIsBusy = false;
            return _itemAttributeVarietyViewLookups;
        }
        public override bool IsValid(ItemAttributeVarietyLookup checkEntity)
        {
            return !string.IsNullOrWhiteSpace(checkEntity.VarietyName); // (checkEntity.ParentAttributeVarietyId != checkEntity.ItemAttributeVarietyLookupId);
        }
        public override ItemAttributeVarietyLookupView NewItemDefaultSetter(ItemAttributeVarietyLookupView newViewEntity)
        {
            newViewEntity ??= new ItemAttributeVarietyLookupView();   // if null create a new record
            newViewEntity.VarietyName = "AttributeVariety (must be unique)";
            newViewEntity.ItemAttributeLookupId = _parentItemAttributeLookupId;
            newViewEntity.UoMId = null;
            newViewEntity.UoMQtyPerItem = 1;
            newViewEntity.DefaultSKUSuffix = string.Empty;
            newViewEntity.Symbol = string.Empty;
            newViewEntity.BGColour = string.Empty;
            newViewEntity.FGColour = string.Empty;
            newViewEntity.SortOrder = 0;
            newViewEntity.Notes = $"Added {DateTime.Now.Date}";
            newViewEntity.CanUpdateECommerceMap = null;
            return newViewEntity;
        }

        public override ItemAttributeVarietyLookup GetItemFromView(ItemAttributeVarietyLookupView fromVeiwEntity)
        {
            ItemAttributeVarietyLookup _newItemAttributeVarietyLookup = new();
            AppMapper.Map(fromVeiwEntity, _newItemAttributeVarietyLookup);
            //{
            //    ItemAttributeVarietyLookupId = fromVeiwEntity.ItemAttributeVarietyLookupId,
            //    VarietyName = fromVeiwEntity.VarietyName,
            //    ItemAttributeLookupId = fromVeiwEntity.ItemAttributeLookupId,
            //    UoMId = fromVeiwEntity.UoMId,
            //    Symbol = fromVeiwEntity.Symbol,
            //    BGColour = fromVeiwEntity.BGColour,
            //    FGColour = fromVeiwEntity.FGColour,
            //    SortOrder = fromVeiwEntity.SortOrder,
            //    UoM = fromVeiwEntity.UoM,
            //    //                ParentAttributeVarietyId = (fromVeiwEntity.ParentAttributeVarietyId == Guid.Empty) ? null : fromVeiwEntity.ParentAttributeVarietyId,
            //    Notes = fromVeiwEntity.Notes,
            //};

            return _newItemAttributeVarietyLookup;
        }
        public override async Task<int> UpdateItemAsync(ItemAttributeVarietyLookupView updateViewItem)
        {
            IRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyLookupRepository = AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            // first check it exists - it could have been deleted 
            ItemAttributeVarietyLookup _CurrentItemAttributeVarietyLookup = await _itemAttributeVarietyLookupRepository.GetByIdAsync(updateViewItem.ItemAttributeVarietyLookupId);
            if (_CurrentItemAttributeVarietyLookup == null)
            {
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"AttributeVariety: {updateViewItem.VarietyName} is no longer found, was it deleted?");
                return UnitOfWork.CONST_WASERROR;
            }
            else
            {
                AppMapper.Map(updateViewItem, _CurrentItemAttributeVarietyLookup);
                //_CurrentItemAttributeVarietyLookup.VarietyName = _CurrentItemAttributeVarietyLookup.VarietyName;
                //_CurrentItemAttributeVarietyLookup.UoMId  = ( (updateViewItem.UoMId?? Guid.Empty) == Guid.Empty) ? null : updateViewItem.UoMId;
                //_CurrentItemAttributeVarietyLookup.Symbol = updateViewItem.Symbol;
                //_CurrentItemAttributeVarietyLookup.BGColour = updateViewItem.BGColour;
                //_CurrentItemAttributeVarietyLookup.FGColour = updateViewItem.FGColour;
                //_CurrentItemAttributeVarietyLookup.SortOrder = updateViewItem.SortOrder;
                //_CurrentItemAttributeVarietyLookup.Notes = updateViewItem.Notes;
                int _recsUpdted = await _itemAttributeVarietyLookupRepository.UpdateAsync(_CurrentItemAttributeVarietyLookup);
                if (_recsUpdted == UnitOfWork.CONST_WASERROR)
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.VarietyName} - {AppUnitOfWork.GetErrorMessage()}", "Error updating Attribute Variety");
                if (await UpdateWooItemAndMappingAsync(updateViewItem) == UnitOfWork.CONST_WASERROR)
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.VarietyName} - {AppUnitOfWork.GetErrorMessage()}", "Error updating Attribute Variety Map");   // should we send a message here error = mapping not updated 
                return _recsUpdted;
            }
        }
        public override async Task InsertRowAsync(ItemAttributeVarietyLookupView newVeiwEntity)
        {
            IRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyLookupRepository = AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            // first check we do not already have a AttributeVariety like this.
            if (await _itemAttributeVarietyLookupRepository.GetByIdAsync(iavl => iavl.VarietyName == newVeiwEntity.VarietyName) == null)
            {
                ItemAttributeVarietyLookup _NewItemAttributeVarietyLookup = GetItemFromView(newVeiwEntity); // store this here since when it is added it will automatically update the id field
                var _recsAdded = await _itemAttributeVarietyLookupRepository.AddAsync(_NewItemAttributeVarietyLookup);
                if (_recsAdded != null)
                {
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.VarietyName} - added", "Attribute Variety Added");
                    if (newVeiwEntity.CanUpdateECommerceMap ?? false)
                    {
                        // they selected to update woo so add to Woo
                        if (await AddWooItemAndMapAsync(_NewItemAttributeVarietyLookup) == null)   // add if they select to update
                            await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error adding {newVeiwEntity.VarietyName} to Woo - {AppUnitOfWork.GetErrorMessage()}", "Error adding Woo Attribute Variety");
                        else
                            await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.VarietyName} - added to Woo", "Woo Attribute Variety Added");
                    }
                }
                else
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.VarietyName} - {AppUnitOfWork.GetErrorMessage()}", "Error adding Attribute Variety");
            }
            else
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.VarietyName} already exists, so could not be added.");
            //-> done in parent       await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }

        public override async Task UpdateRowAsync(ItemAttributeVarietyLookupView updateVeiwEntity)
        {
            if (await IsDuplicateAsync(updateVeiwEntity))
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Attribute Variety Name: {updateVeiwEntity.VarietyName} - already exists, cannot be updated", "Exists already");
            else
            {
                if (IsValid(updateVeiwEntity))
                {
                    // update and check for errors 
                    if (await UpdateItemAsync(updateVeiwEntity) == UnitOfWork.CONST_WASERROR)
                    {
                        await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error updating Attribute Variety: {updateVeiwEntity.VarietyName} -  {AppUnitOfWork.GetErrorMessage()}", "Updating AttributeVariety Error");
                    }
                    else
                        await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Attribute Variety: {updateVeiwEntity.VarietyName} was updated.");
                }
                else
                {
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Attribute Variety Item {updateVeiwEntity.VarietyName} cannot be parent and child.", "Error updating");
                }
            }
            //-> done in parent await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }
        public override async Task<int> UpdateWooMappingAsync(ItemAttributeVarietyLookupView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooProductAttributeTermMap _updateWooProductAttributeTermMap = await GetWooMappedItemAsync(updatedViewEntity.ItemAttributeVarietyLookupId);
            if (_updateWooProductAttributeTermMap != null)
            {
                if (_updateWooProductAttributeTermMap.CanUpdate == updatedViewEntity.CanUpdateECommerceMap)
                {
                    // not necessary to display message.
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo AttributeVariety Map for AttributeVariety: {updatedViewEntity.VarietyName} has not changed, so was not updated?");
                }
                else
                {
                    _updateWooProductAttributeTermMap.CanUpdate = (bool)updatedViewEntity.CanUpdateECommerceMap;
                    IRepository<WooProductAttributeTermMap> WooProductAttributeTermMapRepository = AppUnitOfWork.Repository<WooProductAttributeTermMap>();
                    _recsUpdated = await WooProductAttributeTermMapRepository.UpdateAsync(_updateWooProductAttributeTermMap);
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Attribute Variety: {updatedViewEntity.VarietyName} was updated.");
                }
            }
            else
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute Variety Map for AttributeVariety: {updatedViewEntity.VarietyName} is no longer found, was it deleted?");

            await LoadAllViewItemsAsync();
            return _recsUpdated;
        }

        private async Task<WooProductAttributeTermMap> GetWooProductAttributeTermMapFromIdAsync(Guid? sourceWooEntityId)
        {
            if (sourceWooEntityId == null)
                return null;

            IRepository<WooProductAttributeTermMap> _wooProductAttributeTermMapRepo = AppUnitOfWork.Repository<WooProductAttributeTermMap>();
            WooProductAttributeTermMap _wooProductAttributeTermMap = await _wooProductAttributeTermMapRepo.GetByIdAsync(wam => wam.ItemAttributeVarietyLookupId == sourceWooEntityId);
            return _wooProductAttributeTermMap;
        }
        private async Task<int> DeleteWooAttributeVarietyLinkAsync(WooProductAttributeTermMap deleteWooProductAttributeTermMap)
        {
            IRepository<WooProductAttributeTermMap> _wooProductAttributeTermMapRepo = AppUnitOfWork.Repository<WooProductAttributeTermMap>();
            int _result = await _wooProductAttributeTermMapRepo.DeleteByEntityAsync(deleteWooProductAttributeTermMap);

            return _result;
        }
        private async Task<IWooProductAttributeTerm> GetIWooProductAttributeTermAsync()
        {
            WooAPISettings _wooAPISettings = await GetWooAPISettingsAsync();
            return new WooProductAttributeTerm(_wooAPISettings, AppLoggerManager);
        }
        private async Task<int> DeleteWooAttributeVarietyAsync(WooProductAttributeTermMap deleteWooProductAttributeTermMap)
        {
            int _result = UnitOfWork.CONST_WASERROR;  // if all goes well this will be change to the number of records returned

            IWooProductAttributeTerm _wooProductAttributeTermRepository = await GetIWooProductAttributeTermAsync();
            if (_wooProductAttributeTermRepository != null)
            {
                int _parentAtributeId = await GetAttributeTermParentIdAsync();
                string _TempWooCat = await _wooProductAttributeTermRepository.DeleteProductAttributeTermByIdAsync(deleteWooProductAttributeTermMap.WooProductAttributeTermId, _parentAtributeId);
                _result = (String.IsNullOrEmpty(_TempWooCat)) ? UnitOfWork.CONST_WASERROR : 1;  // return the id
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
            int _result = UnitOfWork.CONST_WASERROR;
            // delete the woo AttributeVariety
            WooProductAttributeTermMap _wooProductAttributeTermMap = await GetWooProductAttributeTermMapFromIdAsync(deleteWooEntityId);
            if (_wooProductAttributeTermMap == null)
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Product AttributeVariety Id {deleteWooEntityId} was not found to have a Woo AttributeVariety Map.");
            else
            {
                if (deleteFromWoo)
                {
                    _result = await DeleteWooAttributeVarietyAsync(_wooProductAttributeTermMap); ///Delete the AttributeVariety in Woo
                    if (_result == UnitOfWork.CONST_WASERROR)
                        await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Variety Id {_wooProductAttributeTermMap.WooProductAttributeTermId} was not deleted from Woo categories. Error {AppUnitOfWork.GetErrorMessage()}");
                    else
                        await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Variety Id {_wooProductAttributeTermMap.WooProductAttributeTermId} was deleted from Woo categories.");
                }
                _result = await DeleteWooAttributeVarietyLinkAsync(_wooProductAttributeTermMap);   //Delete our link data, if there was an error should we?
                if (_result == UnitOfWork.CONST_WASERROR)
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Variety Id {_wooProductAttributeTermMap.WooProductAttributeTermId} was not deleted from Woo linked categories. Error {AppUnitOfWork.GetErrorMessage()}");
                else
                    await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Variety Id {_wooProductAttributeTermMap.WooProductAttributeTermId} was deleted from Woo linked categories.");
            }
            return _result;
        }
        //private readonly Dictionary<OrderBys, string> AttriburteOrderByToWooOrerBy = new()
        //{
        //    {OrderBys.Id, "id" },
        //    {OrderBys.Name, "name" },
        //    {OrderBys.None,"menu_order" },
        //    {OrderBys.Number,"name_num" },
        //    {OrderBys.SortOrder,"menu_order"  }
        //};
        //private string ConvertToWooOrderBy(OrderBys inputValue)
        //{
        //    return AttriburteOrderByToWooOrerBy[inputValue];  // return the string equivalent of the enum OrderBys
        //}
        private async Task<ProductAttributeTerm> AddItemToWooOnlyAsync(ItemAttributeVarietyLookup addEntity)
        {
            IWooProductAttributeTerm _wooProductAttributeTermRepository = await GetIWooProductAttributeTermAsync();
            if (_wooProductAttributeTermRepository == null)
                return null;

            ProductAttributeTerm _wooProductAttributeTerm = new()
            {
                name = addEntity.VarietyName,
                //                 order_by = ConvertToWooOrderBy(addEntity.OrderBy),
            };
            int _parentAttributeId = await GetAttributeTermParentIdAsync();
            return await _wooProductAttributeTermRepository.AddProductAttributeTermAsync(_wooProductAttributeTerm, _parentAttributeId);  // should return a new version with ID
        }

        private async Task<WooProductAttributeTermMap> MapOurItemToWooItemAsync(ProductAttributeTerm newWooProductAttributeTerm, ItemAttributeVarietyLookup addViewEntity)
        {
            // create a map to the woo AttributeVariety maps using the id and the AttributeVariety
            //
            IRepository<WooProductAttributeTermMap> _wooProductAttributeTermMapRepository = AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            return await _wooProductAttributeTermMapRepository.AddAsync(new WooProductAttributeTermMap
            {
                ItemAttributeVarietyLookupId = addViewEntity.ItemAttributeVarietyLookupId,
                WooProductAttributeTermId = Convert.ToInt32(newWooProductAttributeTerm.id),
                CanUpdate = true,
            });
        }
        //private async Task<ProductAttributeVariety> GetWooProductAttributeTermByName(string VarietyName)
        //{
        //    IWooProductAttributeTerm _wooProductAttributeTermRepository = await GetIWooProductAttributeTerm();
        //    if (_wooProductAttributeTermRepository == null)
        //        return null;

        //    return _wooProductAttributeTermRepository.FindProductAttributeVarietyByName(VarietyName);
        //}

        /// <summary>
        /// Add the item to woo
        /// </summary>
        /// <param name="addEntity"></param>
        /// <returns></returns>
        public override async Task<WooProductAttributeTermMap> AddWooItemAndMapAsync(ItemAttributeVarietyLookup addEntity)
        {
            // check it the item exists in woo !!!!!!(we did not do this as there is no such call;, if so get is id and return, otherwise add it and get its id
            //ProductAttributeVariety _wooProductAttributeTerm = await GetWooProductAttributeTermByName(addEntity.VarietyName);
            //if (_wooProductAttributeTerm == null)
            //{
            ProductAttributeTerm _wooProductAttributeTerm = await AddItemToWooOnlyAsync(addEntity);
            if (_wooProductAttributeTerm == null)
                return null;
            return await MapOurItemToWooItemAsync(_wooProductAttributeTerm, addEntity);
            //}
            //else
            //    return AppUnitOfWork.CONST_WASERROR;
        }
        public override async Task<int> UpdateWooItemAsync(ItemAttributeVarietyLookupView updateViewEntity)
        {
            int _result = 0;  /// null or not found
            if ((updateViewEntity.HasECommerceAttributeVarietyMap) && ((bool)(updateViewEntity.CanUpdateECommerceMap)))
            {
                IWooProductAttributeTerm _wooProductAttributeTermRepository = await GetIWooProductAttributeTermAsync();
                if (_wooProductAttributeTermRepository != null)                     //  - > if it does not exist then what?
                {
                    WooProductAttributeTermMap _updateWooMapEntity = await GetWooProductAttributeTermMapFromIdAsync(updateViewEntity.ItemAttributeVarietyLookupId);
                    if (_updateWooMapEntity == null)
                    {
                        // need to add the AttributeVariety -> this is done later.
                        _result = (await AddWooItemAndMapAsync(updateViewEntity)) != null ? 1 : UnitOfWork.CONST_WASERROR;
                    }
                    else
                    {
                        int _parentAttributeId = await GetAttributeTermParentIdAsync();
                        ProductAttributeTerm _wooProductAttributeTerm = await _wooProductAttributeTermRepository.GetProductAttributeTermByIdAsync(_updateWooMapEntity.WooProductAttributeTermId, _parentAttributeId);
                        if (_wooProductAttributeTerm == null)
                        {
                            return UnitOfWork.CONST_WASERROR;  /// oops what happened >?
                        }
                        else
                        {
                            // only update if different
                            if (!_wooProductAttributeTerm.name.Equals(updateViewEntity.VarietyName)) //|| (!_wooProductAttributeTerm.order_by.Equals(updateViewEntity.OrderBy.ToString(), StringComparison.OrdinalIgnoreCase)))
                            {
                                _wooProductAttributeTerm.name = updateViewEntity.VarietyName;  // only update if necessary
                                                                                               //_wooProductAttributeTerm.order_by = ConvertToWooOrderBy(updateViewEntity.OrderBy);
                                var _res = ((await _wooProductAttributeTermRepository.UpdateProductAttributeTermAsync(_wooProductAttributeTerm, _parentAttributeId)));
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
        public override async Task<int> UpdateWooItemAndMappingAsync(ItemAttributeVarietyLookupView updateViewEntity)
        {
            int _result = await UpdateWooItemAsync(updateViewEntity);
            if (_result > 0)
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Updated woo AttributeVariety {updateViewEntity.VarietyName}.");
            else if (_result == UnitOfWork.CONST_WASERROR)
                await CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo AttributeVariety {updateViewEntity.VarietyName} update failed.");

            _result = await UpdateWooProductAttributeTermMapAsync(updateViewEntity);
            return _result;
        }

        public async Task<List<ItemUoMLookup>> GetItemUoMsAsync(List<Guid?> linkedItemUoMIDs)
        {
            if ((linkedItemUoMIDs == null) || (linkedItemUoMIDs.Count == 0))
                return null;

            IRepository<ItemUoMLookup> appRepository = AppUnitOfWork.Repository<ItemUoMLookup>();

            var _ItemUoMs = await appRepository.GetByAsync(iu => linkedItemUoMIDs.Contains(iu.ItemUoMLookupId));   // ItemAttributeVarietyLookupId
            return _ItemUoMs.ToList();
        }
        #endregion
    }
}
