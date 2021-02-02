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
        /// All the atribute term import stuff. Attributes Terms in Woo are Attributed Varieties to us. Could we have generalised this for ezch item import with an object?
        /// </summary>
        #region AttrbiuteStuff

        // Retrieve data from Woo
        async Task<List<Product>> GetAllWooProducts(bool OnlyItemsInStock)
        {
            WooAPISettings _WooAPISettings = new WooAPISettings
            {
                ConsumerKey = WooSettingsModel.ConsumerKey,
                ConsumerSecret = WooSettingsModel.ConsumerSecret,
                QueryURL = WooSettingsModel.QueryURL,
                IsSecureURL = WooSettingsModel.IsSecureURL,
                JSONAPIPostFix = WooSettingsModel.JSONAPIPostFix,
                RootAPIPostFix = WooSettingsModel.RootAPIPostFix
            };

            WooProduct _WooProduct = new WooProduct(_WooAPISettings, Logger);
            List<Product> wooProducts = OnlyItemsInStock ?
                await _WooProduct.GetAllProductsInStock() :
                await _WooProduct.GetAllProducts();

            // unique check not needed 
            //  var _wooProducts = wooProducts.GroupBy(wp => wp.id).Select(wp => wp.FirstOrDefault()); // wooProducts that are distinct;
            //  wooProducts = _wooProducts.ToList();
            return wooProducts;
        }
        // there is essentiall on master item and then variations. how those are represented in a order is eitehr each or using UOOm a qty of each
        // this routine is used both for update and add/create. If null create a new one otherwise update, then bsic Product info across
        private Item CopyWooProductInfo(Product pWooProd, Item pItem, ref List<WooItemWithParent> pWooProductsWithParents)
        {
            if (pItem == null)
                pItem = new Item();
            pItem.ItemName = _StringTools.Truncate(pWooProd.name, 100);  // max length is 100
            pItem.SKU = _StringTools.Truncate(pWooProd.sku, 50);  // max length is 50
            pItem.IsEnabled = true;
            pItem.ItemDetail = _StringTools.Truncate(_StringTools.StripHTML(pWooProd.short_description), 250);   // max length is 255
            pItem.ItemAbbreviatedName = _StringTools.MakeAbbriviation(pWooProd.name);
            pItem.SortOrder = Convert.ToInt32(pWooProd.menu_order);  //  (pWooProd.menu_order == null) ? 50 : (int)pWooProd.menu_order;
            pItem.BasePrice = Convert.ToDecimal(pWooProd.price); // (pWooProd.price == null) ? 0 :(decimal)pWooProd.price;

            if ((pWooProd.parent_id != null) && (pWooProd.parent_id > 0))
                pWooProductsWithParents.Add(new WooItemWithParent
                {
                    ChildId = (int)pWooProd.id,
                    ParentId = (int)pWooProd.parent_id
                });
            return pItem;
        }
        /// <summary>
        /// Using the item sent assign the categoriues tha thw WooProduct has. Remeber that wedo not affect ItemId since the EF core 
        /// </summary>
        /// <param name="Item">The Priduct Item t add too</param>
        /// <param name="WooProduct">The Woo Product Source</param>
        /// <returns></returns>
        async Task<Item> AssignWooProductCategory(Item pItem, Product pWooProd)
        {
            bool _IsCatUpdate = pItem.ItemCategories != null;
            if (pItem != null)
            {
                // if we get here then we have an item and it has an id
                IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepo = _AppUnitOfWork.Repository<ItemCategoryLookup>();
                ItemCategoryLookup _ItemCategoryLookup = null;
                bool _SetPrimary = true;
                foreach (var cat in pWooProd.categories)
                {
                    Guid CategoryId = await GetCategoryById((int)cat.id);
                    _ItemCategoryLookup = await _itemCategoryLookupRepo.FindFirstAsync(ic => ic.ItemCategoryLookupId == CategoryId);
                    if (_ItemCategoryLookup != null)
                    {
                        ///////// checkif exists if this is not a new item
                        if ((_IsCatUpdate) && (pItem.ItemCategories.Exists(ic => ic.ItemCategoryLookupId == _ItemCategoryLookup.ItemCategoryLookupId)))
                        {
                            // this attribute exists so just update it - nothing todo here at the moment.
                        }
                        else
                        {
                            // add an item to the linked list. Since it is a new item the FK must be guid.empty for efcore to work
                            if (pItem.ItemCategories == null)
                                pItem.ItemCategories = new List<ItemCategory>();
                            pItem.ItemCategories.Add(new ItemCategory
                            {
                                ItemCategoryLookupId = _ItemCategoryLookup.ItemCategoryLookupId
                            });
                            if (_SetPrimary)    // assume the first in the list is the primary, as we have no otherw way of knowing.
                            {
                                pItem.PrimaryItemCategoryLookupId = _ItemCategoryLookup.ItemCategoryLookupId;
                                _SetPrimary = false;
                            }
                        }
                    }
                }
            }
            return pItem;   // reutrn the pItem afdter we have manipulated it.
        }
        async Task<Item> AssignWooProductAttrbiutes(Item pItem, Product pWooProd)
        {
            bool _IsAttribUpdate = pItem.ItemAttributes != null;
            bool _IsAttrVarietyUpdate = pItem.ItemAttributeVarieties != null;
            if (pItem != null)
            {
                // loop through all the attributes and see if the are used for variations and add all the terms a variations
                foreach (var attrib in pWooProd.attributes)
                {
                    pItem = await AddOrUpdateItemsAttributes(pItem, attrib, _IsAttribUpdate, _IsAttrVarietyUpdate);
                }
            }
            return pItem;   // reutrn the pItem afdter we have manipulated it.
        }

        async Task<Item> AddOrUpdateItemsAttributes(Item pItem, ProductAttributeLine pAttrib, bool pIsAttribUpdate, bool pIsAttrVarietyUpdate)
        {
            // if we get here then we have an item and it has an id
            IAppRepository<ItemAttributeLookup> _itemAttributeLookupRepo = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            IAppRepository<WooProductAttributeMap> _wooProductAttributeMapRepo = _AppUnitOfWork.Repository<WooProductAttributeMap>();
            // check if this is an update or an add for categories and attributes
            WooProductAttributeMap _wooProductAttributeMap = await _wooProductAttributeMapRepo.FindFirstAsync(wpa => wpa.WooProductAttributeId == pAttrib.id);
            if (_wooProductAttributeMap != null)
            {
                if ((pIsAttribUpdate) && (pItem.ItemAttributes.Exists(ic => ic.ItemAttributeLookupId == _wooProductAttributeMap.ItemAttributeLookupId)))
                {
                    // this attribute exists so just update it
                    ItemAttribute _itemAttribute = pItem.ItemAttributes.Find(ic => ic.ItemAttributeLookupId == _wooProductAttributeMap.ItemAttributeLookupId);
                    _itemAttribute.IsUsedForVariableType = pAttrib.variation == null ? false : (bool)pAttrib.variation;  // the only thing that we can really update
                }
                else
                {
                    if (pItem.ItemAttributes == null)
                        pItem.ItemAttributes = new List<ItemAttribute>();
                    // create a new attribute and update the itemdetails. Do not change Item Id as then EF core knows it is a new record.
                    pItem.ItemAttributes.Add(new ItemAttribute
                    {
                        IsUsedForVariableType = pAttrib.variation == null ? false : (bool)pAttrib.variation,
                        ItemAttributeLookupId = _wooProductAttributeMap.ItemAttributeLookupId,
                    });
                }
                if (Convert.ToBoolean(pAttrib.variation))
                {
                    pItem = await AddOrUpdateVarieties(pItem, pAttrib, pIsAttrVarietyUpdate);
                }
            }
            return pItem;
        }
        /// <summary>
        /// Add or update variation of this item
        /// </summary>
        /// <param name="pItem">the item </param>
        /// <param name="pAttrib">the attribut</param>
        /// <param name="pIsAttrVarietyUpdate">the attribute variation/term</param>
        /// <returns></returns>

        async Task<Item> AddOrUpdateVarieties(Item pItem, ProductAttributeLine pAttrib, bool pIsAttrVarietyUpdate)
        {
            IAppRepository<ItemAttributeVarietyLookup> _itemAttribVarietyLookup = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            ItemAttributeVarietyLookup _itemAttributeVarietyLkup = null;
            foreach (var attrbiTerm in pAttrib.options)
            {
                _itemAttributeVarietyLkup = await _itemAttribVarietyLookup.FindFirstAsync(ItemAttributeVariety => ItemAttributeVariety.VarietyName == attrbiTerm);

                if (_itemAttributeVarietyLkup != null)
                {    // found so update or add
                    if ((pIsAttrVarietyUpdate) && (pItem.ItemAttributeVarieties.Exists(iav => iav.ItemAttributeVarietyLookupId == _itemAttributeVarietyLkup.ItemAttributeVarietyLookupId)))
                    {
                        // this attribute exists so just update it. Do stuff here if we need - so far nada
                    }
                    else
                    {
                        if (pItem.ItemAttributeVarieties == null)
                            pItem.ItemAttributeVarieties = new List<ItemAttributeVariety>();
                        // create a new variety assume 1.0 as in 1-1 Qty and update the itemdetails. Do not change Item Id as then EF core knows it is a new record.
                        pItem.ItemAttributeVarieties.Add(new ItemAttributeVariety
                        {
                            ItemAttributeVarietyLookupId = _itemAttributeVarietyLkup.ItemAttributeVarietyLookupId,
                            UoMQtyPerItem = 1.0
                        });
                    }
                }
            }
            return pItem;
        }

        /// <summary>
        ///   CopyWooDetails:
        ///           The copies the info across:
        ///   •	Set ItemName, SKU, IsEnabled, ItemDetail, SourtOrder as per above mapping in table
        ///   •	For ItemCategotyId – selected the first Category in the array find the CategoryId of it using the WooCategoryMapping and set.We assume the first is primary.Perhaps we should look for one those without parents?
        ///   •	For ParentItemId Add to a List ItemParents the ParentId and the ItemId
        /// </summary>
        async Task<Guid> AddItem(Product pWooProd, List<WooItemWithParent> pWooProductsWithParents)
        {
            // using the prod we need to add the item and link all its Settings: AssginedCategory, Attributes and Attribute variteies
            Item _item = CopyWooProductInfo(pWooProd, null, ref pWooProductsWithParents);
            if (_item != null)
            {
                /// once we get here we have a new ID for the ItemID so we can continue
                _item = await AssignWooProductCategory(_item, pWooProd);    // adding the item but not the linked items
                _item = await AssignWooProductAttrbiutes(_item, pWooProd);
                IItemRepository _itemRepo = _AppUnitOfWork.itemRepository();
                if (await _itemRepo.AddItem(_item) == 0)
                {
                    _item.ItemId = Guid.Empty;
                }
            }
            return _item.ItemId;
        }
        /// <summary>
        /// Update an item that has been found in the wooproduct link
        /// </summary>
        /// <param name="pWooProd"></param>
        /// <param name="pItem"></param>
        /// <param name="pWooProductsWithParents"></param>
        /// <returns></returns>
        async Task<Guid> UpdateItem(Product pWooProd, Item pItem, List<WooItemWithParent> pWooProductsWithParents)
        {
            // using the prod we need to add the item and link all its Settings: AssginedCategory, Attributes and Attribute variteies

            pItem = CopyWooProductInfo(pWooProd, pItem, ref pWooProductsWithParents);
            if (pItem != null)
            {
                //int _RecsDone;  //used for error checking
                /// once we get here we have a new ID for the ItemID so we can continue
                pItem = await AssignWooProductCategory(pItem, pWooProd);
                pItem = await AssignWooProductAttrbiutes(pItem, pWooProd);
                // now update the item
                IItemRepository _itemRepo = _AppUnitOfWork.itemRepository();
                if (await _itemRepo.UpdateAsync(pItem) == 0)
                {
                    pItem.ItemId = Guid.Empty;
                }
            }
            return pItem.ItemId;
        }

        async Task<Guid> AddOrGetIDItem(Product pWooProduct, List<WooItemWithParent> pWooProductsWithParents)
        {
            Guid _ItemId = Guid.Empty;
            IItemRepository _ItemRepository = _AppUnitOfWork.itemRepository();
            // check if the Prod exists based on name since there is no mapped id
            Item _Item = await _ItemRepository.FindFirstWholeItemAsync(i => i.ItemName == pWooProduct.name);
            if ((_Item == null) && (!String.IsNullOrEmpty(pWooProduct.sku)))     // check if perhaps they renamed the by looking for SKU
            {
                _Item = await _ItemRepository.FindFirstWholeItemAsync(i => i.SKU == pWooProduct.sku);
            }
            if (_Item != null)
            {
                _ItemId = await UpdateItem(pWooProduct, _Item, pWooProductsWithParents);
            }
            else
            {
                _ItemId = await AddItem(pWooProduct, pWooProductsWithParents);
            }
            return _ItemId;
        }
        async Task<Guid> UpdateItemWithProduct(Product pWooProduct, WooProductMap pWooProductMap, List<WooItemWithParent> pWooProductsWithParents)
        {
            Guid _ItemId = Guid.Empty;
            // we have found a mapping between the woo Product Product and our Product id so update the Attrbiute table just incase.
            Guid _ItemProductId = Guid.Empty;
            IItemRepository _ItemRepository = _AppUnitOfWork.itemRepository();
            // check if the Prod based on mapped id
            Item _Item = await _ItemRepository.FindFirstWholeItemAsync(i => i.ItemId == pWooProductMap.ItemId);  // we found the Item Id so update that Id
            /// Now update the woo Product using the _ItemProductId returned.
            if (_Item != null)
            {
                _ItemId = await UpdateItem(pWooProduct, _Item, pWooProductsWithParents);
                _importCounters.TotalUpdated++;
            }
            else
            {
                // if we got here then the product map is pointing to the wrong ID, so delete the ID and add the item
                await DeleteWooProductMap(pWooProductMap);
                _ItemId = await AddProductToItems(pWooProduct, pWooProductsWithParents);
            }
            return _ItemId;
        }

        private async Task<int> DeleteWooProductMap(WooProductMap pWooProductMap)
        {
            IAppRepository<WooProductMap> _WooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();
            return await _WooProductMapRepository.DeleteByIdAsync(pWooProductMap.WooProductMapId);
        }

        //does not exists add and CopyWooDetails to new record increase Counter.added
        async Task<Guid> AddProductToItems(Product pWooProd, List<WooItemWithParent> pProductsWithParents)
        {
            Guid _ItemId = Guid.Empty;
            IAppRepository<WooProductMap> _WooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();
            //it may not be in the map, but we may have a proudct with tat name so see if we do -Add Item Product if it does not exist
            _ItemId = await AddOrGetIDItem(pWooProd, pProductsWithParents);
            if (_ItemId != Guid.Empty)
            {
                WooProductMap _WooProductMap = new WooProductMap
                {
                    WooProductId = (int)pWooProd.id,
                    ItemId = _ItemId,
                    CanUpdate = true
                };
                if (await _WooProductMapRepository.AddAsync(_WooProductMap) == AppUnitOfWork.CONST_WASERROR)
                {
                    // did not add so set _ItemProductId to ItemProductID to Guid.Empty = error
                    _ItemId = Guid.Empty;
                }
                _importCounters.TotalAdded++;
            }
            return _ItemId;
        }
        /// <summary>
        /// For each product
        ///     o If the product does not exists add and CopyWooDetails to new record increase Counter.added
        ///     o If product does exist and CanUpdate is true then Update by copying the WooDetaits that are not keys, increase Counter.update
        /// </summary>
        async Task<Guid> ImportAndMapWooProductData(Product pWooProd, List<WooItemWithParent> pProductsWithParents)
        {
            Guid _ItemProductId = Guid.Empty;
            // Get repostiory for each database we are accessing. ItemProduct. WooProductMap & WooSyncLog
            IAppRepository<WooProductMap> _WooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();

            // Import the Product and set sync data
            ///first check if it exists in the mapping, just incase there has been a name change
            WooProductMap _WooProductMap = await _WooProductMapRepository.FindFirstAsync(wp => wp.WooProductId == pWooProd.id);
            if (_WooProductMap != null)   // the id exists so update
            {
                if (_WooProductMap.CanUpdate)    /// only if the product can be updated 
                {
                    _ItemProductId = await UpdateItemWithProduct(pWooProd, _WooProductMap, pProductsWithParents);
                }
            }
            else      // the id does not exists so add
            {
                _ItemProductId = await AddProductToItems(pWooProd, pProductsWithParents);
                _importCounters.TotalAdded++;
            }

            return _ItemProductId;
        }
        // Get the Variety's Parent id using the WooMapping, if is not found then return Empty
        async Task<Guid> GetItemId(int pWooProductId)
        {
            IAppRepository<WooProductMap> _WooProductMapRepo = _AppUnitOfWork.Repository<WooProductMap>();

            WooProductMap _WooProductMap = await _WooProductMapRepo.FindFirstAsync(wpa => wpa.WooProductId == pWooProductId);
            return (_WooProductMap == null) ? Guid.Empty : _WooProductMap.ItemId;

        }
        async Task<bool> FindAndSetParentProduct(WooItemWithParent pWooProductWithAParent)
        {
            ///Logic using the ids passed look for the linked attribute to the id then look for the parentid  get the guids of each and update thed atabase
            // Get pAttributeWithAParent.ID GUID from ItemAttribute Table = ParentID
            // Get pAttributeWithAParent.AttrID GUID from ItemAttribute Table = ChildID
            // Set the  ItemAttribute.ParentID = ParentID for ItemsAttrib.ID = ChildID
            Guid _AttribId = await GetItemId(pWooProductWithAParent.ChildId);
            Guid _ParentAttribId = await GetItemId(pWooProductWithAParent.ParentId);

            bool _IsDone = await SetParentCategory(_AttribId, _ParentAttribId);

            await LogImport(pWooProductWithAParent.ChildId, $"Setting of Parent of Child Item id: {pWooProductWithAParent.ChildId} to Parent Id {pWooProductWithAParent.ParentId} status: {_IsDone}", Models.WooSections.ProductCategories);
            return _IsDone;
        }

        string ProductToString(Product pWooProd, Guid pImportedId)
        {
            return $"Product {pWooProd.name}, id: {pWooProd.id}, imported and Item Id is {pImportedId}";
        }
        /// <summary>
        /// Get All Woo Products
        /// Set counters
        /// Import each product
        /// Once finished we need to map an Items parents.We do this by cycling through the list of items that have parents, then we find the item and parent’s GUID and take match them.
        /// </summary>
        async Task<int> ImportProductData(List<Product> _WooProducts)
        {
            Guid _IdImported;
            List<WooItemWithParent> _ProductsWithParents = new List<WooItemWithParent>();
            int _Imported = 0;
            foreach (var wooProd in _WooProducts)
            {
                ImportingThis = $"Importing Product ({_importCounters.TotalImported}/{_importCounters.MaxRecs}): {wooProd.name} woo id: {wooProd.id}";
                await LogImport((int)wooProd.id, ImportingThis, Models.WooSections.Products);
                // set the values as per
                _IdImported = await ImportAndMapWooProductData(wooProd, _ProductsWithParents);

                // varriations from item to item cannot be duplicated so kalita 155 / 185 fitler cannot have also brewer
                _Imported++;
                await LogImport((int)wooProd.id, ProductToString(wooProd, _IdImported), Models.WooSections.Product);
                if (_AppUnitOfWork.IsInErrorState())
                    return AppUnitOfWork.CONST_WASERROR;
                _importCounters.TotalImported++;
                _importCounters.PercentOfRecsImported = _importCounters.CalcPercentage(_importCounters.TotalImported);
                StateHasChanged();
            }
            // Now we loop through all the Attribues that have parents and find them
            foreach (var ProductWithAParent in _ProductsWithParents)
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
