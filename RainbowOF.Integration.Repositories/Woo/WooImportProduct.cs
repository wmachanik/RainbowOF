using AutoMapper;
using Microsoft.AspNetCore.Components;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Items;
using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Integration.Repositories.Woo
{
    /// <summary>
    /// Import Products from a Woo Product into Items, adding or updating the variant.
    /// </summary>
    public partial class WooImportProduct : IWooImportWithParents<Item, Product, WooProductMap>
    {
        [Inject]
        public IMapper autoMapper { get; set; }
        #region Variables
        public List<WooItemWithParent> EntityWithParents { get; set; } = new();
        public IUnitOfWork appUnitOfWork { get; set; }
        public ILoggerManager appLoggerManager { get; set; }
        public WooSettings appWooSettings { get; set; }
        public ImportCounters CurrImportCounters { get; set; } = new();
        #endregion
        #region Private / Class Variables
        //private StringTools _StringTools = new StringTools();
        private ProductToItemMapper productToItemMapper = null;
        #endregion
        #region Constructor
        public WooImportProduct(IUnitOfWork appUnitOfWork, ILoggerManager logger, WooSettings appWooSettings, IMapper mapper)
        {
            this.appUnitOfWork = appUnitOfWork;
            appLoggerManager = logger;
            this.appWooSettings = appWooSettings;
            autoMapper = mapper;
            if (appLoggerManager.IsDebugEnabled()) if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("WooImportProduct initialised.");
        }

        public WooImportProduct()
        {
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// Using the current Item, assign any product categories to it, using our mapped data.
        /// </summary>
        /// <param name="currItem">Current Item we are working with, or we need to assign the categories too</param>
        /// <param name="currWooProd">Current Woo Product information</param>
        /// <returns>the modified Item with the Categories added, if any</returns>
        private async Task<Item> AssignWooProductCategoryAsync(Item currItem, Product currWooProd)
        {
            bool _isCatUpdate = currItem.ItemCategories != null;
            if (currItem != null)
            {
                WooImportProductCategory _wooImportProductCategories = new WooImportProductCategory(appUnitOfWork, appLoggerManager, appWooSettings);

                // if we get here then we have an item and it has an id
                IRepository<ItemCategoryLookup> _itemCategoryLookupRepo = appUnitOfWork.Repository<ItemCategoryLookup>();
                ItemCategoryLookup _itemCategoryLookup = null;
                bool _SetPrimary = true;
                foreach (var cat in currWooProd.categories)
                {
                    Guid CategoryId = await _wooImportProductCategories.GetWooMappedEntityIdByIdAsync((uint)cat.id);
                    _itemCategoryLookup = await _itemCategoryLookupRepo.GetByIdAsync(ic => ic.ItemCategoryLookupId == CategoryId);
                    if (_itemCategoryLookup != null)
                    {
                        ///////// check if exists if this is not a new item
                        if ((_isCatUpdate) && (currItem.ItemCategories.Exists(ic => ic.ItemCategoryLookupId == _itemCategoryLookup.ItemCategoryLookupId)))
                        {
                            // this attribute exists so just update it - nothing to do here at the moment.
                        }
                        else
                        {
                            // add an item to the linked list. Since it is a new item the FK must be guid.empty for EF Core to work
                            if (currItem.ItemCategories == null)
                                currItem.ItemCategories = new List<ItemCategory>();
                            currItem.ItemCategories.Add(new ItemCategory
                            {
                                ItemCategoryLookupId = _itemCategoryLookup.ItemCategoryLookupId
                            });
                            if (_SetPrimary)    // assume the first in the list is the primary, as we have no other way of knowing.
                            {
                                currItem.PrimaryItemCategoryLookupId = _itemCategoryLookup.ItemCategoryLookupId;
                                _SetPrimary = false;
                            }
                        }
                    }
                }
            }
            return currItem;   // return the currItem after we have manipulated it.
        }
        /// <summary>
        /// Add or update variation of this item
        /// </summary>
        /// <param name="currItem">Current Item we are working with, or we need to assign the Attributes too</param>
        /// <param name="prodAttrib">the Woo Product Attributes</param>
        /// <returns>Item modified with new data</returns>
        private async Task<Item> AddOrUpdateVarietiesAsync(Item currItem, ProductAttributeLine prodAttrib, WooProductAttributeMap currWooProductAttributeMap) //, bool IsAttributeVarietyUpddate)
        {
            IRepository<ItemAttributeVarietyLookup> _itemAttribVarietyLookupRepo = appUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            ItemAttributeVarietyLookup _itemAttributeVarietyLookup = null;
            foreach (var attribTerm in prodAttrib.options)
            {
                if (attribTerm.StartsWith("Aero"))
                    currItem.IsEnabled = true;

                /////----> attribute add / update this was wrong
                ///
                // find any term that has the same ParentId and the same term. changed 10 Dec 2021 to include the "parent" ItemAttributeLookupId
                _itemAttributeVarietyLookup = await _itemAttribVarietyLookupRepo.GetByIdAsync(iavl => ((iavl.ItemAttributeLookupId == currWooProductAttributeMap.ItemAttributeLookupId) && (iavl.VarietyName == attribTerm)));
                if (_itemAttributeVarietyLookup != null)
                {
                    // found so update or add
                    ItemAttribute _itemAttribute = currItem.ItemAttributes.Find(ic => ic.ItemAttributeLookupId == currWooProductAttributeMap.ItemAttributeLookupId);
                    if (_itemAttribute == null)
                        return currItem; // this should never occur  

                    // does this attribute have this variety, if so update otherwise add
                    if ((_itemAttribute.ItemAttributeVarieties != null) && (_itemAttribute.ItemAttributeVarieties.Exists(iav => iav.ItemAttributeVarietyLookupId == _itemAttributeVarietyLookup.ItemAttributeVarietyLookupId)))
                    {
                        // this attribute variety / term exists so just update it. Do stuff here if we need - so far nada
                        //make sure this matches
                        ItemAttributeVariety _itemAttributeVariety = _itemAttribute.ItemAttributeVarieties.FirstOrDefault(iav => iav.ItemAttributeVarietyLookupId == _itemAttributeVarietyLookup.ItemAttributeVarietyLookupId);
                        ///-> can this be null? It should never be unless deleted
                        if (_itemAttributeVariety != null)
                        {
                            // copy the whole item across just in case there have been changes
                            _itemAttributeVariety.ItemAttributeVarietyDetail = _itemAttributeVarietyLookup;
                            _itemAttributeVariety.ItemAttributeId = _itemAttribute.ItemAttributeId;
                        }
                    }
                    else
                    {
                        // we have a attribute variety, this means we should have an attribute that, that it belongs too.
                        if (_itemAttribute.ItemAttributeVarieties == null)
                            _itemAttribute.ItemAttributeVarieties = new List<ItemAttributeVariety>();
                        // create a new variety assume 1.0 as in 1-1 QTY and update the ItemDetails. Do not change Item Id as then EF core knows it is a new record.
                        ItemAttributeVariety _itemAttributeVariety = new ItemAttributeVariety();
                        autoMapper.Map(_itemAttributeVarietyLookup, _itemAttributeVariety);

                        // need to allocate the attribute id
                        _itemAttributeVariety.ItemAttributeId = _itemAttribute.ItemAttributeId;
                        _itemAttribute.ItemAttributeVarieties.Add(_itemAttributeVariety);  // add the new variant
                        //_itemAttribute.ItemAttributeVarieties.Add(new ItemAttributeVariety
                        //{
                        //    ItemAttributeVarietyLookupId = _itemAttributeVarietyLookup.ItemAttributeVarietyLookupId,
                        //    ItemAttributeVarietyDetail = _itemAttributeVarietyLookup,    // copy the whole attribute across
                        //    ItemAttributeId = _itemAttribute.ItemAttributeId,
                        //    UoMId = _itemAttributeVarietyLookup.UoMId,
                        //    UoM = _itemAttributeVarietyLookup.UoM,
                        //    UoMQtyPerItem = 1.0
                        //});
                    }
                    //currItem.ItemAttributes.Add(_itemAttribute);
                }
                //-> should we not add it here?
            }
            return currItem;
        }
        /// <summary>
        /// Using the Item we are working with check if an attribute is assigned, then add if not or update.
        /// </summary>
        /// <param name="currItem">The current Item we are working on</param>
        /// <param name="prodAttrib">The Product attributes to be assigned</param>
        /// <param name="HasAttributes">Does it have attributes?</param>
        /// <returns>updated Item</returns>
        private async Task<Item> AddOrUpdateItemsAttributesAsync(Item currItem, ProductAttributeLine prodAttrib, bool HasAttributes)
        {
            // if we get here then we have an item and it has an id
            //            IAppRepository<ItemAttributeLookup> _itemAttributeLookupRepo = appUnitOfWork.Repository<ItemAttributeLookup>();
            IRepository<WooProductAttributeMap> _wooProductAttributeMapRepo = appUnitOfWork.Repository<WooProductAttributeMap>();
            // check if this is an update or an add for categories and attributes
            WooProductAttributeMap _wooProductAttributeMap = await _wooProductAttributeMapRepo.GetByIdAsync(wpa => wpa.WooProductAttributeId == prodAttrib.id);
            if (_wooProductAttributeMap != null)
            {
                if ((HasAttributes) && (currItem.ItemAttributes.Exists(ic => ic.ItemAttributeLookupId == _wooProductAttributeMap.ItemAttributeLookupId)))
                {
                    // this attribute exists so just update it
                    ItemAttribute _itemAttribute = currItem.ItemAttributes.Find(ic => ic.ItemAttributeLookupId == _wooProductAttributeMap.ItemAttributeLookupId);
                    _itemAttribute.IsUsedForItemVariety = prodAttrib.variation == null ? false : (bool)prodAttrib.variation;  // the only thing that we can really update
                }
                else
                {
                    if (currItem.ItemAttributes == null)
                        currItem.ItemAttributes = new List<ItemAttribute>();
                    // create a new attribute and update the ItemDetails. Do not change Item Id as then EF core knows it is a new record.
                    currItem.ItemAttributes.Add(new ItemAttribute
                    {
                        IsUsedForItemVariety = prodAttrib.variation == null ? false : (bool)prodAttrib.variation,
                        ItemAttributeLookupId = _wooProductAttributeMap.ItemAttributeLookupId,
                    });
                }
                //if (Convert.ToBoolean(prodAttrib.variation))
                //{   --> not sure why we had this but we were only adding terms / varieties if they were used for variations, but this means attributes are left hanging.
                currItem = await AddOrUpdateVarietiesAsync(currItem, prodAttrib, _wooProductAttributeMap); //--> need to check each attribute, IsAttributeVarietyUpddate);
                //}
            }
            return currItem;
        }
        /// <summary>
        /// Using the current Item, assign any product attributes to it, using our mapped data.
        /// </summary>
        /// <param name="currItem">Current Item we are working with, or we need to assign the attributes too</param>
        /// <param name="currWooProd">Current Woo Product information</param>
        /// <returns>the modified Item with the attributes added, if any</returns>
        private async Task<Item> AssignWooProductAttributesAsync(Item currItem, Product currWooProd)
        {
            if (currItem != null)
            {
                bool _isAttribUpdate = currItem.ItemAttributes != null;
                // loop through all the attributes and see if the are used for variations and add all the terms a variations
                foreach (var attrib in currWooProd.attributes)
                {
                    currItem = await AddOrUpdateItemsAttributesAsync(currItem, attrib, _isAttribUpdate);
                }
            }
            return currItem;   // return the currItem after we have manipulated it.
        }
        /// <summary>
        /// Add Woo Product to Item table as the Id in the mapping was not found.
        /// </summary>
        /// <param name="currWooProd">current Woo Product we are importing</param>
        /// <returns></returns>
        private async Task<Guid> AddProductToItemsAsync(Product currWooProd)
        {
            Guid _itemId = Guid.Empty;
            IRepository<WooProductMap> _wooProductMapRepository = appUnitOfWork.Repository<WooProductMap>();
            //it may not be in the map, but we may have a product with that name so see if we do -Add Item Product if it does not exist
            _itemId = await AddOrGetEntityIDAsync(currWooProd);
            if (_itemId != Guid.Empty)
            {
                WooProductMap _wooProductMap = new WooProductMap
                {
                    WooProductId = (int)currWooProd.id,
                    ItemId = _itemId,
                    CanUpdate = true
                };
                if (await _wooProductMapRepository.AddAsync(_wooProductMap) == null)
                {
                    // did not add so set _ItemProductId to ItemProductID to Guid.Empty = error
                    _itemId = Guid.Empty;
                }
                CurrImportCounters.TotalAdded++;
            }
            return _itemId;
        }
        /// <summary>
        /// Delete a Map from the item map table.
        /// </summary>
        /// <param name="sourceItemId">The Id of source map to delete</param>
        /// <returns>1 if deleted or Error</returns>
        private async Task<int> DeleteWooProductMapByItemIdAsync(Guid sourceItemId)
        {
            IRepository<WooProductMap> _wooProductMapRepository = appUnitOfWork.Repository<WooProductMap>();
            return await _wooProductMapRepository.DeleteByAsync(wpm => wpm.ItemId == sourceItemId);
        }
        /// <summary>
        /// Import the actual Variations of a variable product into the items table and set currItem.Id as parent
        /// </summary>
        /// <param name="sourceWooEntity"></param>
        /// <param name="sourceEntity"></param>
        /// <returns>number of variants added or error</returns>
        private async Task<ImportCounters> ImportProductVariationsAsync(Product sourceWooEntity, Item sourceEntity)
        {
            WooImportVariation _wooImportVariation = new WooImportVariation(appUnitOfWork,appLoggerManager,appWooSettings, autoMapper);
            return await _wooImportVariation.ImportProductVariantsAsync((uint)sourceWooEntity.id, sourceEntity.ItemId);
        }
        #endregion
        #region Interface Methods
        /// <summary>
        /// Retrieve all products from Woo that are either in stock or not.
        /// </summary>
        /// <param name="OnlyItemsInStock">default only items in stock</param>
        /// <returns>List of Products</returns>
        public async Task<List<Product>> GetWooEntityDataAsync(bool OnlyItemsInStock = true)
        {
            WooAPISettings _wooAPISettings = new WooAPISettings(appWooSettings);
            WooProduct _wooProduct = new WooProduct(_wooAPISettings, appLoggerManager);
            List<Product> wooProducts = OnlyItemsInStock ? await _wooProduct.GetAllProductsInStockAsync()
                                                         : await _wooProduct.GetAllProductsAsync();

            return wooProducts;
            ///---> may need to rewrite this so that it returns 20 and loops
        }
        /// <summary>
        /// Get the Item's Id from the mapping table if it exists. If not return Guid.Empty
        /// </summary>
        /// <param name="sourceWooEntityId"></param>
        /// <returns></returns>
        public async Task<Guid> GetWooMappedEntityIdByIdAsync(uint sourceWooEntityId)
        {
            IRepository<WooProductMap> _wooProductMapRepo = appUnitOfWork.Repository<WooProductMap>();
            WooProductMap _wooProductMap = await _wooProductMapRepo.GetByIdAsync(wp => wp.WooProductId == sourceWooEntityId);
            return (_wooProductMap == null) ? Guid.Empty : _wooProductMap.ItemId;
        }
        /// <summary>
        /// Using the Woo Product as a source add Item to our tables 
        ///  Past Name: async Task<Guid> AddItem(Product currWooProd, List<WooItemWithParent> currWooProductsWithParents)
        /// </summary>
        /// <param name="newWooEntity">a new Woo Product we do not have</param>
        /// <returns>Id of item added, or Guid.Empty if not</returns>
        public async Task<Guid> AddEntityAsync(Product newWooEntity)
        {
            // using the prod we need to add the item and link all its Settings: AssginedCategory, Attributes and Attribute varieties
            if (productToItemMapper == null)
                productToItemMapper = new(autoMapper);
            Item _newItem = productToItemMapper.MapWooProductInfo(newWooEntity, null); //, ref -> uses the class level values currWooProductsWithParents);
            if (_newItem != null)
            {
                /// once we get here we have a new ID for the ItemID so we can continue
                _newItem = await AssignWooProductCategoryAsync(_newItem, newWooEntity);    // adding the item but not the linked items
                _newItem = await AssignWooProductAttributesAsync(_newItem, newWooEntity);
                if (await appUnitOfWork.itemRepository.AddItemAsync(_newItem) == null)   
                {
                    _newItem.ItemId = Guid.Empty;    // if no records added or there was an error.
                }
                else if (_newItem.ItemType.Equals(ItemTypes.Variable))
                {
                    // once item added we can add variants
                    /* _newItem = not using the number of variants here*/ 
                    await ImportProductVariationsAsync(newWooEntity, _newItem);
                }
                return _newItem.ItemId;
            }
            else
                return Guid.Empty;
        }
        /// <summary>
        /// Check if the Item of the same name or SKU exists in the Item database, if so update and return, otherwise add and return it
        ///    Was called:  async Task<Guid> AddOrGetIDItem(Product currWooProduct, List<WooItemWithParent> currWooProductsWithParents)
        /// </summary>
        /// <param name="sourceEntity">The source Woo Commerce product wee are importing</param>
        /// <returns>The Id of the item found or added</returns>
        public async Task<Guid> AddOrGetEntityIDAsync(Product sourceEntity)
        {
            Guid _itemId = Guid.Empty;
            // check if the Prod exists based on name since there is no mapped id
            Item _item = await appUnitOfWork.itemRepository.FindFirstEagerLoadingItemAsync(i => i.ItemName == sourceEntity.name);
            if (appUnitOfWork.IsInErrorState())
                return Guid.Empty;
            if ((_item == null) && (!String.IsNullOrEmpty(sourceEntity.sku)))     // check if perhaps they renamed the by looking for SKU
            {
                _item = await appUnitOfWork.itemRepository.FindFirstEagerLoadingItemAsync(i => i.SKU == sourceEntity.sku);
                if (appUnitOfWork.IsInErrorState())
                    return Guid.Empty;
            }
            _itemId = (_item != null) 
                ? await UpdateEntityAsync(sourceEntity, _item)
                : await AddEntityAsync(sourceEntity);

            return _itemId;
        }
        /// <summary>
        /// See if an Item of this Id exists in the item table. If so, update it otherwise added it, returning the Id
        /// </summary>
        /// <param name="sourceEntity">The Source Woo Product</param>
        /// <param name="sourceWooMappedEntityId">The Id we found mapped or added</param>
        /// <returns>Id of the Item Added or Updated</returns>
        public async Task<Guid> AddOrUpdateEntityAsync(Product sourceEntity, Guid sourceWooMappedEntityId)
        {
            Guid _itemId = Guid.Empty;
            // check if the Prod based on mapped id
            Item _item = await appUnitOfWork.itemRepository.FindFirstEagerLoadingItemAsync(i => i.ItemId == sourceWooMappedEntityId);
            /// Now update the woo Product using the _ItemProductId returned.
            if (_item != null)
            {    // we found the Item Id so update that Id
                _itemId = await UpdateEntityAsync(sourceEntity, _item);
                CurrImportCounters.TotalUpdated++;
            }
            else
            {
                // if we got here then the product map is pointing to the wrong ID, so delete the ID and add the item
                if (!appUnitOfWork.IsInErrorState())  // did something go wrong rather?
                    await DeleteWooProductMapByItemIdAsync(sourceWooMappedEntityId);
                ////-> we need to add this item since the id we have is not corresponding.
                _itemId = await AddProductToItemsAsync(sourceEntity);
            }
            return _itemId;
        }
        /// <summary>
        /// For the WooItem passed in get the Item's Child and Parent and set their Ids
        /// Logic using the ids passed look for the linked attribute to the id then look for the ParentId  get the Guids of each and update the database
        /// Get ProdWithAParent.ID GUID from ItemAttribute Table = ParentID
        /// Get ProdWithAParent.ParentID GUID from ItemAttribute Table = ChildID
        /// Set the  ItemAttribute.ParentID = ParentID for ItemsAttrib.ID = ChildID
        /// </summary>
        /// <param name="sourceWooEntityWithParent">the woo item with a parent</param>
        /// <returns></returns>
        public async Task<bool> FindAndSetParentEntityAsync(WooItemWithParent sourceWooEntityWithParent)
        {
            Guid _itemId = await GetWooMappedEntityIdByIdAsync(sourceWooEntityWithParent.ChildId);
            Guid _parentAttribId = await GetWooMappedEntityIdByIdAsync(sourceWooEntityWithParent.ParentId);

            bool _isDone = await SetWooEntityParentAsync(_itemId, _parentAttribId);

            return _isDone;
        }
        /// <summary>
        /// Using the Id of the Parent for the child item set the parent item.
        /// </summary>
        /// <param name="sourceChildWooEntityId">source child ItemId</param>
        /// <param name="sourceParentWooEntityId">source Parent Id Item</param>
        /// <returns>success or failure</returns>
        public async Task<bool> SetWooEntityParentAsync(Guid sourceChildWooEntityId, Guid sourceParentWooEntityId)
        {
            // using the GUIDs of the category id update the parent of that record
            bool _IsUpdated = false;
            if ((sourceChildWooEntityId != Guid.Empty) && (sourceParentWooEntityId != Guid.Empty))
            {
                Item _item = await appUnitOfWork.itemRepository.GetByIdAsync(ic => ic.ItemId == sourceChildWooEntityId);
                if (_item == null)
                    _IsUpdated = false;
                else
                {     // found so update
                    _IsUpdated = (await appUnitOfWork.itemRepository.UpdateAsync(_item)) != UnitOfWork.CONST_WASERROR;
                }
            }
            return _IsUpdated;
        }
        /// <summary>
        ///  Update an item that has been found in the WooPoduct link
        /// </summary>
        /// <param name="updatedWooEntity">Woo Entity that is updated</param>
        /// <param name="updatedEntity">Our Item to be updated</param>
        /// <returns></returns>
        public async Task<Guid> UpdateEntityAsync(Product updatedWooEntity, Item updateEntity)
        {
            // using the prod we need to add the item and link all its Settings: AssginedCategory, Attributes and Attribute varieties
            if (productToItemMapper == null)
                productToItemMapper = new(autoMapper);
            updateEntity = productToItemMapper.MapWooProductInfo(updatedWooEntity, updateEntity); //, ref currWooProductsWithParents);
            if (updateEntity != null)
            {
                //int _RecsDone;  //used for error checking
                /// once we get here we have a new ID for the ItemID so we can continue
                updateEntity = await AssignWooProductCategoryAsync(updateEntity, updatedWooEntity);
                updateEntity = await AssignWooProductAttributesAsync(updateEntity, updatedWooEntity);
                if (updateEntity.ItemType == ItemTypes.Variable)
                    await ImportProductVariationsAsync(updatedWooEntity, updateEntity);  // we don't handle errors here, probably should do something, but no way of knowing where the error is
                // now update the item
                if (await appUnitOfWork.itemRepository.UpdateAsync(updateEntity) == 0)
                {
                    updateEntity.ItemId = Guid.Empty;
                }
            }
            return updateEntity.ItemId;
        }
        /// <summary>
        /// Update the anything in the Mapping we need to update, Item with the data we are importing. If the mapping is pointing to the wrong place delete it
        /// </summary>
        /// <param name="updatedWooEntity">Updated Woo Product</param>
        /// <param name="targetWooMap">The mapping we are targeting</param>
        /// <returns>Id of item updated</returns>
        public async Task<Guid> UpdateWooMappingEntityAsync(Product updatedWooEntity, WooProductMap targetWooMap)
        {
            Guid _itemId = await AddOrUpdateEntityAsync(updatedWooEntity, targetWooMap.ItemId);
            if (_itemId != Guid.Empty)
            {
                // update the map if there are changes -> at the moment there are none since this is a pure Id map.
                // If we add other date we should update
            }
            return _itemId;
        }
            //////----> same as AddOrUpdateEntity so used there.

            //IItemRepository _itemRepository = appUnitOfWork.itemRepository();
            //// check if the Prod based on mapped id
            //Item _item = await _itemRepository.FindFirstEagerLoadingItemAsync(i => i.ItemId == currWooProductMap.ItemId);
            ///// Now update the woo Product using the _ItemProductId returned.
            //if (_item != null)
            //{    // we found the Item Id so update that Id
            //    _itemId = await UpdateItem(currWooProduct, _item, currWooProductsWithParents);
            //    currImportCounters.TotalUpdated++;
            //}
            //else
            //{
            //    // if we got here then the product map is pointing to the wrong ID, so delete the ID and add the item
            //    await DeleteWooProductMap(currWooProductMap);
            //    _itemId = await AddProductToItems(currWooProduct, currWooProductsWithParents);
            //}
            //return _itemId;
            //throw new NotImplementedException();
            /// in categories we call add or get Entity, do we here? 
        
        /// <summary>
        /// For each product
        ///     o If the product does not exists add and copy woo details to new record increase Counter.added
        ///     o If product does exist and CanUpdate is true then update by copying the WooDetaits that are not keys, increase Counter.update
        /// </summary>
        /// <param name="sourceEntity"></param>
        /// <returns></returns>
        public async Task<Guid> ImportAndMapWooEntityDataAsync(Product sourceEntity)
        {
            Guid _itemProductId = Guid.Empty;
            // Get repository for each database we are accessing. ItemProduct. WooProductMap & WooSyncLog
            IRepository<WooProductMap> _wooProductMapRepository = appUnitOfWork.Repository<WooProductMap>();

            // Import the Product and set sync data
            ///first check if it exists in the mapping, just in case there has been a name change
            WooProductMap _wooProductMap = await _wooProductMapRepository.GetByIdAsync(wp => wp.WooProductId == sourceEntity.id);
            if (_wooProductMap != null)   // the id exists so update
            {
                if (_wooProductMap.CanUpdate)    /// only if the product can be updated 
                {
                    _itemProductId = await UpdateWooMappingEntityAsync(sourceEntity, _wooProductMap);
                }
            }
            else      // the id does not exists so add
            {
                _itemProductId = await AddProductToItemsAsync(sourceEntity);
                CurrImportCounters.TotalAdded++;
            }

            return _itemProductId;
        }
        #endregion
    }
}
