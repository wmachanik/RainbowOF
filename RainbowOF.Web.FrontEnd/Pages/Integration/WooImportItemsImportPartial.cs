using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RainbowOF.Repositories.Common;
using RainbowOF.Models.Items;
using RainbowOF.Models.Woo;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using WooCommerceNET.WooCommerce.v3;
using RainbowOF.Models.Lookups;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Tools;
using RainbowOF.Repositories.Items;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImportItemsComponent // for CategoryImport so file name WooImportItemsComponentProductsImport Partial
    {
        /// <summary>
        /// All the attribute term import stuff. Attributes Terms in Woo are Attributed Varieties to us. Could we have generalised this for each item import with an object?
        /// </summary>
        #region AttrbiuteStuff

        // Retrieve data from Woo
        async Task<List<Product>> GetAllWooProducts(bool OnlyItemsInStock)
        {
            WooAPISettings _wooAPISettings = new WooAPISettings(AppWooSettings);

            WooProduct _wooProduct = new WooProduct(_wooAPISettings, _Logger);
            List<Product> wooProducts = OnlyItemsInStock ?
                await _wooProduct.GetAllProductsInStock() :
                await _wooProduct.GetAllProducts();


            // unique check not needed 
            //  var _wooProducts = wooProducts.GroupBy(wp => wp.id).Select(wp => wp.FirstOrDefault()); // wooProducts that are distinct;
            //  wooProducts = _wooProducts.ToList();
            return wooProducts;
        }
        // there is essential on master item and then variations. how those are represented in a order is either each or using UOOm a Qty of each
        // this routine is used both for update and add/create. If null create a new one otherwise update, then basic Product info across
        private Item CopyWooProductInfo(Product sourceWooProd, Item currItem, ref List<WooItemWithParent> currWooProductsWithParents)
        {
            if (currItem == null)
                currItem = new Item();
            currItem.ItemName = _StringTools.Truncate(sourceWooProd.name, 100);  // max length is 100
            currItem.SKU = _StringTools.Truncate(sourceWooProd.sku, 50);  // max length is 50
            currItem.IsEnabled = true;
            currItem.ItemDetail = _StringTools.Truncate(_StringTools.StripHTML(sourceWooProd.short_description), 250);   // max length is 255
            currItem.ItemAbbreviatedName = _StringTools.MakeAbbriviation(sourceWooProd.name);
            currItem.SortOrder = Convert.ToInt32(sourceWooProd.menu_order?? 0);  //  (currWooProd.menu_order == null) ? 50 : (int)currWooProd.menu_order;
            currItem.BasePrice = Convert.ToDecimal(sourceWooProd.price == null ? 0.0 : sourceWooProd.price); // (currWooProd.price == null) ? 0 :(decimal)currWooProd.price;
            currItem.ManageStock = Convert.ToBoolean(sourceWooProd.manage_stock?? false);
            currItem.QtyInStock = Convert.ToInt32(sourceWooProd.stock_quantity?? 0);
            // copy the image data across
            if ((sourceWooProd.images != null) && (sourceWooProd.images.Count > 0))
            {
                if (currItem.ItemImages == null)
                    currItem.ItemImages = new();
                // only add if it does not exist
                bool isPrimaryImage = true;
                foreach (var srcImg in sourceWooProd.images)
                {
                    if (!currItem.ItemImages.Exists(tgtImg => tgtImg.ImageURL == srcImg.src))
                    {
                        // add this url and associated item as it does not exist
                        currItem.ItemImages.Add(new ItemImage
                        {
                            IsPrimary = isPrimaryImage,
                            Name = srcImg.name,
                            Alt = srcImg.alt,
                            ImageURL = srcImg.src,
                            ItemId = currItem.ItemId,     ///?? is this correct?
                        }
                        );
                    }
                    isPrimaryImage = false;  // the first one is assumed to be the primary image
                }
            }

            if ((sourceWooProd.parent_id != null) && (sourceWooProd.parent_id > 0))
                currWooProductsWithParents.Add(new WooItemWithParent
                {
                    ChildId = (int)sourceWooProd.id,
                    ParentId = (int)sourceWooProd.parent_id
                });
            return currItem;
        }
        /// <summary>
        /// Using the item sent assign the categories that the WooProduct has. Remember that we do not affect ItemId since the EF core 
        /// </summary>
        /// <param name="Item">The Product Item t add too</param>
        /// <param name="WooProduct">The Woo Product Source</param>
        /// <returns></returns>
        async Task<Item> AssignWooProductCategory(Item currItem, Product currWooProd)
        {
            bool _isCatUpdate = currItem.ItemCategories != null;
            if (currItem != null)
            {
                // if we get here then we have an item and it has an id
                IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepo = _AppUnitOfWork.Repository<ItemCategoryLookup>();
                ItemCategoryLookup _itemCategoryLookup = null;
                bool _SetPrimary = true;
                foreach (var cat in currWooProd.categories)
                {
                    Guid CategoryId = await GetCategoryById((int)cat.id);
                    _itemCategoryLookup = await _itemCategoryLookupRepo.FindFirstAsync(ic => ic.ItemCategoryLookupId == CategoryId);
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
        async Task<Item> AssignWooProductAttrbiutes(Item currItem, Product currWooProd)
        {
            if (currItem != null)
            {
                bool _isAttribUpdate = currItem.ItemAttributes != null;
                // loop through all the attributes and see if the are used for variations and add all the terms a variations
                foreach (var attrib in currWooProd.attributes)
                {
                    currItem = await AddOrUpdateItemsAttributes(currItem, attrib, _isAttribUpdate);   
                }
            }
            return currItem;   // return the currItem after we have manipulated it.
        }

        async Task<Item> AddOrUpdateItemsAttributes(Item currItem, ProductAttributeLine prodAttrib, bool HasAttributes) 
        {
            // if we get here then we have an item and it has an id
//            IAppRepository<ItemAttributeLookup> _itemAttributeLookupRepo = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            IAppRepository<WooProductAttributeMap> _wooProductAttributeMapRepo = _AppUnitOfWork.Repository<WooProductAttributeMap>();
            // check if this is an update or an add for categories and attributes
            WooProductAttributeMap _wooProductAttributeMap = await _wooProductAttributeMapRepo.FindFirstAsync(wpa => wpa.WooProductAttributeId == prodAttrib.id);
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
                currItem = await AddOrUpdateVarieties(currItem, prodAttrib, _wooProductAttributeMap); //--> need to check each attribute, IsAttributeVarietyUpddate);
                //}
            }
            return currItem;
        }
        /// <summary>
        /// Add or update variation of this item
        /// </summary>
        /// <param name="currItem">the item </param>
        /// <param name="prodAttrib">the attribute</param>
        /// <returns>Item modified with new data</returns>
        async Task<Item> AddOrUpdateVarieties(Item currItem, ProductAttributeLine prodAttrib, WooProductAttributeMap currWooProductAttributeMap) //, bool IsAttributeVarietyUpddate)
        {
            IAppRepository<ItemAttributeVarietyLookup> _itemAttribVarietyLookupRepo = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            ItemAttributeVarietyLookup _itemAttributeVarietyLookup = null;
            foreach (var attrbiTerm in prodAttrib.options)
            {
                _itemAttributeVarietyLookup = await _itemAttribVarietyLookupRepo.FindFirstAsync(ItemAttributeVariety => ItemAttributeVariety.VarietyName == attrbiTerm);
                if (_itemAttributeVarietyLookup != null)
                {
                    // found so update or add
                    ItemAttribute _itemAttribute = currItem.ItemAttributes.Find(ic => ic.ItemAttributeLookupId == currWooProductAttributeMap.ItemAttributeLookupId);
                    if (_itemAttribute == null)
                        return currItem; // this should never occur  

                    // does this attribute have this variety, if so update otherwise add
                    if ((_itemAttribute.ItemAttributeVarieties !=null) && (_itemAttribute.ItemAttributeVarieties.Exists(iav => iav.ItemAttributeVarietyLookupId == _itemAttributeVarietyLookup.ItemAttributeVarietyLookupId)))
                    {
                        // this attribute variety / term exists so just update it. Do stuff here if we need - so far nada
                        //make sure this matches
                        ItemAttributeVariety _itemAttributeVariety = _itemAttribute.ItemAttributeVarieties.FirstOrDefault(iav => iav.ItemAttributeVarietyLookupId == _itemAttributeVarietyLookup.ItemAttributeVarietyLookupId);
                        ///-> can this be null? It should never be
                        // copy the whole item across just in case there have been changes
                        _itemAttributeVariety.ItemAttributeVarietyLookupDetail = _itemAttributeVarietyLookup;   
                        _itemAttributeVariety.ItemAttributeId = _itemAttribute.ItemAttributeId;
                    }
                    else
                    {
                        // we have a attribute variety, this means we should have an attribute that, that belongs too.

                        if (_itemAttribute.ItemAttributeVarieties == null)
                            _itemAttribute.ItemAttributeVarieties = new List<ItemAttributeVariety>();
                        // create a new variety assume 1.0 as in 1-1 QTY and update the ItemDetails. Do not change Item Id as then EF core knows it is a new record.
                        _itemAttribute.ItemAttributeVarieties.Add(new ItemAttributeVariety
                        {
                            ItemAttributeVarietyLookupId = _itemAttributeVarietyLookup.ItemAttributeVarietyLookupId,
                            ItemAttributeVarietyLookupDetail = _itemAttributeVarietyLookup,    // copy the whole attribute across
                            ItemAttributeId = _itemAttribute.ItemAttributeId,
                            UoMId = _itemAttributeVarietyLookup.UoMId,
                            UoM = _itemAttributeVarietyLookup.UoM,
                            UoMQtyPerItem = 1.0
                        }); 
                    }
                }
            }
            return currItem;
        }
        /// <summary>
        ///   CopyWooDetails:
        ///           The copies the info across:
        ///   •	Set ItemName, SKU, IsEnabled, ItemDetail, SourtOrder as per above mapping in table
        ///   •	For ItemCategotyId – selected the first Category in the array find the CategoryId of it using the WooCategoryMapping and set.We assume the first is primary.Perhaps we should look for one those without parents?
        ///   •	For ParentItemId Add to a List ItemParents the ParentId and the ItemId
        /// </summary>
        async Task<Guid> AddItem(Product currWooProd, List<WooItemWithParent> currWooProductsWithParents)
        {
            // using the prod we need to add the item and link all its Settings: AssginedCategory, Attributes and Attribute varieties
            Item _item = CopyWooProductInfo(currWooProd, null, ref currWooProductsWithParents);
            if (_item != null)
            {
                /// once we get here we have a new ID for the ItemID so we can continue
                _item = await AssignWooProductCategory(_item, currWooProd);    // adding the item but not the linked items
                _item = await AssignWooProductAttrbiutes(_item, currWooProd);
                IItemRepository _itemRepo = _AppUnitOfWork.itemRepository();
                if (await _itemRepo.AddItem(_item) == 0)
                {
                    _item.ItemId = Guid.Empty;
                }
            }
            return _item.ItemId;
        }
        /// <summary>
        /// Update an item that has been found in the WooPoduct link
        /// </summary>
        /// <param name="currWooProd"></param>
        /// <param name="currItem"></param>
        /// <param name="currWooProductsWithParents"></param>
        /// <returns></returns>
        async Task<Guid> UpdateItem(Product currWooProd, Item currItem, List<WooItemWithParent> currWooProductsWithParents)
        {
            // using the prod we need to add the item and link all its Settings: AssginedCategory, Attributes and Attribute varieties
            currItem = CopyWooProductInfo(currWooProd, currItem, ref currWooProductsWithParents);
            if (currItem != null)
            {
                //int _RecsDone;  //used for error checking
                /// once we get here we have a new ID for the ItemID so we can continue
                currItem = await AssignWooProductCategory(currItem, currWooProd);
                currItem = await AssignWooProductAttrbiutes(currItem, currWooProd);
                // now update the item
                IItemRepository _itemRepo = _AppUnitOfWork.itemRepository();
                if (await _itemRepo.UpdateAsync(currItem) == 0)
                {
                    currItem.ItemId = Guid.Empty;
                }
            }
            return currItem.ItemId;
        }

        async Task<Guid> AddOrGetIDItem(Product currWooProduct, List<WooItemWithParent> currWooProductsWithParents)
        {
            Guid _itemId = Guid.Empty;
            IItemRepository _ItemRepository = _AppUnitOfWork.itemRepository();
            // check if the Prod exists based on name since there is no mapped id
            Item _item = await _ItemRepository.FindFirstEagerLoadingItemAsync(i => i.ItemName == currWooProduct.name);
            if ((_item == null) && (!String.IsNullOrEmpty(currWooProduct.sku)))     // check if perhaps they renamed the by looking for SKU
            {
                _item = await _ItemRepository.FindFirstEagerLoadingItemAsync(i => i.SKU == currWooProduct.sku);
            }
            if (_item != null)
            {
                _itemId = await UpdateItem(currWooProduct, _item, currWooProductsWithParents);
            }
            else
            {
                _itemId = await AddItem(currWooProduct, currWooProductsWithParents);
            }
            return _itemId;
        }
        async Task<Guid> UpdateItemWithProduct(Product currWooProduct, WooProductMap currWooProductMap, List<WooItemWithParent> currWooProductsWithParents)
        {
            // We have found a mapping between the woo Product and our Product id so update the Attribute table just in case.
            Guid _itemId = Guid.Empty;
            Guid _itemProductId = Guid.Empty;
            IItemRepository _itemRepository = _AppUnitOfWork.itemRepository();
            // check if the Prod based on mapped id
            Item _item = await _itemRepository.FindFirstEagerLoadingItemAsync(i => i.ItemId == currWooProductMap.ItemId); 
            /// Now update the woo Product using the _ItemProductId returned.
            if (_item != null)
            {    // we found the Item Id so update that Id
                _itemId = await UpdateItem(currWooProduct, _item, currWooProductsWithParents);
                currImportCounters.TotalUpdated++;
            }
            else
            {
                // if we got here then the product map is pointing to the wrong ID, so delete the ID and add the item
                await DeleteWooProductMap(currWooProductMap);
                _itemId = await AddProductToItems(currWooProduct, currWooProductsWithParents);
            }
            return _itemId;
        }

        private async Task<int> DeleteWooProductMap(WooProductMap currWooProductMap)
        {
            IAppRepository<WooProductMap> _wooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();
            return await _wooProductMapRepository.DeleteByIdAsync(currWooProductMap.WooProductMapId);
        }

        //does not exists add and CopyWooDetails to new record increase Counter.added
        async Task<Guid> AddProductToItems(Product currWooProd, List<WooItemWithParent> pProductsWithParents)
        {
            Guid _itemId = Guid.Empty;
            IAppRepository<WooProductMap> _wooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();
            //it may not be in the map, but we may have a product with tat name so see if we do -Add Item Product if it does not exist
            _itemId = await AddOrGetIDItem(currWooProd, pProductsWithParents);
            if (_itemId != Guid.Empty)
            {
                WooProductMap _wooProductMap = new WooProductMap
                {
                    WooProductId = (int)currWooProd.id,
                    ItemId = _itemId,
                    CanUpdate = true
                };
                if (await _wooProductMapRepository.AddAsync(_wooProductMap) == AppUnitOfWork.CONST_WASERROR)
                {
                    // did not add so set _ItemProductId to ItemProductID to Guid.Empty = error
                    _itemId = Guid.Empty;
                }
                currImportCounters.TotalAdded++;
            }
            return _itemId;
        }
        /// <summary>
        /// For each product
        ///     o If the product does not exists add and CopyWooDetails to new record increase Counter.added
        ///     o If product does exist and CanUpdate is true then Update by copying the WooDetaits that are not keys, increase Counter.update
        /// </summary>
        async Task<Guid> ImportAndMapWooProductData(Product currWooProd, List<WooItemWithParent> pProductsWithParents)
        {
            Guid _itemProductId = Guid.Empty;
            // Get repository for each database we are accessing. ItemProduct. WooProductMap & WooSyncLog
            IAppRepository<WooProductMap> _wooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();

            // Import the Product and set sync data
            ///first check if it exists in the mapping, just in case there has been a name change
            WooProductMap _wooProductMap = await _wooProductMapRepository.FindFirstAsync(wp => wp.WooProductId == currWooProd.id);
            if (_wooProductMap != null)   // the id exists so update
            {
                if (_wooProductMap.CanUpdate)    /// only if the product can be updated 
                {
                    _itemProductId = await UpdateItemWithProduct(currWooProd, _wooProductMap, pProductsWithParents);
                }
            }
            else      // the id does not exists so add
            {
                _itemProductId = await AddProductToItems(currWooProd, pProductsWithParents);
                currImportCounters.TotalAdded++;
            }

            return _itemProductId;
        }
        // Get the Variety's Parent id using the WooMapping, if is not found then return Empty
        async Task<Guid> GetItemId(int currWooProductId)
        {
            IAppRepository<WooProductMap> _wooProductMapRepo = _AppUnitOfWork.Repository<WooProductMap>();

            WooProductMap _wooProductMap = await _wooProductMapRepo.FindFirstAsync(wpa => wpa.WooProductId == currWooProductId);
            return (_wooProductMap == null) ? Guid.Empty : _wooProductMap.ItemId;
        }
        async Task<bool> FindAndSetParentProduct(WooItemWithParent currWooProductWithAParent)
        {
            ///Logic using the ids passed look for the linked attribute to the id then look for the ParentId  get the Guids of each and update the database
            // Get prodAttributeWithAParent.ID GUID from ItemAttribute Table = ParentID
            // Get prodAttributeWithAParent.AttrID GUID from ItemAttribute Table = ChildID
            // Set the  ItemAttribute.ParentID = ParentID for ItemsAttrib.ID = ChildID
            Guid _attribId = await GetItemId(currWooProductWithAParent.ChildId);
            Guid _parentAttribId = await GetItemId(currWooProductWithAParent.ParentId);

            bool _isDone = await SetParentCategory(_attribId, _parentAttribId);

            await LogImport(currWooProductWithAParent.ChildId, $"Setting of Parent of Child Item id: {currWooProductWithAParent.ChildId} to Parent Id {currWooProductWithAParent.ParentId} status: {_isDone}", Models.WooSections.ProductCategories);
            return _isDone;
        }

        string ProductToString(Product currWooProd, Guid importedId)
        {
            return $"Product {currWooProd.name}, id: {currWooProd.id}, imported and Item Id is {importedId}";
        }
        /// <summary>
        /// Get All Woo Products
        /// Set counters
        /// Import each product
        /// Once finished we need to map an Items parents.We do this by cycling through the list of items that have parents, then we find the item and parent’s GUID and take match them.
        /// </summary>
        async Task<int> ImportProductData(List<Product> _wooProducts)
        {
            Guid _importedId;
            List<WooItemWithParent> _productsWithParents = new List<WooItemWithParent>();
            int _Imported = 0;
            foreach (var wooProd in _wooProducts)
            {
                ImportingThis = $"Importing Product ({currImportCounters.TotalImported}/{currImportCounters.MaxRecs}): {wooProd.name} woo id: {wooProd.id}";
                await LogImport((int)wooProd.id, ImportingThis, Models.WooSections.Products);
                // set the values as per
                _importedId = await ImportAndMapWooProductData(wooProd, _productsWithParents);
                // abort if there was an error - Or should we log and restart? need to restart DbContext somehow
                if (_AppUnitOfWork.IsInErrorState())
                {
                    await LogImport((int)wooProd.id, $"Error occurred importing {wooProd.name}", Models.WooSections.Products);
                    return AppUnitOfWork.CONST_WASERROR;
                }
                _Imported++;
                await LogImport((int)wooProd.id, ProductToString(wooProd, _importedId), Models.WooSections.Products);
                currImportCounters.TotalImported++;
                currImportCounters.PercentOfRecsImported = currImportCounters.CalcPercentage(currImportCounters.TotalImported);
                StateHasChanged();
            }
            // Now we loop through all the Attributes that have parents and find them
            foreach (var ProductWithAParent in _productsWithParents)
            {
                if (!await FindAndSetParentProduct(ProductWithAParent))
                {
                    if (_AppUnitOfWork.IsInErrorState())   // was there an error that was database related?
                        return AppUnitOfWork.CONST_WASERROR;
                }
            }
            return _Imported;
        }
        #endregion

    }
}
