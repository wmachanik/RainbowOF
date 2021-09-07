using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
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
    /// Objective:
    /// Create a list of item categories. From that list we need to create a list of primary categories.
    /// These are used in the items mapping.Only those that are imported can be used Mapping:
    /// 
    ///    The Table WooCategoryMaps stores the category information.
    ///    Field	                                UsedFor
    ///    WooCategoryID [int]	                    To store the CategoryID  that Woo returns
    ///    WooCategoryName [string (size:2-255)]	To store Categories.name that Woo returns
    ///    WooCategorySlug [string (size:2-255)]	To store Categories.slug that Woo returns
    ///    WooCategoryParentID	                    Can be null if not will point to a WooCategoryID
    ///    ItemCategoryLookupID	                    This links to the Category in the system. Allows us to transfer the data.
    ///    
    ///    Item categories Should be linked to tracking so items in similar categories are tracked similarly. 
    /// </summary>
    public class WooImportProductCategory : IWooImportWithParents<ItemCategoryLookup, ProductCategory, WooCategoryMap>
    {
        #region Variables
        public IAppUnitOfWork _AppUnitOfWork { get; set; }
        public ILoggerManager _Logger { get; set; }
        public WooSettings _AppWooSettings { get; set; }
        public ImportCounters CurrImportCounters { get; set; } = new();
        public List<WooItemWithParent> EntityWithParents { get; set; } = new List<WooItemWithParent>();
        #endregion
        #region Constructor
        public WooImportProductCategory(IAppUnitOfWork appUnitOfWork, ILoggerManager logger, WooSettings appWooSettings)
        {
            _AppUnitOfWork = appUnitOfWork;
            _Logger = logger;
            _AppWooSettings = appWooSettings;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Get all the categories from Woo, using the woo app settings we got from the constructor
        /// </summary>
        /// <returns>List of Product Categories from Woo</returns>
        public async Task<List<ProductCategory>> GetWooEntityDataAsync(bool InStock = false)  // dont us the instock here.
        {
            WooAPISettings _wooAPISettings = new WooAPISettings(_AppWooSettings);
            IWooProductCategory _WooProductCategory = new WooProductCategory(_wooAPISettings, _Logger);
            return await _WooProductCategory.GetAllProductCategoriesAsync();
        }
        //        public async Task<Guid> GetCategoryById(int sourceWooCategoryId)
        /// <summary>
        /// Get Our Category Id using a Woo Category Id
        /// </summary>
        /// <param name="sourceWooEntityId">The Woo Category Id</param>
        /// <returns>Id of mapped Category or guid.empty if not found</returns>

        public async Task<Guid> GetWooMappedEntityIdByIdAsync(uint sourceWooEntityId)
        {
            // using the category woo category mapping find the associated ID
            IAppRepository<WooCategoryMap> _wooCatrgoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            WooCategoryMap _wooCategoryMap = await _wooCatrgoryMapRepository.FindFirstAsync(wc => wc.WooCategoryId == sourceWooEntityId);
            return (_wooCategoryMap == null)
                ? Guid.Empty
                : _wooCategoryMap.ItemCategoryLookupId;

        }
        //        public async Task<Guid> AddOrGetIDItemCategoryLookup(ProductCategory sourcePC, List<WooItemWithParent> sourceCategoriesWithParents)
        /// <summary>
        /// See if a product/item category exist if so return that ID or add it and return it.
        /// </summary>
        /// <param name="sourceEntity"></param>
        /// <returns>Guid based Id from the systems lookup table that matches found or created Category, Guid.Empty for error</returns>
        public async Task<Guid> AddOrGetEntityIDAsync(ProductCategory sourceEntity)
        {
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

            ItemCategoryLookup _itemCategoryLookup = await _itemCategoryLookupRepository.FindFirstAsync(ic => ic.CategoryName == sourceEntity.name);
            if (_itemCategoryLookup == null)
            {
                ItemCategoryLookup _newItemCategoryLookup = new ItemCategoryLookup
                {
                    CategoryName = sourceEntity.name,
                    //  Null;? ParentCategoryId = Guid.Empty,
                    UsedForPrediction = (sourceEntity.parent == null) || (sourceEntity.parent == 0),
                    Notes = $"Imported Woo Category ID {sourceEntity.id}"
                };
                if (sourceEntity.parent > 0)
                {
                    EntityWithParents.Add(new WooItemWithParent
                    {
                        ChildId = (uint)sourceEntity.id,
                        ParentId = (uint)sourceEntity.parent
                    });
                }
                if (await _itemCategoryLookupRepository.AddAsync(_newItemCategoryLookup) != AppUnitOfWork.CONST_WASERROR)
                    return _newItemCategoryLookup.ItemCategoryLookupId;
            }
            else
                return _itemCategoryLookup.ItemCategoryLookupId;

            // if there was any issue return failure
            return Guid.Empty;
        }
        /// <summary>
        /// With an id that exists in the mapping update or add the category
        /// </summary>
        /// <param name="sourceEntity">the Woo Product Category source</param>
        /// <param name="sourceWooMappedEntityId">the mapped Categories id, as per the mapped table</param>
        /// <returns></returns>
        public async Task<Guid> AddOrUpdateEntityAsync(ProductCategory sourceEntity, Guid sourceWooMappedEntityId)
        {
            Guid _itemCategoryLookupId = Guid.Empty;
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            // check if the category exists
            ItemCategoryLookup _itemCategoryLookup = await _itemCategoryLookupRepository.FindFirstAsync(ic => ic.ItemCategoryLookupId == sourceWooMappedEntityId);
            if (_itemCategoryLookup != null)
            {
                _itemCategoryLookupId = await UpdateEntityAsync(sourceEntity, _itemCategoryLookup); //--, sourceCategoriesWithParents);
            }
            else
            {
                // should we delete the mapping here if it exists?
                _itemCategoryLookupId = await AddOrGetEntityIDAsync(sourceEntity); //--, sourceCategoriesWithParents);
            }
            return _itemCategoryLookupId;
        }
        /// <summary>
        /// Add a new Product Category if it does not already exists.
        /// </summary>
        /// <param name="newWooEntity"></param>
        /// <returns></returns>
        public async Task<Guid> AddEntityAsync(ProductCategory newWooEntity)
        {
            Guid _itemCategoryLookupId = Guid.Empty;
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // Add Item Category if it does not exist
            _itemCategoryLookupId = await AddOrGetEntityIDAsync(newWooEntity); //--  , newCategoriesWithParents);
            if (_itemCategoryLookupId != Guid.Empty)
            {
                //--> should we use AutoMapper?
                WooCategoryMap _wooCategoryMap = new WooCategoryMap
                {
                    WooCategoryName = newWooEntity.name,
                    WooCategorySlug = newWooEntity.slug,
                    WooCategoryParentId = (uint)newWooEntity.parent,
                    ItemCategoryLookupId = _itemCategoryLookupId,
                    WooCategoryId = (uint)newWooEntity.id,
                    CanUpdate = true
                };
                if (await _wooCategoryMapRepository.AddAsync(_wooCategoryMap) == AppUnitOfWork.CONST_WASERROR)
                {
                    // did not add so set _itemCategoryLookupId to ItemCategoryLookupID to Guid.Empty = error
                    _itemCategoryLookupId = Guid.Empty;
                }
            }
            return _itemCategoryLookupId;
        }
        /// <summary>
        /// For a Category that has a parent find its parent and set the parent and child id in the table
        /// Logic:
        ///   Get pAttributeWithAParent.ID GUID from ItemAttribute Table = ParentID
        ///   Get pAttributeWithAParent.AttrID GUID from ItemAttribute Table = ChildID
        ///   Set the  ItemAttribute.ParentID = ParentID for ItemsAttrib.ID = ChildID
        /// </summary>
        /// <param name="sourceWooEntityWithParent">source Woo Item that has a parent</param>
        /// <returns></returns>
        public async Task<bool> FindAndSetParentEntityAsync(WooItemWithParent sourceWooEntityWithParent)
        {
            Guid _attribId = await GetWooMappedEntityIdByIdAsync(sourceWooEntityWithParent.ChildId);
            Guid _parentAttribId = await GetWooMappedEntityIdByIdAsync(sourceWooEntityWithParent.ParentId);
            bool _IsDone = await SetWooEntityParentAsync(_attribId, _parentAttribId);
            //await LogImport(sourceWooEntityWithParent.ChildId, $"Setting of Parent of Child Category id: {sourceWooEntityWithParent.ChildId} to Parent Id {sourceWooEntityWithParent.ParentId} status: {_IsDone}", Models.WooSections.ProductCategories);
            return _IsDone;
        }
        /// <summary>
        /// Import the Product Category from Woo, into the Item Category Lookup making sure to map it. 
        ///     If it exists update only allowed, otherwise add it.
        /// </summary>
        /// <param name="sourceEntity">source Product Category</param>
        /// <returns></returns>
        public async Task<Guid> ImportAndMapWooEntityDataAsync(ProductCategory sourceEntity)
        {
            Guid _itemCategoryLookupId = Guid.Empty;
            // Get repository for each database we are accessing. ItemCategoryLookup. WooProductCategoryMap & WooSyncLog
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();
            // Import the category and set sync data
            ///first check if it exists in the mapping, just in case there has been a name change
            WooCategoryMap _wooCategoryMap = await _wooCategoryMapRepository.FindFirstAsync(wpc => wpc.WooCategoryId == sourceEntity.id);
            if (!_AppUnitOfWork.IsInErrorState())
            {
                if (_wooCategoryMap != null)                  // the id exists so update
                {
                    if (_wooCategoryMap.CanUpdate)
                        _itemCategoryLookupId = await UpdateWooMappingEntityAsync(sourceEntity, _wooCategoryMap);//-, sourceCategoriesWithParents);
                    CurrImportCounters.TotalUpdated++;
                }
                else                  // the id does not exists so add
                {
                    _itemCategoryLookupId = await AddEntityAsync(sourceEntity); //--, sourceCategoriesWithParents);
                    CurrImportCounters.TotalAdded++;
                }
            }
            return _itemCategoryLookupId;
        }
        /// <summary>
        /// Using the id of the Parent for the child category set the parent category
        /// </summary>
        /// <param name="sourceChildWooEntityId">source child Woo CategoryId</param>
        /// <param name="sourceParentWooEntityId">source Parent woo CategoryId</param>
        /// <returns>success or failure</returns>
        public async Task<bool> SetWooEntityParentAsync(Guid sourceChildWooCategoryId, Guid sourceParentWooCategoryId)
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
        /// <summary>
        /// Update the woo product/item category lookup table with the current woo Product Category Data
        /// </summary>
        /// <param name="updatedWooEntity">updated Woo Product Category</param>
        /// <param name="updatedEntity">Item Category Lookup to be updated</param>
        /// <returns></returns>
        public async Task<Guid> UpdateEntityAsync(ProductCategory updatedWooEntity, ItemCategoryLookup updatedEntity)
        {
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            updatedEntity.CategoryName = updatedWooEntity.name;
            //-> don't update this, updatedEntity.UsedForPrediction = (updatedWooEntity.parent == null) || (updatedWooEntity.parent == 0);
            // do not set parent Id here since it could cause database problems - if it is already null then it will be updated later.
            // pItemCategoryLookup.ParentCategoryId = Guid.Empty;   /// need to find the  ParentId if it exists - or need to say it does not exists so that we can look later?
            if (updatedWooEntity.parent > 0)
            {
                EntityWithParents.Add(new WooItemWithParent
                {
                    ChildId = (uint)updatedWooEntity.id,
                    ParentId = (uint)updatedWooEntity.parent
                });
            }
            updatedEntity.Notes = $"Updated Woo Category ID {updatedWooEntity.id}";
            bool _success = await _itemCategoryLookupRepository.UpdateAsync(updatedEntity) != AppUnitOfWork.CONST_WASERROR;
            return (_success) ? updatedEntity.ItemCategoryLookupId : Guid.Empty;
        }
        /// <summary>
        /// Update the woo product/item category lookup table with the current woo Product Category Data
        /// </summary>
        /// <param name="updatedEntity">Woo Product Category</param>
        /// <param name="targetWooMap">target Woo mapped Category</param>
        /// <returns>ID of item mapped</returns>
        public async Task<Guid> UpdateWooMappingEntityAsync(ProductCategory updatedEntity, WooCategoryMap targetWooMap)
        {
            Guid _itemCategoryLookupId = Guid.Empty;
            IAppRepository<WooCategoryMap> _wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();
            // copy data across
            targetWooMap.WooCategoryName = updatedEntity.name;
            targetWooMap.WooCategorySlug = updatedEntity.slug;
            targetWooMap.WooCategoryParentId = (uint)updatedEntity.parent;
            _itemCategoryLookupId = await AddOrUpdateEntityAsync(updatedEntity, targetWooMap.ItemCategoryLookupId);  //--, sourceCategoriesWithParents);
            if (_itemCategoryLookupId != Guid.Empty)
            {
                /// Now update the woo categorY using the _itemCategoryLookupId returned.
                if (await _wooCategoryMapRepository.UpdateAsync(targetWooMap) == AppUnitOfWork.CONST_WASERROR)
                {
                    // did not updated so set _itemCategoryLookupId to ItemCategoryLookupID to Guid.Empty = error
                    _itemCategoryLookupId = Guid.Empty;
                }
            }
            return _itemCategoryLookupId;
        }
        #endregion


    }
}
