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
using RainbowOF.Repositories.Integrations;
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
                                           //GridSettings sourceGridSettings,
                                           IMapper sourceMapper) : base(sourceLogger, sourceAppUnitOfWork, /*sourceGridSettings,*/ sourceMapper)
        {
            //_logger = logger;
            //_appUnitOfWork = appUnitOfWork;
            //_WooLinkedGridSettings = gridSettings;
            sourceLogger.LogDebug("ItemWooLinkedViewRepository initialised.");
        }
        #endregion
        #region Support Methods
        /// <summary>
        /// Add categories in the addedEntity to the targetProduct using the Woo Mappings we have. This is only called when a complete product is added. 
        /// Logic: If not null get the mapping using our current Item Category GUID so we can use the Woo values
        /// </summary>
        /// <param name="addEntity">The Item just to be added to the database.</param>
        /// <param name="targetProduct">The current Target Product, which we add to and return</param>
        /// <returns>modified (if required) product</returns>
        private async Task<Product> AddItemCategoriesToWooProductSync(Item addEntity, Product targetProduct)
        {
            if (addEntity.ItemCategories != null)
            {
                _Logger.LogInfo($"Adding {addEntity.ItemCategories.Count} categories to {addEntity.ItemName} Product to Woo...");
                IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();
                WooCategoryMap _thisProductCategory = null;
                foreach (var _itemCategory in addEntity.ItemCategories)
                {
                    if (_itemCategory.ItemCategoryDetail != null)
                    {
                        // Get Woo Category from our Id
                        _thisProductCategory = await _wooCategoryMapRepository.FindFirstByAsync(wcm => wcm.ItemCategoryLookupId == _itemCategory.ItemCategoryLookupId);
                        targetProduct.categories.Add(new ProductCategoryLine
                        {
                            id = _thisProductCategory.WooCategoryId,
                            name = _itemCategory.ItemCategoryDetail.CategoryName
                        });
                    }
                }
            }
            return targetProduct;
        }
        /// <summary>
        /// Add attributes in the addedEntity to the targetProduct using the Woo Mappings we have. This is only called when a complete product is added. 
        /// Logic: If not null get the mapping using our current Item Category GUID so we can use the Woo values
        /// </summary>
        /// <param name="addEntity">The Item just to be added to the database.</param>
        /// <param name="targetProduct">The current Target Product, which we add to and return</param>
        /// <returns>modified (if required) product</returns>
        private async Task<Product> AddItemAttributesToWooProductSync(Item addEntity, Product targetProduct)
        {
            if (addEntity.ItemAttributes != null)
            {
                _Logger.LogInfo($"Adding {addEntity.ItemAttributes.Count} attributes to {addEntity.ItemName} Product to Woo...");
                IAppRepository<WooProductAttributeMap> _wooProductAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();
                WooProductAttributeMap _thisProductAttributeMap = null;
                foreach (var _itemAttribute in addEntity.ItemAttributes)
                {
                    //use our values to get the mapped values and assign the attribute and terms
                    if (_itemAttribute.ItemAttributeDetail != null)
                    {
                        // Get Woo Attributes from our Id
                        _thisProductAttributeMap = await _wooProductAttributeMapRepository.FindFirstByAsync(wpam => wpam.ItemAttributeLookupId == _itemAttribute.ItemAttributeLookupId);
                        if (_thisProductAttributeMap != null)
                        {
                            ProductAttributeLine _productAttribute = new ProductAttributeLine
                            {
                                id = (uint)_thisProductAttributeMap.WooProductAttributeId,
                                name = _itemAttribute.ItemAttributeDetail.AttributeName,
                                variation = _itemAttribute.IsUsedForItemVariety,
                            };
                            foreach (var _itemAttributeVariety in _itemAttribute.ItemAttributeVarieties)
                            {
                                _productAttribute.options.Add(_itemAttributeVariety.ItemAttributeVarietyDetail.VarietyName);
                            }
                            targetProduct.attributes.Add(_productAttribute);
                        }
                    }
                }
            }
            return targetProduct;
        }
        /// <summary>
        /// Add item to woo, after mapping it. Rev 1.1 – Added support to copy across Attributes and Categories from item.
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
            // copy across an Items Attributes and Categories
            _wooProduct = await AddItemCategoriesToWooProductSync(addEntity, _wooProduct);
            _wooProduct = await AddItemAttributesToWooProductSync(addEntity, _wooProduct);

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
        private async Task<WooProductMap> AddItemToWooItemMapAsync(Product newWooProduct, Item addEntity)
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
            _result = await _wooProductMapRepo.DeleteByPrimaryIdAsync(deleteWooProductMap.WooProductMapId);
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
        #region Item Options Support 
        /// <summary>
        /// Add a source ItemCategory to the Product passed in
        /// </summary>
        /// <param name="sourceItemCategory">source item category</param>
        /// <param name="originalProduct">the product we will modify</param>
        /// <returns></returns>
        public async Task<Product> AddItemCategoriesToWooProduct(ItemCategory sourceItemCategory, Product originalProduct)
        {
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();
            // Get Woo Category from our Id
            WooCategoryMap _thisProductCategory = await _wooCategoryMapRepository.FindFirstByAsync(wcm => wcm.ItemCategoryLookupId == sourceItemCategory.ItemCategoryLookupId);
            if (_thisProductCategory != null)
                originalProduct.categories.Add(new ProductCategoryLine
                {
                    id = _thisProductCategory.WooCategoryId,
                    name = sourceItemCategory.ItemCategoryDetail.CategoryName
                });
            return originalProduct;
        }
        /// <summary>
        /// Here we check if any categories have been added or removed from the WooProduct.Categories. If so we set the HasChanged to true and modify the sourceProduct
        /// Logic: For each Item Category name check if the name exists as a category.If not add it.Then for each product category check if that exist, if not delete it.
        /// </summary>
        /// <param name="originalProduct">The Woo Product we are working with</param>
        /// <param name="sourceItem">Entity / Item to be added.</param>
        /// <returns>A class that return true if changes, and includes the changed product.</returns>
        public async Task<ClassHasChanged<Product>> CheckIfCategoriesHaveChanged(Product originalProduct, Item sourceItem)
        {
            ClassHasChanged<Product> _classHasChanged = new ClassHasChanged<Product>
            {
                HasChanged = false,
                Entity = originalProduct
            };
            if (sourceItem.ItemCategories != null)
            {
                foreach (var _itemCat in sourceItem.ItemCategories)
                {
                    if (!originalProduct.categories.Exists(cat => cat.name == _itemCat.ItemCategoryDetail.CategoryName))
                    {
                        // the name does not exist so it we must add it
                        _classHasChanged.Entity = await AddItemCategoriesToWooProduct(_itemCat, _classHasChanged.Entity);
                        _classHasChanged.HasChanged = true;
                    }
                }
                foreach (var _prodCat in originalProduct.categories)
                {
                    if (!sourceItem.ItemCategories.Exists(ic => ic.ItemCategoryDetail.CategoryName == _prodCat.name))
                    {
                        // the name does not exist so it we must add it
                        _classHasChanged.Entity.categories.Remove(_prodCat);
                        //_classHasChanged.Entity = await DeleteItemCategoriesToWooProduct(sourceItem, _classHasChanged.Entity);
                        _classHasChanged.HasChanged = true;
                    }
                }
            }
            return _classHasChanged;
        }
        /// <summary>
        /// Add a source ItemAttribute to the Product passed in
        /// </summary>
        /// <param name="sourceItemAttribute">source item Attribute</param>
        /// <param name="originalProduct">the product we will modify</param>
        /// <returns></returns>
        public async Task<Product> AddItemAttributesToWooProduct(ItemAttribute sourceItemAttribute, Product originalProduct)
        {
            IAppRepository<WooProductAttributeMap> _wooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();
            // Get Woo Attribute from our Id
            WooProductAttributeMap _thisProductAttribute = await _wooAttributeMapRepository.FindFirstByAsync(wcm => wcm.ItemAttributeLookupId == sourceItemAttribute.ItemAttributeLookupId);
            if (_thisProductAttribute != null)
                originalProduct.attributes.Add(new ProductAttributeLine
                {
                    id = (uint)_thisProductAttribute.WooProductAttributeId,
                    name = sourceItemAttribute.ItemAttributeDetail.AttributeName,
                    variation = sourceItemAttribute.IsUsedForItemVariety,
                    options = sourceItemAttribute.ItemAttributeVarieties.Select(iav => iav.ItemAttributeVarietyDetail.VarietyName).ToList(),   // copy all the options
                });
            return originalProduct;
        }
        /// <summary>
        /// Here we check if any Attributes have been added or removed from the WooProduct.Attributes. If so we set the HasChanged to true and modify the sourceProduct
        /// Logic: For each Item Attribute name check if the name exists as a Attribute.If not add it.Then for each product Attribute check if that exist, if not delete it.
        /// </summary>
        /// <param name="originalProduct">The Woo Product we are working with</param>
        /// <param name="sourceItem">Entity / Item to be added.</param>
        /// <returns>A class that return true if changes, and includes the changed product.</returns>
        public async Task<ClassHasChanged<Product>> CheckIfAttributesHaveChanged(Product originalProduct, Item sourceItem)
        {
            ClassHasChanged<Product> _classHasChanged = new ClassHasChanged<Product>
            {
                HasChanged = false,
                Entity = originalProduct
            };

            if (sourceItem.ItemAttributes != null)
            {
                foreach (var _itemCat in sourceItem.ItemAttributes)
                {
                    if (!originalProduct.attributes.Exists(cat => cat.name == _itemCat.ItemAttributeDetail.AttributeName))
                    {
                        // the name does not exist so it we must add it
                        _classHasChanged.Entity = await AddItemAttributesToWooProduct(_itemCat, _classHasChanged.Entity);
                        _classHasChanged.HasChanged = true;
                    }
                    else
                    {
                        ///--> may be a little too much work here, perhaps we should just check if they are the same if not set it to the new options

                        ///---> now we have to search for the terms
                        /// if the original product (_classHasChanged.Entity) check if there are any terms that have been added, by seeing if they are in the sourceItem
                        var _currentProductAttibutes = _classHasChanged.Entity.attributes.Find(cat => cat.name == _itemCat.ItemAttributeDetail.AttributeName);
                        var _AttributeOptions = _itemCat.ItemAttributeVarieties.Select(iav => iav.ItemAttributeVarietyDetail.VarietyName).ToList();
                        var _newAttributionOption = _AttributeOptions.Except(_currentProductAttibutes.options);  // should return a list of items not in the list.

                        if (_newAttributionOption != null)
                        {
                            // not null so means we found a term that does not exists
                            _currentProductAttibutes.options.AddRange(_newAttributionOption);
                            _classHasChanged.HasChanged = true;
                        }
                        //// now we have to check if we need to delete any. So we scan all the existing attributes to sss
                        var _removedAttributionOption = _currentProductAttibutes.options.Except(_AttributeOptions);  // should return a list of items not in the list.
                        if (_removedAttributionOption != null)
                        {
                            // not null so means we found a term that does not exists
                            foreach (var _option in _removedAttributionOption)
                            {
                                _currentProductAttibutes.options.Remove(_option);
                            }
                            _classHasChanged.HasChanged = true;
                        }
                    }
                }

                foreach (var _prodCat in originalProduct.attributes)
                {
                    if (!sourceItem.ItemAttributes.Exists(ic => ic.ItemAttributeDetail.AttributeName == _prodCat.name))
                    {
                        // the name does not exist so it we must add it
                        _classHasChanged.Entity.attributes.Remove(_prodCat);
                        //_classHasChanged.Entity = await DeleteItemAttributesToWooProduct(sourceItem, _classHasChanged.Entity);
                        _classHasChanged.HasChanged = true;
                    }
                }
            }
            return _classHasChanged;
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

            return await _wooAttributeMapRepository.FindFirstByAsync(wcm => wcm.ItemId == mapWooEntityID);
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
            _WooLinkedGridSettings.TotalItems = _dataGridItems.TotalRecordCount;
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
            if (await _ItemRepository.FindFirstByAsync(ial => ial.ItemName == newVeiwEntity.ItemName) == null)
            {
                Item _NewItem = GetItemFromView(newVeiwEntity); // store this here since when it is added it will automatically update the id field
                var _recsAdded = await _ItemRepository.AddAsync(_NewItem);
                if (_recsAdded != null)
                {
                    _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.ItemName} - added", "Attribute Added");
                    if (newVeiwEntity.CanUpdateECommerceMap ?? false)
                    {
                        // they selected to update woo so add to Woo
                        if (await AddWooItemAndMapAsync(_NewItem) == null)   // add if they select to update
                            _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding {newVeiwEntity.ItemName} to Woo - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Woo Attribute");
                        else
                            _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newVeiwEntity.ItemName} - added to Woo", "Woo Attribute Added");
                    }
                }
                else
                    _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.ItemName} - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Attribute");
            }
            else
                _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newVeiwEntity.ItemName} already exists, so could not be added.");
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
            WooProductMap _wooProductMap = await _wooProductMapRepo.FindFirstByAsync(wam => wam.ItemId == sourceWooEntityId);
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
                _WooLinkedGridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {deleteWooEntityId} was not found to have a Woo Attribute Map.");
            else
            {
                if (deleteFromWoo)
                {
                    _result = await DeleteWooProductAsync(_wooProductMap); ///Delete the Attribute in Woo
                    if (_result == AppUnitOfWork.CONST_WASERROR)
                        _WooLinkedGridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {_wooProductMap.WooProductId} was not deleted from Woo categories. Error {_AppUnitOfWork.GetErrorMessage()}");
                    else
                        _WooLinkedGridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Id {_wooProductMap.WooProductId} was deleted from Woo categories.");
                }
                _result = await DeleteWooProductMappingAsync(_wooProductMap);   //Delete our link data, if there was an error should we?
                if (_result == AppUnitOfWork.CONST_WASERROR)
                    _WooLinkedGridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Attribute Id {_wooProductMap.WooProductId} was not deleted from Woo linked categories. Error {_AppUnitOfWork.GetErrorMessage()}");
                else
                    _WooLinkedGridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Woo Product Attribute Id {_wooProductMap.WooProductId} was deleted from Woo linked categories.");
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
        public override async Task<WooProductMap> AddWooItemAndMapAsync(Item addEntity)
        {
            // check it the item exists in woo !!!!!!(we did not do this as there is no such call;, if so get is id and return, otherwise add it and get its id

            //Product _wooProduct = await GetWooProductByName(addEntity.ItemName);
            //if (_wooProduct == null)
            //{
            Product _wooProduct = await AddItemToWooOnlySync(addEntity);
            if (_wooProduct == null)
                return null;
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

            var _recsDelete = await _itemRepository.DeleteByPrimaryIdAsync(deleteViewEntity.ItemId);

            if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Item: {deleteViewEntity.ItemName} is no longer found, was it deleted?");
            else
                _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {deleteViewEntity.ItemName} was it deleted?");
        }
        /// <summary>
        /// Update whether the woo Product map for the view item can be updated, dependant on the current setting
        /// Logic: Retrieve item from database. If it exists and the status has changed (i.e we can update, but we now we cannot and vice versa) then update the can map status. If it did not exists add the mapping.
        /// </summary>
        /// <param name="updatedViewEntity">Item view we want to update and link to woo if required.</param>
        /// <returns>number of records updated, or error</returns>
        public override async Task<int> UpdateWooMappingAsync(ItemView updatedViewEntity)
        {
            int _recsUpdated = 0;

            WooProductMap updateWooProductMap = await GetWooMappedItemAsync(updatedViewEntity.ItemId);
            if (updateWooProductMap != null)
            {
                // this should only be called if their is a woo mapping and all is enabled
                //if (updateWooProductMap.CanUpdate == updatedViewEntity.CanUpdateECommerceMap)
                //{
                //    // not necessary to display message.
                //    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo Attribute Map for Attribute: {updatedViewEntity.ItemName} has not changed, so was not updated?");
                //}
                //else
                //{

                if (updateWooProductMap.CanUpdate != (bool)updatedViewEntity.CanUpdateECommerceMap)
                {
                    updateWooProductMap.CanUpdate = (bool)updatedViewEntity.CanUpdateECommerceMap;
                    IAppRepository<WooProductMap> WooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();
                    _recsUpdated = await WooProductMapRepository.UpdateAsync(updateWooProductMap);
                    _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {updatedViewEntity.ItemName} was updated.");
                }
            }
            else
                _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Product Map for Item: {updatedViewEntity.ItemName} is no longer found, was it deleted?");
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
        //            _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {updatedViewEntity.ItemName} was updated.");
        //        }
        //    }
        //    else
        //    {
        //        // nothing was found, so this probably means they now want to add. We should had a Pop up to check
        //        int _result = await AddWooItemAndMapAsync(GetItemFromView(updatedViewEntity));
        //        if (_result != AppUnitOfWork.CONST_WASERROR)
        //            _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item {updatedViewEntity.ItemName} has been added to woo?");
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
            IWooProduct _wooProductRepository = await GetIWooProductAsync();
            if (_wooProductRepository != null)                     //  - > if it does not exist then what?
            {
                WooProductMap _updateWooMapEntity = await GetWooProductMapFromIDAsync(updateViewEntity.ItemId);
                if (_updateWooMapEntity == null)
                {
                    // need to add the Attribute -> this is done later.
                    _result = (await AddWooItemAndMapAsync(updateViewEntity)) != null ? 1 : AppUnitOfWork.CONST_WASERROR;
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
                        _Mapper.Map(updateViewEntity, _wooProduct); //
                        ClassHasChanged<Product> _productClassHasChanged = await CheckIfAttributesHaveChanged(_wooProduct, GetItemFromView(updateViewEntity));
                        bool _HasChanged = _productClassHasChanged.HasChanged;  // store this since we may over write this
                        _productClassHasChanged = await CheckIfAttributesHaveChanged(_productClassHasChanged.Entity, GetItemFromView(updateViewEntity));
                        _HasChanged = (_HasChanged || _productClassHasChanged.HasChanged);
                        // If attribute or category has changed we need to save
                        // Now need to check if product has change
                        // AddItemToWooItemMapAsyn
    ///-----> busy here
    ///
    /// -> think I finished 
                        if (_HasChanged) //|| (!_wooProduct.order_by.Equals(updateViewEntity.OrderBy.ToString(), StringComparison.OrdinalIgnoreCase)))
                        {
                            // _wooProduct.name = updateEntity.ItemName;  // only update if necessary
                            //_wooProduct.order_by = ConvertToWooOrderBy(updateViewEntity.OrderBy);
                            var _res = ((await _wooProductRepository.UpdateProductAsync(_wooProduct)));
                            _result = ((_res == null) || (_res.id == null)) ? AppUnitOfWork.CONST_WASERROR : (int)_res.id; // if null there is an issue
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
        public override async Task<int> UpdateWooItemAndMappingAsync(ItemView updateItemView)
        {
            int _result = await UpdateWooItemAsync(updateItemView);
            if (_result > 0)
                _WooLinkedGridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Updated woo Attribute {updateItemView.ItemName}.");
            else if (_result == AppUnitOfWork.CONST_WASERROR)
                _WooLinkedGridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute {updateItemView.ItemName} update failed.");

            _result = await UpdateWooMappingAsync(updateItemView);
            return _result;
        }
        /// <summary>
        /// Update the Item using the View Item data we have from the system
        /// Logic: Retrieve the item by Id; if it does not exist, show error, otherwise map data across and update in the database.
        /// </summary>
        /// <param name="updateItem">Item view we want to update.</param>
        /// <returns>number of records updated, or zero means item no longer there. OR return Error if there was one.</returns>
        public override async Task<int> UpdateItemAsync(ItemView updateItem)
        {
            int _recsUpdted = 0;
            IAppRepository<Item> _ItemRepository = _AppUnitOfWork.Repository<Item>();
            // first check it exists - it could have been deleted 
            Item _CurrentItem = await _ItemRepository.GetByIdAsync(updateItem.ItemId);
            if (_CurrentItem == null)
            {
                _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Item: {updateItem.ItemName} is no longer found, was it deleted?");
                return AppUnitOfWork.CONST_WASERROR;
            }
            else
            {
                //                _Mapper.Map(updateViewItem, _CurrentItem);
                //_CurrentItem.ItemName = updateViewItem.ItemName;
                //_CurrentItem.SKU = updateViewItem.SKU;
                //_CurrentItem.IsEnabled = updateViewItem.IsEnabled;
                //_CurrentItem.ItemDetail = updateViewItem.ItemDetail;
                _CurrentItem.PrimaryItemCategoryLookupId = ((updateItem.PrimaryItemCategoryLookupId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)updateItem.PrimaryItemCategoryLookupId;
                _CurrentItem.ReplacementItemId = ((updateItem.ReplacementItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)updateItem.ReplacementItemId;
                //_CurrentItem.ItemAbbreviatedName = updateViewItem.ItemAbbreviatedName;
                //_CurrentItem.SortOrder = updateViewItem.SortOrder;
                //_CurrentItem.BasePrice = updateViewItem.BasePrice;
                //_CurrentItem.ParentItemId = ((updateViewItem.ParentItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)updateViewItem.ParentItemId;
                ////????? Lists and other lazy loads

                _recsUpdted = await _ItemRepository.UpdateAsync(_CurrentItem);
                if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
                    _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateItem.ItemName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating Attribute");
                // if Woo Is Active and this item is mapped
                if ((_WooLinkedGridSettings.WooIsActive) && (updateItem.HasECommerceAttributeMap))
                {
                    if (await UpdateWooItemAndMappingAsync(updateItem) == AppUnitOfWork.CONST_WASERROR)
                        _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updateItem.ItemName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating Attribute Map");   // should we send a message here error = mapping not updated 
                }

             /*   need to go and update all the updates to only use entity and not view, check for integrations(Woo)
                    should be done here.
             */

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
            //    _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Name: {updateVeiwEntity.ItemName} - already exists, cannot be updated", "Exists already");
            //else
            //{
            if (IsValid(updateVeiwEntity))
            {
                // update and check for errors 
                if (await UpdateItemAsync(updateVeiwEntity) == AppUnitOfWork.CONST_WASERROR)
                {
                    _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating Attribute: {updateVeiwEntity.ItemName} -  {_AppUnitOfWork.GetErrorMessage()}", "Updating Attribute Error");
                }
                else
                    _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Item: {updateVeiwEntity.ItemName} was updated.");
            }
            else
            {
                _WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Item {updateVeiwEntity.ItemName} cannot be parent and child.", "Error updating");
            }
            //}
            //-> done in parent await LoadAllViewItemsAsync();   // reload the list so the latest item is displayed
        }
        #endregion
    }
}
