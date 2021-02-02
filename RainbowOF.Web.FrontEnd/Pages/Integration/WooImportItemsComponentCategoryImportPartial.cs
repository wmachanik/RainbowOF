using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RainbowOF.Repositories.Common;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.Items;
using RainbowOF.Models.Woo;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using WooCommerceNET.WooCommerce.v3;
using RainbowOF.Models.Lookups;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImportItemsComponent // for CategoryImport so file name WooImportItemsComponent    {CategoryImport 
    {
        #region ItemCategoryLookupImportStuff
        /// <summary>
        /// All the category import stuff. Could we have generalised this?
        /// </summary>
        /* Objective:

            We create a list of item categories. From that list we need to create a list of primary categories. These are used in the items mapping. Only those that are imported can be used
            Mapping:

            The Table WooCategoryMaps stores the category information.

            Field	                                UsedFor
            WooCategoryID [int]	                    To store the CategoryID  that Woo returns
            WooCategoryName [string (size:2-255)]	To store Categories.name that Woo returns
            WooCategorySlug [string (size:2-255)]	To store Categories.slug that Woo returns
            WooCategoryParentID	                    Can be null if not will point to a WooCategoryID
            ItemCategoryLookupID	                        This links to the Category in the system. Allows us to transfer the data.

            Item categories Should be linked to tracking so items in similar categories are tracked similarly.
        */
        // Retrieve data from Woo
        public async Task<List<ProductCategory>> GetWooCategoryData()
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

            WooProductCategory _WooProductCategory = new WooProductCategory(_WooAPISettings, Logger);
            //List<ProductCategory> wooProductCategories = await _WooProductCategory.GetAllProductCategories();
            return await _WooProductCategory.GetAllProductCategories();
            //return wooProductCategories;
        }
        async Task<Guid> UpdateItemCategoryLookup(ProductCategory pPC, ItemCategoryLookup pItemCategoryLookup, List<WooItemWithParent> pAttribsWithParents)
        {
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

            pItemCategoryLookup.CategoryName = pPC.name;
            // do not set parent Id here since it could cause datbase problems - if it is already null then it will be updated later.
            // pItemCategoryLookup.ParentCategoryId = Guid.Empty;   /// need to find the  parentid if it exists - or need to say it does not exists so that we can look later?
            if (pPC.parent > 0)
            {
                pAttribsWithParents.Add(new WooItemWithParent
                {
                    ChildId = (int)pPC.id,
                    ParentId = (int)pPC.parent
                });
            }
            pItemCategoryLookup.Notes = $"Updated Woo Category ID {pPC.id}";
            bool _success = await _ItemCategoryLookupRepository.UpdateAsync(pItemCategoryLookup) != AppUnitOfWork.CONST_WASERROR;
            return (_success) ? pItemCategoryLookup.ItemCategoryLookupId : Guid.Empty;
        }
        public async Task<Guid> AddOrGetIDItemCategoryLookup(ProductCategory pPC, List<WooItemWithParent> pCategoriesWithParents)
        {
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

            ItemCategoryLookup _ItemCategoryLookup = await _ItemCategoryLookupRepository.FindFirstAsync(ic => ic.CategoryName == pPC.name);
            if (_ItemCategoryLookup == null)
            {
                ItemCategoryLookup _newItemCategoryLookup = new ItemCategoryLookup
                {
                    CategoryName = pPC.name,
                    //  Null;? ParentCategoryId = Guid.Empty,
                    Notes = $"Imported Woo Category ID {pPC.id}"
                };
                if (pPC.parent > 0)
                {
                    pCategoriesWithParents.Add(new WooItemWithParent
                    {
                        ChildId = (int)pPC.id,
                        ParentId = (int)pPC.parent
                    });
                }

                await _ItemCategoryLookupRepository.AddAsync(_newItemCategoryLookup);
                return _newItemCategoryLookup.ItemCategoryLookupId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        async Task<Guid> AddOrUpdateItemCategoryLookup(ProductCategory pPC, Guid pWooMappedItemCategoryLookupId, List<WooItemWithParent> pCategoriesWithParents)
        {
            Guid _ItemCategoryLookupId = Guid.Empty;
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            // check if the category existgs
            ItemCategoryLookup _ItemCategoryLookup = await _ItemCategoryLookupRepository.FindFirstAsync(ic => ic.ItemCategoryLookupId == pWooMappedItemCategoryLookupId);
            if (_ItemCategoryLookup != null)
            {
                _ItemCategoryLookupId = await UpdateItemCategoryLookup(pPC, _ItemCategoryLookup, pCategoriesWithParents);
            }
            else
            {
                _ItemCategoryLookupId = await AddOrGetIDItemCategoryLookup(pPC, pCategoriesWithParents);
            }
            return _ItemCategoryLookupId;
        }
        async Task<Guid> UpdateProductCategory(ProductCategory pPC, WooCategoryMap pWooCategoryMap, List<WooItemWithParent> pCategoriesWithParents)
        {
            Guid _ItemCategoryLookupId = Guid.Empty;
            IAppRepository<WooCategoryMap> _WooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // copy data across
            pWooCategoryMap.WooCategoryName = pPC.name;
            pWooCategoryMap.WooCategorySlug = pPC.slug;
            pWooCategoryMap.WooCategoryParentId = pPC.parent;
            _ItemCategoryLookupId = await AddOrUpdateItemCategoryLookup(pPC, pWooCategoryMap.ItemCategoryLookupId, pCategoriesWithParents);
            if (_ItemCategoryLookupId != Guid.Empty)
            {
                /// Now update the woo categorY using the _ItemCategoryLookupId returned.
                if (await _WooCategoryMapRepository.UpdateAsync(pWooCategoryMap) == AppUnitOfWork.CONST_WASERROR)
                {
                    // did not updated so set _ItemCategoryLookupId to ItemCategoryLookupID to Guid.Empty = error
                    _ItemCategoryLookupId = Guid.Empty;
                }
            }
            return _ItemCategoryLookupId;
        }
        async Task<Guid> AddProductCategory(ProductCategory pPC, List<WooItemWithParent> pCategoriesWithParents)
        {
            Guid _ItemCategoryLookupId = Guid.Empty;
            IAppRepository<WooCategoryMap> _WooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // Add Item Category if it does not exist
            _ItemCategoryLookupId = await AddOrGetIDItemCategoryLookup(pPC, pCategoriesWithParents);
            if (_ItemCategoryLookupId != Guid.Empty)
            {
                WooCategoryMap _WooCategoryMap = new WooCategoryMap
                {
                    WooCategoryName = pPC.name,
                    WooCategorySlug = pPC.slug,
                    WooCategoryParentId = pPC.parent,
                    ItemCategoryLookupId = _ItemCategoryLookupId,
                    WooCategoryId = (int)pPC.id
                };
                //else  was check if woomap was null
                //{
                //    _WooCategoryMap.WooCategoryName = pPC.name;
                //    _WooCategoryMap.WooCategorySlug = pPC.slug;
                //    _WooCategoryMap.WooCategoryParentId = pPC.parent;
                //    _WooCategoryMap.ItemCategoryLookupId = _ItemCategoryLookupId;
                //    _WooCategoryMap.WooCategoryId = (int)pPC.id;
                //}
                if (await _WooCategoryMapRepository.AddAsync(_WooCategoryMap) == AppUnitOfWork.CONST_WASERROR)
                {
                    // did not add so set _ItemCategoryLookupId to ItemCategoryLookupID to Guid.Empty = error
                    _ItemCategoryLookupId = Guid.Empty;
                }
            }

            return _ItemCategoryLookupId;
        }
        async Task<Guid> ImportAndMapCategoryData(ProductCategory pPC, List<WooItemWithParent> pCategoriesWithParents)
        {
            Guid _ItemCategoryLookupId = Guid.Empty;
            // Get repostiory for each database we are accessing. ItemCategoryLookup. WooProductCategoryMap & WooSyncLog
            IAppRepository<WooCategoryMap> _WooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // Import the category and set sync data
            ///first check if it exists in the mapping, just incase there has been a name change
            WooCategoryMap _WooCategoryMap = await _WooCategoryMapRepository.FindFirstAsync(wpc => wpc.WooCategoryId == pPC.id);
            if (_WooCategoryMap != null)                  // the id exists so update
            {
                _ItemCategoryLookupId = await UpdateProductCategory(pPC, _WooCategoryMap, pCategoriesWithParents);
                _importCounters.TotalUpdated++;
            }
            else                  // the id does not exists so add
            {
                _ItemCategoryLookupId = await AddProductCategory(pPC, pCategoriesWithParents);
                _importCounters.TotalAdded++;
            }

            return _ItemCategoryLookupId;
        }
        // string
        string ProductCatToString(ProductCategory pPC, Guid pImportedId)
        {
            return $"Product Category {pPC.name}, id: {pPC.id}, imported and Category Id is {pImportedId}";
        }
        /// <summary>
        /// Get Our Category Id using a Woo Category Id
        /// </summary>
        /// <param name="pWooCategoryId">The Woo Category Id</param>
        /// <returns></returns>
        async Task<Guid> GetCategoryById(int pWooCategoryId)
        {
            // using hte category woo category mapping find the assocated ID
            IAppRepository<WooCategoryMap> _WooCatrgoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            WooCategoryMap _WooCategoryMap = await _WooCatrgoryMapRepository.FindFirstAsync(wc => wc.WooCategoryId == pWooCategoryId);
            return (_WooCategoryMap == null) ? Guid.Empty : _WooCategoryMap.ItemCategoryLookupId;

        }
        async Task<bool> SetParentCategory(Guid pChildWooCategoryId, Guid pParentWooCategoryId)
        {
            // using the guids of the category id update the parent of that record
            bool _IsUpdated = false;

            if ((pChildWooCategoryId != Guid.Empty) && (pParentWooCategoryId != Guid.Empty))
            {
                IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

                ItemCategoryLookup _ItemCategoryLookup = await _ItemCategoryLookupRepository.FindFirstAsync(ic => ic.ItemCategoryLookupId == pChildWooCategoryId);
                if (_ItemCategoryLookup == null)
                    _IsUpdated = false;
                else
                {     // found so update
                    _ItemCategoryLookup.ParentCategoryId = pParentWooCategoryId;
                    _IsUpdated = (await _ItemCategoryLookupRepository.UpdateAsync(_ItemCategoryLookup)) != AppUnitOfWork.CONST_WASERROR;
                }
            }
            return _IsUpdated;
        }

        async Task<bool> FindAndSetParentCategory(WooItemWithParent pCategoryWithParent)
        {
            ///Logic using the ids passed look for the linked attribute to the id then look for the parentid  get the guids of each and update thed atabase
            // Get pAttributeWithAParent.ID GUID from ItemAttribute Table = ParentID
            // Get pAttributeWithAParent.AttrID GUID from ItemAttribute Table = ChildID
            // Set the  ItemAttribute.ParentID = ParentID for ItemsAttrib.ID = ChildID
            Guid _AttribId = await GetCategoryById(pCategoryWithParent.ChildId);
            Guid _ParentAttribId = await GetCategoryById(pCategoryWithParent.ParentId);

            bool _IsDone = await SetParentCategory(_AttribId, _ParentAttribId);

            await LogImport(pCategoryWithParent.ChildId, $"Setting of Parent of Child Category id: {pCategoryWithParent.ChildId} to Parent Id {pCategoryWithParent.ParentId} status: {_IsDone}", Models.WooSections.ProductCategories);
            return _IsDone;
        }

        // cycle through catagories and add to database if they do not exists
        // Store a WooReultsDate so we can filter the results later
        // log each category and what we do with t in the log and in the WooResults
        public async Task<int> ImportCategoryData(List<ProductCategory> pWooProductCategories)
        {
            int _Imported = 0;
            Guid _IdImported;
            //            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            List<WooItemWithParent> CategoriessWithParents = new List<WooItemWithParent>();

            //// Load the current itemCategoriers
            // cycle through catagories and add to database if they do not exists
            foreach (var pc in pWooProductCategories)
            {
                ImportingThis = $"Importing Category ({_importCounters.TotalImported}/{_importCounters.MaxRecs}): {pc.name}";
                // Import the categories that have count  > 0
                if (pc.count > 0)
                {
                    // set the values as per
                    _IdImported = await ImportAndMapCategoryData(pc, CategoriessWithParents);
                    _Imported++;
                    await LogImport((int)pc.id, ProductCatToString(pc, _IdImported), Models.WooSections.ProductCategories);
                }
                if (_AppUnitOfWork.IsInErrorState())
                    return 0;
                _importCounters.TotalImported++;
                _importCounters.PercentOfRecsImported = _importCounters.CalcPercentage(_importCounters.TotalImported);
                StateHasChanged();
            }
            // Now we loop through all the Attribues that have parents and find them
            foreach (var AttributeWithAParent in CategoriessWithParents)
            {
                if (!await FindAndSetParentCategory(AttributeWithAParent))
                {
                    if (_AppUnitOfWork.IsInErrorState())   // was there an error that was database related?
                        return 0;
                }
            }
            return _Imported;
        }
        #endregion


    }
}
