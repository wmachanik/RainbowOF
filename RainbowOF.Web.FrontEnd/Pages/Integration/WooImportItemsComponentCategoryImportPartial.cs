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
            WooAPISettings _wooAPISettings = new WooAPISettings(AppWooSettings);
   

            IWooProductCategory _WooProductCategory = new WooProductCategory(_wooAPISettings, _Logger);
            //List<ProductCategory> wooProductCategories = await _WooProductCategory.GetAllProductCategories();
            return await _WooProductCategory.GetAllProductCategoriesAsync();
            //return wooProductCategories;
        }
        async Task<Guid> UpdateItemCategoryLookup(ProductCategory updatedPC, ItemCategoryLookup updatedItemCategoryLookup, List<WooItemWithParent> sourceAttribsWithParents)
        {
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

            updatedItemCategoryLookup.CategoryName = updatedPC.name;
            updatedItemCategoryLookup.UsedForPrediction = (updatedPC.parent == null) || (updatedPC.parent == 0);
            // do not set parent Id here since it could cause database problems - if it is already null then it will be updated later.
            // pItemCategoryLookup.ParentCategoryId = Guid.Empty;   /// need to find the  ParentId if it exists - or need to say it does not exists so that we can look later?
            if (updatedPC.parent > 0)
            {
                sourceAttribsWithParents.Add(new WooItemWithParent
                {
                    ChildId = (int)updatedPC.id,
                    ParentId = (int)updatedPC.parent
                });
            }
            updatedItemCategoryLookup.Notes = $"Updated Woo Category ID {updatedPC.id}";
            bool _success = await _itemCategoryLookupRepository.UpdateAsync(updatedItemCategoryLookup) != AppUnitOfWork.CONST_WASERROR;
            return (_success) ? updatedItemCategoryLookup.ItemCategoryLookupId : Guid.Empty;
        }
        public async Task<Guid> AddOrGetIDItemCategoryLookup(ProductCategory sourcePC, List<WooItemWithParent> sourceCategoriesWithParents)
        {
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

            ItemCategoryLookup _itemCategoryLookup = await _itemCategoryLookupRepository.FindFirstAsync(ic => ic.CategoryName == sourcePC.name);
            if (_itemCategoryLookup == null)
            {
                ItemCategoryLookup _newItemCategoryLookup = new ItemCategoryLookup
                {
                    CategoryName = sourcePC.name,
                    //  Null;? ParentCategoryId = Guid.Empty,
                    UsedForPrediction = (sourcePC.parent == null) || (sourcePC.parent == 0),
                    Notes = $"Imported Woo Category ID {sourcePC.id}"
                };
                if (sourcePC.parent > 0)
                {
                    sourceCategoriesWithParents.Add(new WooItemWithParent
                    {
                        ChildId = (int)sourcePC.id,
                        ParentId = (int)sourcePC.parent
                    });
                }

                await _itemCategoryLookupRepository.AddAsync(_newItemCategoryLookup);
                return _newItemCategoryLookup.ItemCategoryLookupId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        async Task<Guid> AddOrUpdateItemCategoryLookup(ProductCategory sourcePC, Guid sourceWooMappedItemCategoryLookupId, List<WooItemWithParent> sourceCategoriesWithParents)
        {
            Guid _itemCategoryLookupId = Guid.Empty;
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            // check if the category exists
            ItemCategoryLookup _itemCategoryLookup = await _itemCategoryLookupRepository.FindFirstAsync(ic => ic.ItemCategoryLookupId == sourceWooMappedItemCategoryLookupId);
            if (_itemCategoryLookup != null)
            {
                _itemCategoryLookupId = await UpdateItemCategoryLookup(sourcePC, _itemCategoryLookup, sourceCategoriesWithParents);
            }
            else
            {
                _itemCategoryLookupId = await AddOrGetIDItemCategoryLookup(sourcePC, sourceCategoriesWithParents);
            }
            return _itemCategoryLookupId;
        }
        async Task<Guid> UpdateProductCategory(ProductCategory updatedPC, WooCategoryMap targetWooCategoryMap, List<WooItemWithParent> sourceCategoriesWithParents)
        {
            Guid _itemCategoryLookupId = Guid.Empty;
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();
            // copy data across
            targetWooCategoryMap.WooCategoryName = updatedPC.name;
            targetWooCategoryMap.WooCategorySlug = updatedPC.slug;
            targetWooCategoryMap.WooCategoryParentId = (int) updatedPC.parent;
            _itemCategoryLookupId = await AddOrUpdateItemCategoryLookup(updatedPC, targetWooCategoryMap.ItemCategoryLookupId, sourceCategoriesWithParents);
            if (_itemCategoryLookupId != Guid.Empty)
            {
                /// Now update the woo categorY using the _itemCategoryLookupId returned.
                if (await _wooCategoryMapRepository.UpdateAsync(targetWooCategoryMap) == AppUnitOfWork.CONST_WASERROR)
                {
                    // did not updated so set _itemCategoryLookupId to ItemCategoryLookupID to Guid.Empty = error
                    _itemCategoryLookupId = Guid.Empty;
                }
            }
            return _itemCategoryLookupId;
        }
        async Task<Guid> AddProductCategory(ProductCategory newPC, List<WooItemWithParent> newCategoriesWithParents)
        {
            Guid _itemCategoryLookupId = Guid.Empty;
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // Add Item Category if it does not exist
            _itemCategoryLookupId = await AddOrGetIDItemCategoryLookup(newPC, newCategoriesWithParents);
            if (_itemCategoryLookupId != Guid.Empty)
            {
                WooCategoryMap _wooCategoryMap = new WooCategoryMap
                {
                    WooCategoryName = newPC.name,
                    WooCategorySlug = newPC.slug,
                    WooCategoryParentId = (int)newPC.parent,
                    ItemCategoryLookupId = _itemCategoryLookupId,
                    WooCategoryId = (int)newPC.id,
                    CanUpdate = true
                };
                //else  was check if woomap was null
                //{
                //    _wooCategoryMap.WooCategoryName = pPC.name;
                //    _wooCategoryMap.WooCategorySlug = pPC.slug;
                //    _wooCategoryMap.WooCategoryParentId = pPC.parent;
                //    _wooCategoryMap.ItemCategoryLookupId = _itemCategoryLookupId;
                //    _wooCategoryMap.WooCategoryId = (int)pPC.id;
                //}
                if (await _wooCategoryMapRepository.AddAsync(_wooCategoryMap) == AppUnitOfWork.CONST_WASERROR)
                {
                    // did not add so set _itemCategoryLookupId to ItemCategoryLookupID to Guid.Empty = error
                    _itemCategoryLookupId = Guid.Empty;
                }
            }

            return _itemCategoryLookupId;
        }
        async Task<Guid> ImportAndMapCategoryData(ProductCategory sourcePC, List<WooItemWithParent> sourceCategoriesWithParents)
        {
            Guid _itemCategoryLookupId = Guid.Empty;
            // Get repository for each database we are accessing. ItemCategoryLookup. WooProductCategoryMap & WooSyncLog
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // Import the category and set sync data
            ///first check if it exists in the mapping, just in case there has been a name change
            WooCategoryMap _wooCategoryMap = await _wooCategoryMapRepository.FindFirstAsync(wpc => wpc.WooCategoryId == sourcePC.id);
            if (_wooCategoryMap != null)                  // the id exists so update
            {
                if (_wooCategoryMap.CanUpdate)
                  _itemCategoryLookupId = await UpdateProductCategory(sourcePC, _wooCategoryMap, sourceCategoriesWithParents);
                currImportCounters.TotalUpdated++;
            }
            else                  // the id does not exists so add
            {
                _itemCategoryLookupId = await AddProductCategory(sourcePC, sourceCategoriesWithParents);
                currImportCounters.TotalAdded++;
            }

            return _itemCategoryLookupId;
        }
        // string
        string ProductCatToString(ProductCategory sourcePC, Guid importedId)
        {
            return $"Product Category {sourcePC.name}, id: {sourcePC.id}, imported and Category Id is {importedId}";
        }
        /// <summary>
        /// Get Our Category Id using a Woo Category Id
        /// </summary>
        /// <param name="sourceWooCategoryId">The Woo Category Id</param>
        /// <returns></returns>
        async Task<Guid> GetCategoryById(int sourceWooCategoryId)
        {
            // using the category woo category mapping find the associated ID
            IAppRepository<WooCategoryMap> _wooCatrgoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            WooCategoryMap _wooCategoryMap = await _wooCatrgoryMapRepository.FindFirstAsync(wc => wc.WooCategoryId == sourceWooCategoryId);
            return (_wooCategoryMap == null) ? Guid.Empty : _wooCategoryMap.ItemCategoryLookupId;

        }
        async Task<bool> SetParentCategory(Guid sourceChildWooCategoryId, Guid sourceParentWooCategoryId)
        {
            // using the GUIDs of the category id update the parent of that record
            bool _IsUpdated = false;

            if ((sourceChildWooCategoryId != Guid.Empty) && (sourceParentWooCategoryId != Guid.Empty))
            {
                IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

                ItemCategoryLookup _itemCategoryLookup = await _itemCategoryLookupRepository.FindFirstAsync(ic => ic.ItemCategoryLookupId == sourceChildWooCategoryId);
                if (_itemCategoryLookup == null)
                    _IsUpdated = false;
                else
                {     // found so update
                    _itemCategoryLookup.ParentCategoryId = sourceParentWooCategoryId;
                    _IsUpdated = (await _itemCategoryLookupRepository.UpdateAsync(_itemCategoryLookup)) != AppUnitOfWork.CONST_WASERROR;
                }
            }
            return _IsUpdated;
        }

        async Task<bool> FindAndSetParentCategory(WooItemWithParent sourceCategoryWithParent)
        {
            ///Logic using the ids passed look for the linked attribute to the id then look for the ParentId  get the GUIDs of each and update the database
            // Get pAttributeWithAParent.ID GUID from ItemAttribute Table = ParentID
            // Get pAttributeWithAParent.AttrID GUID from ItemAttribute Table = ChildID
            // Set the  ItemAttribute.ParentID = ParentID for ItemsAttrib.ID = ChildID
            Guid _attribId = await GetCategoryById(sourceCategoryWithParent.ChildId);
            Guid _parentAttribId = await GetCategoryById(sourceCategoryWithParent.ParentId);

            bool _IsDone = await SetParentCategory(_attribId, _parentAttribId);

            await LogImport(sourceCategoryWithParent.ChildId, $"Setting of Parent of Child Category id: {sourceCategoryWithParent.ChildId} to Parent Id {sourceCategoryWithParent.ParentId} status: {_IsDone}", Models.WooSections.ProductCategories);
            return _IsDone;
        }

        // cycle through categories and add to database if they do not exists
        // Store a WooReultsDate so we can filter the results later
        // log each category and what we do with t in the log and in the WooResults
        public async Task<int> ImportCategoryData(List<ProductCategory> sourceWooProductCategories)
        {
            int _numImported = 0;
            Guid _importedId;
            //            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            List<WooItemWithParent> _categoriessWithParents = new List<WooItemWithParent>();

            //// Load the current itemCategoriers
            // cycle through categories and add to database if they do not exists
            foreach (var pc in sourceWooProductCategories)
            {
                ImportingThis = $"Importing Category ({currImportCounters.TotalImported}/{currImportCounters.MaxRecs}): {pc.name}";
                // Import the categories that have count  > 0
                if (pc.count > 0)
                {
                    // set the values as per
                    _importedId = await ImportAndMapCategoryData(pc, _categoriessWithParents);
                    _numImported++;
                    await LogImport((int)pc.id, ProductCatToString(pc, _importedId), Models.WooSections.ProductCategories);
                }
                if (_AppUnitOfWork.IsInErrorState())
                    return 0;
                currImportCounters.TotalImported++;
                currImportCounters.PercentOfRecsImported = currImportCounters.CalcPercentage(currImportCounters.TotalImported);
                StateHasChanged();
            }
            // Now we loop through all the Attributes that have parents and find them
            foreach (var AttributeWithAParent in _categoriessWithParents)
            {
                if (!await FindAndSetParentCategory(AttributeWithAParent))
                {
                    if (_AppUnitOfWork.IsInErrorState())   // was there an error that was database related?
                        return 0;
                }
            }
            return _numImported;
        }
        #endregion


    }
}
