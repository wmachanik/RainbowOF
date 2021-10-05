using AutoMapper;
using Blazorise.DataGrid;
using Blazorise.Extensions;
using Microsoft.AspNetCore.Components;
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
        #region Constructor
        public ItemWooLinkedViewRepository(ILoggerManager sourceLogger,
                                           IAppUnitOfWork sourceAppUnitOfWork,
                                           GridSettings sourceGridSettings,
                                           IMapper sourceMapper) : base(sourceLogger, sourceAppUnitOfWork, sourceGridSettings, sourceMapper)
        {
            //_logger = logger;
            //_appUnitOfWork = appUnitOfWork;
            //_gridSettings = gridSettings;
        }
        #endregion
        #region Support Methods
        //private Dictionary<OrderBys, string> WooOrderByToWooOrerBy = new Dictionary<OrderBys, string>
        //{
        //    {OrderBys.Id, "id" },
        //    {OrderBys.Name, "name" },
        //    {OrderBys.None,"menu_order" },
        //    {OrderBys.Number,"name_num" },
        //    {OrderBys.SortOrder,"menu_order"  }
        //};
        /// <summary>
        /// Add item to woo, after mapping it
        /// Logic: using the addedEntity map the relevant fields and add it to Woo
        /// </summary>
        /// <param name="addEntity">Entity to be added</param>
        /// <returns></returns>
        private async Task<Product> AddItemToWooOnlySync(Item addEntity)
        {
            IWooProduct _wooProductRepository = await GetIWooProductAsync();
            if (_wooProductRepository == null)
                return null;

            // copy Product items across that are used in Woo
            Product _wooProduct = new Product();

            _Mapper.Map(addEntity, _wooProduct);
            //Product _wooProduct = new Product
            //{
            //    name = addEntity.ItemName,
            //    //order_by = ConvertToWooOrderBy(addEntity.OrderBy),
            //};
            return await _wooProductRepository.AddProductAsync(_wooProduct);  // should return a new version with ID
        }
        /// <summary>
        /// Using the new woo product and new item add a mapping between the two in the mapping table
        /// Logic: map values across and add to mapping table
        /// </summary>
        /// <param name="newWooProduct">New Woo Product</param>
        /// <param name="addEntity">added Item</param>
        /// <returns>result of adding the entity</returns>
        private async Task<int> AddItemToWooItemMapAsync(Product newWooProduct, Item addEntity)
        {
            // create a map to the woo Attribute maps using the id and the Attribute
            //
            IAppRepository<WooProductMap> _wooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();

            return await _wooProductMapRepository.AddAsync(new WooProductMap
            {
                ItemId = addEntity.ItemId,
                WooProductId = Convert.ToInt32(newWooProduct.id),
                CanUpdate = true,
            });
        }
        /// <summary>
        /// Delete a product mapping in the Item to product Mapping table
        /// Logic: use repo to delete.
        /// </summary>
        /// <param name="deleteWooProductMap">The map that we want to delete.</param>
        /// <returns>number of records delete or error</returns>
        private async Task<int> DeleteWooProductMappingAsync(WooProductMap deleteWooProductMap)
        {
            int _result = 0;
            IAppRepository<WooProductMap> _wooProductMapRepo = _AppUnitOfWork.Repository<WooProductMap>();
            _result = await _wooProductMapRepo.DeleteByIdAsync(deleteWooProductMap.WooProductMapId);
            return _result;
        }
        private WooAPISettings _wooAPISettings { get; set; } = null;
        private IWooProduct _WooProduct { get; set; } = null;
        /// <summary>
        /// Get and interface to the Woo Product API
        /// </summary>
        /// <returns>IWooProduct instance</returns>
        private async Task<IWooProduct> GetIWooProductAsync()
        {
            if (_wooAPISettings == null)
                _wooAPISettings = await GetWooAPISettingsAsync();

            if (_WooProduct == null)
                _WooProduct = new WooProduct(_wooAPISettings, _Logger);
            return _WooProduct;
        }
        /// <summary>
        /// Delete a woo product.
        /// Logic: Using the Id delete
        /// </summary>
        /// <param name="deleteWooProductMap">Woo Product mapping</param>
        /// <returns>id of item delete or error</returns>
        private async Task<int> DeleteWooProductAsync(WooProductMap deleteWooProductMap)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;  // if all goes well this will be change to the number of records returned

            IWooProduct _wooProductRepository = await GetIWooProductAsync();
            if (_wooProductRepository != null)
            {
                Product _TempWooCat = await _wooProductRepository.DeleteProductByIdAsync(deleteWooProductMap.WooProductId);
                _result = (_TempWooCat == null) ? AppUnitOfWork.CONST_WASERROR : Convert.ToInt32(_TempWooCat.id);  // return the id
            }
            return _result;
        }
        #endregion
        #region Interface Methods
        private List<ItemView> _oldSelectedItems = null;
        /// <summary>
        /// Saves the current selected items, so they can be retrieved later with Pop
        /// Logic: Store current selected items for later retrieval
        /// </summary>
        /// <param name="currentSelectedItems">Current selected items</param>
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
        /// <summary>
        /// Retrieves the last pushed selected items.
        /// Logic: Using the stored list sew if those items exists still and add them to a selected list
        /// </summary>
        /// <param name="modelViewItems">Current model view items</param>
        /// <returns>items that were popped if they exist</returns>
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
        /// <summary>
        /// Search and return for the woo mapped item
        /// Logic: Find the first Woo item mapped to Item Guid mapped to a Product
        /// </summary>
        /// <param name="mapWooEntityID">Item Id to search for.</param>
        /// <returns>WooProductMap if it exists otherwise null</returns>
        public override async Task<WooProductMap> GetWooMappedItemAsync(Guid mapWooEntityID)
        {
            IAppRepository<WooProductMap> _wooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductMap>();

            return await _wooAttributeMapRepository.FindFirstAsync(wcm => wcm.ItemId == mapWooEntityID);
        }
        /// <summary>
        /// Search and return for the woo mapped items using the list sent in.
        /// Logic: Get all mapped entities from the WooProductMapRepo using the Contains condition
        /// </summary>
        /// <param name="mapWooEntityIDs">All the Item Ids to search for.</param>
        /// <returns>List of Product Mappings found – null if none</returns>
        public override async Task<List<WooProductMap>> GetWooMappedItemsAsync(List<Guid> mapWooEntityIDs)
        {
            IAppRepository<WooProductMap> _wooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();
            var _WooMappedItems = await _wooProductMapRepository.GetByAsync(wam => mapWooEntityIDs.Contains(wam.ItemId));   // ItemId
            return _WooMappedItems.ToList();
        }
        /// <summary>
        /// Return all the items as per the specific data a grid settings.
        /// Logic: Mainly relies on the ItemRepo implementation which uses the parameters to apply search, filter and paging info to an eager load.
        /// </summary>
        /// <param name="currentDataGridParameters">current data grid parameters</param>
        /// <returns>List of Items as per parameters – null if none</returns>
        public override async Task<List<Item>> GetPagedItemsAsync(DataGridParameters currentDataGridParameters)    //int startPage, int currentPageSize)
        {
            IItemRepository _ItemRepository = _AppUnitOfWork.itemRepository();
            DataGridItems<Item> _dataGridItems = await _ItemRepository.GetPagedDataEagerWithFilterAndOrderByAsync(currentDataGridParameters);
            List<Item> _Items = _dataGridItems.Entities.ToList();
            _GridSettings.TotalItems = _dataGridItems.TotalRecordCount;
            return _Items;
        }
        /// <summary>
        /// Retrieve the current data grid parameters using the values as per set by user and user interaction
        /// Logic: get current DataGridParamters from UI paging info and add any filter requirements
        /// </summary>
        /// <param name="inputDataGridReadData">Current grid parameters specific to the ItemView</param>
        /// <param name="inputCustomerFilter">the string that the user has entered into the general filter</param>
        /// <returns></returns>
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
                    if (col.SortDirection != Blazorise.SortDirection.None)
                    {
                        if (_dataGridParameters.SortParams == null)
                            _dataGridParameters.SortParams = new List<SortParam>();
                        _dataGridParameters.SortParams.Add(new SortParam
                        {
                            FieldName = col.Field,
                            Direction = col.SortDirection
                        });
                    }
                    if (!string.IsNullOrEmpty((string)col.SearchValue))
                    {
                        if (_dataGridParameters.FilterParams == null)
                            _dataGridParameters.FilterParams = new List<FilterParam>();
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
        /// <summary>
        /// Load all the item and view item data into a single list to be displayed in the UI
        /// Logic: Get all paged items using the user selected options. Add the Woo related data specific to if we are integrated with a REST API
        /// </summary>
        /// <param name="currentDataGridParameters">Current grid parameters</param>
        /// <returns>List of Items with View items added</returns>
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
                WooProductMap _wooProductMap = WooProductMaps.Where(wam => wam.ItemId == entity.ItemId).FirstOrDefault();    //  if retrieving / record await GetWooMappedItemAsync(itemCat.ItemId);

                ItemView _itemView = new();
                _Mapper.Map(entity, _itemView);
                _itemView.CanUpdateECommerceMap = (_wooProductMap == null) ? null : _wooProductMap.CanUpdate;
                _itemViewLookups.Add(_itemView);

                ///*
                //_itemViewLookups.Add(new ItemView
                //{
                //    ItemId = entity.ItemId,
                //    ItemName = entity.ItemName,
                //    SKU = entity.SKU,
                //    IsEnabled = entity.IsEnabled,
                //    ItemDetail = entity.ItemDetail,
                //    PrimaryItemCategoryLookupId = ((entity.PrimaryItemCategoryLookupId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.PrimaryItemCategoryLookupId,
                //    ParentItemId = ((entity.ParentItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.ParentItemId,
                //    ReplacementItemId = ((entity.ReplacementItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.ReplacementItemId,
                //    ItemAbbreviatedName = entity.ItemAbbreviatedName,
                //    ParentItem = entity.ParentItem,
                //    ReplacementItem = entity.ReplacementItem,
                //    ItemCategories = entity.ItemCategories,
                //    ItemAttributes = entity.ItemAttributes,
                //    ItemImages = entity.ItemImages,
                //    SortOrder = entity.SortOrder,
                //    BasePrice = entity.BasePrice,
                //    ManageStock = entity.ManageStock,
                //    QtyInStock = entity.QtyInStock,
                //    CanUpdateWooMap = (_wooProductMap == null) ? null : _wooProductMap.CanUpdate
                //}); 
                //*/
            }
            return _itemViewLookups;
        }
        /// <summary>
        ///  Initialise the new item with default values. These values are then overwritten by the user.
        /// Logic: Set each part of the Item record to a default value.
        /// </summary>
        /// <param name="newViewEntity">blank Item passed in</param>
        /// <returns>ItemView that is initialised.</returns>
        public override ItemView NewItemDefaultSetter(ItemView newViewEntity)
        {
            if (newViewEntity == null)
                newViewEntity = new ItemView();

            newViewEntity.ItemName = "Item (must be unique)";
            newViewEntity.SKU = "SKU (must be unique)";
            newViewEntity.IsEnabled = true;
            newViewEntity.ItemDetail = "Item description / detail";
            newViewEntity.ItemAbbreviatedName = "ItmAbv2Use";  // use the date string
            newViewEntity.SortOrder = 0;
            newViewEntity.ManageStock = false;
            newViewEntity.QtyInStock = 0;
            newViewEntity.BasePrice = decimal.Zero;
            newViewEntity.CanUpdateECommerceMap = null;
            newViewEntity.ItemType = ItemTypes.Simple;
            newViewEntity.ItemAttributes = null;
            newViewEntity.ItemCategories = null;
            return newViewEntity;
        }
        /// <summary>
        /// Is the Item a duplicate of an item that already exists
        /// Logic: Check in an item with that name or SKU exists, if so return true
        /// </summary>
        /// <param name="targetEntity">the item about to be added.</param>
        /// <returns>true if it exists</returns>
        public override async Task<bool> IsDuplicateAsync(Item targetEntity)
        {
            // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
            IAppRepository<Item> _ItemRepository = _AppUnitOfWork.Repository<Item>();
            var _exists = (await _ItemRepository.GetByAsync(itm => (itm.ItemName == targetEntity.ItemName) || (itm.SKU == targetEntity.SKU))).ToList();
            return ((_exists != null) && (_exists.Count > 1));
        }
        /// <summary>
        /// Is the item a valid item.
        /// Logic: check all required fields are there
        /// </summary>
        /// <param name="checkEntity">Item to check</param>
        /// <returns></returns>
        public override bool IsValid(Item checkEntity)
        {
            return !string.IsNullOrWhiteSpace(checkEntity.ItemName); // (checkEntity.ParentAttributeId != checkEntity.ItemId);
        }
        /// <summary>
        /// Perform the selected group action on the selected items
        /// Logic: Perform action - dependant of the Group Action specified
        /// </summary>
        /// <param name="targetVeiwEntity">Target item with view additions to apply action to</param>
        /// <param name="selectedAction">the selected action</param>
        /// <returns></returns>
        public override async Task<int> DoGroupActionAsync(ItemView targetVeiwEntity, BulkAction selectedAction)
        {
            switch (selectedAction)
            {
                case BulkAction.AllowWooSync:
                    targetVeiwEntity.CanUpdateECommerceMap = true;
                    break;
                case BulkAction.DisallowWooSync:
                    targetVeiwEntity.CanUpdateECommerceMap = false;
                    break;
                default:
                    return 0;
            }
            return await UpdateWooMappingAsync(targetVeiwEntity);
        }
        /// <summary>
        /// Takes the Item of Type Item View and returns the mapped item
        /// Logic: Use mapper to map across values to a new item
        /// </summary>
        /// <param name="fromVeiwEntity">the View Item to map from.</param>
        /// <returns>new item with the mapped values</returns>
        public override Item GetItemFromView(ItemView fromVeiwEntity)
        {
            Item _newItem = new Item();
            _Mapper.Map(fromVeiwEntity, _newItem);
            //Item _newItem = new Item
            //{
            //    ItemId = fromVeiwEntity.ItemId,
            //    ItemName = fromVeiwEntity.ItemName,
            //    SKU = fromVeiwEntity.SKU,
            //    IsEnabled = fromVeiwEntity.IsEnabled,
            //    ItemDetail = fromVeiwEntity.ItemDetail,
            //    PrimaryItemCategoryLookupId = ((fromVeiwEntity.PrimaryItemCategoryLookupId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)fromVeiwEntity.PrimaryItemCategoryLookupId,
            //    ReplacementItemId = ((fromVeiwEntity.ReplacementItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)fromVeiwEntity.ReplacementItemId,
            //    ItemAbbreviatedName = fromVeiwEntity.ItemAbbreviatedName,
            //    SortOrder = fromVeiwEntity.SortOrder,
            //    BasePrice = fromVeiwEntity.BasePrice,

            //    //ParentItemId = ((fromVeiwEntity.ParentItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)fromVeiwEntity.ParentItemId,
            //    ////????? Lists and other lazy loads?
            //};
            return _newItem;
        }
        /// <summary>
        /// Insert the new item that user has added. Displaying relevant messages along the way
        /// Logic: if the item does not exist then add it. If the integration (eg. Woo) mapping is enabled then add the item to the integrated system and mapping table. 
        /// </summary>
        /// <param name="newVeiwEntity">The new Item View that has been created and needs to be saved.</param>
        /// <returns>void</returns>
        public override async Task InsertRowAsync(ItemView newVeiwEntity)
        {
            IAppRepository<Item> _ItemRepository = _AppUnitOfWork.Repository<Item>();
            // first check we do not already have a Attribute like this.
            if (await _ItemRepository.FindFirstAsync(ial => ial.ItemName == newVeiwEntity.ItemName) == null)
            {
                Item _NewItem = GetItemFromView(newVeiwEntity); // store this here since when it is added it will automatically update the id field
                int _recsAdded = await _ItemRepository.AddAsync(_NewItem);
                if (_recsAdded != AppUnitOfWork.CONST_WASERROR)
                {
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.ItemName} - added", "Attribute Added");
                    if (newVeiwEntity.CanUpdateECommerceMap ?? false)
                    {
                        // they selected to update woo so add to Woo
                        if (await AddWooItemAndMapAsync(_NewItem) == AppUnitOfWork.CONST_WASERROR)   // add if they select to update
                            _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding {newVeiwEntity.ItemName} to Woo - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Woo Attribute");
                        else
                            _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.ItemName} - added to Woo", "Woo Attribute Added");
                    }
                }
                else
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.ItemName} - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Attribute");
            }
            else
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.ItemName} already exists, so could not be added.");
            //-> done in parent       await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }
        /// <summary>
        /// Find a Woo Product Mapping that is mapped to the source Item's Id
        /// Logic: Check if a mapping exists related to the Item's Id passed in 
        /// </summary>
        /// <param name="sourceWooEntityId">Source Item's Id </param>
        /// <returns>WooProductMap of item found or null if not</returns>
        private async Task<WooProductMap> GetWooProductMapFromIDAsync(Guid? sourceWooEntityId)
        {
            if (sourceWooEntityId == null)
                return null;

            IAppRepository<WooProductMap> _wooProductMapRepo = _AppUnitOfWork.Repository<WooProductMap>();
            WooProductMap _wooProductMap = await _wooProductMapRepo.FindFirstAsync(wam => wam.ItemId == sourceWooEntityId);
            return _wooProductMap;
        }
        /// <summary>
        /// Delete a Woo Product from the mapped table and Woo, if they ask us to 
        /// </summary>
        /// <param name="WooEntityId">Id to delete</param>
        /// <param name="deleteFromWoo">True of we want to delete from Woo too</param>
        /// <returns></returns>
        public override async Task<int> DeleteWooItemAsync(Guid deleteWooEntityId, bool deleteFromWoo)
        {
            int _result = AppUnitOfWork.CONST_WASERROR;
            // delete the woo Attribute
            WooProductMap _wooProductMap = await GetWooProductMapFromIDAsync(deleteWooEntityId);
            if (_wooProductMap == null)
                _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {deleteWooEntityId} was not found to have a Woo Attribute Map.");
            else
            {
                if (deleteFromWoo)
                {
                    _result = await DeleteWooProductAsync(_wooProductMap); ///Delete the Attribute in Woo
                    if (_result == AppUnitOfWork.CONST_WASERROR)
                        _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {_wooProductMap.WooProductId} was not deleted from Woo categories. Error {_AppUnitOfWork.GetErrorMessage()}");
                    else
                        _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Id {_wooProductMap.WooProductId} was deleted from Woo categories.");
                }
                _result = await DeleteWooProductMappingAsync(_wooProductMap);   //Delete our link data, if there was an error should we?
                if (_result == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {_wooProductMap.WooProductId} was not deleted from Woo linked categories. Error {_AppUnitOfWork.GetErrorMessage()}");
                else
                    _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Id {_wooProductMap.WooProductId} was deleted from Woo linked categories.");
            }
            return _result;
        }
        //private string ConvertToWooOrderBy(OrderBys inputValue)
        //{
        //    return WooOrderByToWooOrerBy[inputValue];  // return the string equivalent of the enum OrderBys
        //}
        //private Product MapItemToWooProduct(Item sourceEntity)
        //{
        //    Product _wooProduct = new Product();
        //    return _wooProduct;
        //}

        //private async Task<Product> GetWooProductByName(string ItemName)
        //{
        //    IWooProduct _wooProductRepository = await GetIWooProduct();
        //    if (_wooProductRepository == null)
        //        return null;

        //    return _wooProductRepository.FindProductByName(ItemName);
        //}

        /// <summary>
        /// Add the item to woo and map that item to the item passed in
        /// Logic: Add the item to Woo, if successful add it to the ItemToWoo mapping table
        /// </summary>
        /// <param name="addEntity">Item that we have added to the system.</param>
        /// <returns>number of records added, or error</returns>
        public override async Task<int> AddWooItemAndMapAsync(Item addEntity)
        {
            // check it the item exists in woo !!!!!!(we did not do this as there is no such call;, if so get is id and return, otherwise add it and get its id

            //Product _wooProduct = await GetWooProductByName(addEntity.ItemName);
            //if (_wooProduct == null)
            //{
            Product _wooProduct = await AddItemToWooOnlySync(addEntity);
            if (_wooProduct == null)
                return AppUnitOfWork.CONST_WASERROR;
            return await AddItemToWooItemMapAsync(_wooProduct, addEntity);
            //}
            //else
            //    return AppUnitOfWork.CONST_WASERROR;
        }
        /// <summary>
        /// Delete the item passed in from the Item table
        /// Logic: Do the delete using the item id. if success display that otherwise display error
        /// </summary>
        /// <param name="deleteViewEntity">Item View entity to delete</param>
        /// <returns>void</returns>
        public override async Task DeleteRowAsync(ItemView deleteViewEntity)
        {
            IAppRepository<Item> _itemRepository = _AppUnitOfWork.Repository<Item>();

            var _recsDelete = await _itemRepository.DeleteByIdAsync(deleteViewEntity.ItemId);

            if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Item: {deleteViewEntity.ItemName} is no longer found, was it deleted?");
            else
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {deleteViewEntity.ItemName} was it deleted?");
        }
        /// <summary>
        /// Update whether the woo Product map for the view item can be updated, dependant on the current setting
        /// Logic: Retrieve item from database. If it exists and the status has changed (i.e we can update, but we now we cannot and vice versa) then update the can map status. If it did not exists add the mapping.
        /// </summary>
        /// <param name="updatedViewEntity">Item view we want to update if required.</param>
        /// <returns>number of records updated, or error</returns>
        public override async Task<int> UpdateWooMappingAsync(ItemView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooProductMap updateWooProductMap = await GetWooMappedItemAsync(updatedViewEntity.ItemId);
            if (updateWooProductMap != null)
            {
                if (updateWooProductMap.CanUpdate == updatedViewEntity.CanUpdateECommerceMap)
                {
                    // not necessary to display message.
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo Attribute Map for Attribute: {updatedViewEntity.ItemName} has not changed, so was not updated?");
                }
                else
                {
                    updateWooProductMap.CanUpdate = (bool)updatedViewEntity.CanUpdateECommerceMap;
                    IAppRepository<WooProductMap> WooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();
                    _recsUpdated = await WooProductMapRepository.UpdateAsync(updateWooProductMap);
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {updatedViewEntity.ItemName} was updated.");
                }
            }
            else
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Map for Item: {updatedViewEntity.ItemName} is no longer found, was it deleted?");
            await LoadAllViewItemsAsync();
            return _recsUpdated;
        }
        //async Task<int> UpdateWooProductMap(ItemView updatedViewEntity)
        //{
        //    int _recsUpdated = 0;

        //    WooProductMap _updateWooProductMap = await GetWooMappedItemAsync(updatedViewEntity.ItemId);
        //    if (_updateWooProductMap != null)
        //    {
        //        if (_updateWooProductMap.CanUpdate == updatedViewEntity.CanUpdateECommerceMap)
        //        {
        //        }
        //        else
        //        {
        //            _updateWooProductMap.CanUpdate = (bool)updatedViewEntity.CanUpdateECommerceMap;
        //            IAppRepository<WooProductMap> WooProductRepository = _AppUnitOfWork.Repository<WooProductMap>();
        //            _recsUpdated = await WooProductRepository.UpdateAsync(_updateWooProductMap);
        //            _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {updatedViewEntity.ItemName} was updated.");
        //        }
        //    }
        //    else
        //    {
        //        // nothing was found, so this probably means they now want to add. We should had a Pop up to check
        //        int _result = await AddWooItemAndMapAsync(GetItemFromView(updatedViewEntity));
        //        if (_result != AppUnitOfWork.CONST_WASERROR)
        //            _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item {updatedViewEntity.ItemName} has been added to woo?");
        //    }
        //    return _recsUpdated;
        //}
        /// <summary>
        /// Update the woo product item using the view item that was edited.
        /// Logic: If the updated item has a attribute map and can be updated then Retrieve the product form Woo using the item id. If it does not exist add it. If it does update it if the name has changed.
        /// </summary>
        /// <param name="updatedViewEntity">Item view we want to update, if required.</param>
        /// <returns></returns>
        public override async Task<int> UpdateWooItemAsync(ItemView updateViewEntity)
        {
            int _result = 0;  /// null or not found
            if ((updateViewEntity.HasECommerceAttributeMap) && ((bool)(updateViewEntity.CanUpdateECommerceMap)))
            {
                IWooProduct _wooProductRepository = await GetIWooProductAsync();
                if (_wooProductRepository != null)                     //  - > if it does not exist then what?
                {
                    WooProductMap _updateWooMapEntity = await GetWooProductMapFromIDAsync(updateViewEntity.ItemId);
                    if (_updateWooMapEntity == null)
                    {
                        // need to add the Attribute -> this is done later.
                        _result = await AddWooItemAndMapAsync(updateViewEntity);
                    }
                    else
                    {
                        Product _wooProduct = await _wooProductRepository.GetProductByIdAsync(_updateWooMapEntity.WooProductId);
                        if (_wooProduct == null)
                        {
                            return AppUnitOfWork.CONST_WASERROR;  /// oops what happened >?
                        }
                        else
                        {
                            // only update if different
                            if ((!_wooProduct.name.Equals(updateViewEntity.ItemName))) //|| (!_wooProduct.order_by.Equals(updateViewEntity.OrderBy.ToString(), StringComparison.OrdinalIgnoreCase)))
                            {
                                _wooProduct.name = updateViewEntity.ItemName;  // only update if necessary
                                //_wooProduct.order_by = ConvertToWooOrderBy(updateViewEntity.OrderBy);
                                var _res = ((await _wooProductRepository.UpdateProductAsync(_wooProduct)));
                                _result = ((_res == null) || (_res.id == null)) ? AppUnitOfWork.CONST_WASERROR : (int)_res.id; // if null there is an issue
                            }
                        }
                    }
                }
            }
            return _result;
        }
        /// <summary>
        /// Checks if the any of the Woo link values where changed during an edit, if so update.
        /// Logic: Update the view item; if it is updated, then show a success message, otherwise an error. Update the Mapping, return the result of Mapping.
        /// </summary>
        /// <param name="updateViewEntity">The Entity that is being updated</param>
        /// <returns>null if nothing changed or the new WooCategopryMap</returns>
        public override async Task<int> UpdateWooItemAndMappingAsync(ItemView updateViewEntity)
        {
            int _result = await UpdateWooItemAsync(updateViewEntity);
            if (_result > 0)
                _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Updated woo Attribute {updateViewEntity.ItemName}.");
            else if (_result == AppUnitOfWork.CONST_WASERROR)
                _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute {updateViewEntity.ItemName} update failed.");

            _result = await UpdateWooMappingAsync(updateViewEntity);
            return _result;
        }
        /// <summary>
        /// Update the Item using the View Item data we have from the system
        /// Logic: Retrieve the item by Id; if it does not exist, show error, otherwise map data across and update in the database.
        /// </summary>
        /// <param name="updateViewItem">Item view we want to update.</param>
        /// <returns>number of records updated, or zero means item no longer there. OR return Error if there was one.</returns>
        public override async Task<int> UpdateItemAsync(ItemView updateViewItem)
        {
            int _recsUpdted = 0;
            IAppRepository<Item> _ItemRepository = _AppUnitOfWork.Repository<Item>();
            // first check it exists - it could have been deleted 
            Item _CurrentItem = await _ItemRepository.GetByIdAsync(updateViewItem.ItemId);
            if (_CurrentItem == null)
            {
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Item: {updateViewItem.ItemName} is no longer found, was it deleted?");
                return AppUnitOfWork.CONST_WASERROR;
            }
            else
            {
                _Mapper.Map(updateViewItem, _CurrentItem);
                //_CurrentItem.ItemName = updateViewItem.ItemName;
                //_CurrentItem.SKU = updateViewItem.SKU;
                //_CurrentItem.IsEnabled = updateViewItem.IsEnabled;
                //_CurrentItem.ItemDetail = updateViewItem.ItemDetail;
                _CurrentItem.PrimaryItemCategoryLookupId = ((updateViewItem.PrimaryItemCategoryLookupId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)updateViewItem.PrimaryItemCategoryLookupId;
                _CurrentItem.ReplacementItemId = ((updateViewItem.ReplacementItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)updateViewItem.ReplacementItemId;
                //_CurrentItem.ItemAbbreviatedName = updateViewItem.ItemAbbreviatedName;
                //_CurrentItem.SortOrder = updateViewItem.SortOrder;
                //_CurrentItem.BasePrice = updateViewItem.BasePrice;
                //_CurrentItem.ParentItemId = ((updateViewItem.ParentItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)updateViewItem.ParentItemId;
                ////????? Lists and other lazy loads?

                _recsUpdted = await _ItemRepository.UpdateAsync(_CurrentItem);
                if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.ItemName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating Attribute");
                if (await UpdateWooItemAndMappingAsync(updateViewItem) == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateViewItem.ItemName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating Attribute Map");   // should we send a message here error = mapping not updated 

            }
            return _recsUpdted;
        }
        /// <summary>
        /// Master update for the view entity, that calls all the others.
        /// Logic: Check if the item is valid, if so update. If successful, then display message or if there was an error then and error message.
        /// </summary>
        /// <param name="updateVeiwEntity">Item view we want to update.</param>
        /// <returns>void</returns>
        public override async Task UpdateRowAsync(ItemView updateVeiwEntity)
        {
            //if (await IsDuplicateAsync(updateVeiwEntity))
            //    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Name: {updateVeiwEntity.ItemName} - already exists, cannot be updated", "Exists already");
            //else
            //{
            if (IsValid(updateVeiwEntity))
            {
                // update and check for errors 
                if (await UpdateItemAsync(updateVeiwEntity) == AppUnitOfWork.CONST_WASERROR)
                {
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating Attribute: {updateVeiwEntity.ItemName} -  {_AppUnitOfWork.GetErrorMessage()}", "Updating Attribute Error");
                }
                else
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {updateVeiwEntity.ItemName} was updated.");
            }
            else
            {
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Item {updateVeiwEntity.ItemName} cannot be parent and child.", "Error updating");
            }
            //}
            //-> done in parent await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }
        #endregion
    }
}
