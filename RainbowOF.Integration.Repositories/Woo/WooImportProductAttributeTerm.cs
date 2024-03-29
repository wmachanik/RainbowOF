﻿using RainbowOF.FrontEnd.Models.Classes;
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
    public class WooImportProductAttributeTerm : IWooImportWithAParent<ItemAttributeVarietyLookup, ProductAttributeTerm, WooProductAttributeTermMap>
    {
        #region public variables
        public IAppUnitOfWork _AppUnitOfWork { get; set; }
        public ILoggerManager _Logger { get; set; }
        public WooSettings _AppWooSettings { get; set; }
        public ImportCounters CurrImportCounters { get; set; } = new ImportCounters();
        #endregion
        #region Constructor
        public WooImportProductAttributeTerm(IAppUnitOfWork appUnitOfWork, ILoggerManager logger, WooSettings appWooSettings)
        {
            _AppUnitOfWork = appUnitOfWork ?? throw new ArgumentNullException(nameof(appUnitOfWork));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _AppWooSettings = appWooSettings ?? throw new ArgumentNullException(nameof(appWooSettings));
        }
        #endregion
        #region Interface Methods
        //        async Task<Guid> AddOrGetIDItemAttributeVariety(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId)
        /// <summary>
        /// Return the Id of a an added or found Attribute Term with details that we have of this variant
        /// Logic: Check if the Item variant of the same name or SKU exists in the Item database, if so update check if it has the 
        ///         same parent and return, otherwise add and return it.
        /// </summary>
        /// <param name="sourceEntity">Woo Product Attribute Term information to use</param>
        /// <param name="sourceParentId">The Id of our Parent Item</param>
        /// <returns>Guid of Item Added or found or Guid.Empty if not</returns>
        public async Task<Guid> AddOrGetEntityIDAsync(ProductAttributeTerm sourceEntity, Guid sourceParentId)
        {
            IAppRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();

            ItemAttributeVarietyLookup _ItemAttributeVariety = await _itemAttributeVarietyRepository.FindFirstByAsync(ic => ic.VarietyName == sourceEntity.name);
            if (_ItemAttributeVariety == null)
            {
                ItemAttributeVarietyLookup _newItemAttributeVariety = new ItemAttributeVarietyLookup
                {
                    ItemAttributeLookupId = sourceParentId,
                    VarietyName = sourceEntity.name,
                    Notes = $"Imported Woo Attribute Term ID {sourceEntity.id}"
                };

                var _recsAdded = await _itemAttributeVarietyRepository.AddAsync(_newItemAttributeVariety);
                return (_recsAdded != null) ? _newItemAttributeVariety.ItemAttributeVarietyLookupId : Guid.Empty;
            }
            else
            {
                return _ItemAttributeVariety.ItemAttributeVarietyLookupId;   // we found one with the same name so assume this is the correct one.
            }
        }
        public async Task<Guid> AddOrUpdateEntityAsync(ProductAttributeTerm sourceEntity, WooProductAttributeTermMap sourceWooMappedEntity, Guid sourceParentId)
        {
            Guid _itemAttributeVarietyId = Guid.Empty;
            IAppRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            // check if the AttributeTerm exists
            ItemAttributeVarietyLookup _ItemAttributeVariety = await _itemAttributeVarietyRepository.FindFirstByAsync(ic => ic.ItemAttributeVarietyLookupId == sourceWooMappedEntity.ItemAttributeVarietyLookupId);
            _itemAttributeVarietyId = (_ItemAttributeVariety != null)
                                        ? await UpdateEntityAsync(sourceEntity, sourceParentId, _ItemAttributeVariety)
                                        : await AddEntityAsync(sourceEntity, sourceWooMappedEntity, sourceParentId);
            return _itemAttributeVarietyId;
        }
        // async Task<Guid> AddProductAttributeTerm(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, WooProductAttributeTermMap sourceWooAttributeTermMap)
        /// <summary>
        /// Add new Entity to Map to Attribute Terms / variations if it does not exist otherwise update it
        /// </summary>
        /// <param name="newWooEntity">new ProductAttributeTerm</param>
        /// <param name="sourceWooMap">Source Woo Attribute Term</param>
        /// <param name="sourceParentId">Source Attribute Term ParentId</param>
        /// <returns></returns>
        public async Task<Guid> AddEntityAsync(ProductAttributeTerm newWooEntity, WooProductAttributeTermMap sourceWooMap, Guid sourceParentId)
        {
            Guid _itemAttributeTermId = Guid.Empty;
            IAppRepository<WooProductAttributeTermMap> _wooAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();
            // Add Item AttributeTerm if it does not exist
            _itemAttributeTermId = await AddOrGetEntityIDAsync(newWooEntity, sourceParentId);
            if (sourceWooMap == null)
            {
                sourceWooMap = new WooProductAttributeTermMap
                {
                    WooProductAttributeTermId = (int)newWooEntity.id,
                    ItemAttributeVarietyLookupId = _itemAttributeTermId
                };
            }
            else
            {
                sourceWooMap.WooProductAttributeTermId = (int)newWooEntity.id;
                sourceWooMap.ItemAttributeVarietyLookupId = _itemAttributeTermId;
            }
            if (await _wooAttributeTermMapRepository.AddAsync(sourceWooMap) == null)
            {
                // did not add so set _ItemAttributeTermId to ItemAttributeTermID to Guid.Empty = error
                _itemAttributeTermId = Guid.Empty;
            }
            return _itemAttributeTermId;
        }

        public Task<bool> FindAndSetParentEntityAsync(WooItemWithParent sourceWooEntityWithParent)
        {
            throw new NotImplementedException();
        }
        // Retrieve data from Woo
        // async Task<List<ProductAttributeTerm>> GetWooAttributeTermData(ProductAttribute currProductAttribute)

        public async Task<List<ProductAttributeTerm>> GetWooEntityDataAsync(uint parentAttributeId)
        {
            WooAPISettings _wooAPISettings = new WooAPISettings(_AppWooSettings);
            IWooProductAttributeTerm _WooProductAttributeTerm = new WooProductAttributeTerm(_wooAPISettings, _Logger);
            List<ProductAttributeTerm> wooProductAttributeTerms = await _WooProductAttributeTerm.GetAttributeTermsByAtttributeAsync(parentAttributeId);
            return wooProductAttributeTerms;
        }
        //         private Guid GetVarietysParentAttributeID(uint? sourceParentId)
        /// <summary>
        /// Get the Variety's Parent id using the WooMapping, if is not found then return Empty
        /// </summary>
        /// <param name="sourceWooEntityId">source's Id</param>
        /// <returns>UId of the Entity</returns>
        public async Task<Guid> GetWooMappedEntityIdByIdAsync(uint sourceWooEntityId)
        {
            IAppRepository<WooProductAttributeMap> _wooProductAttributeMapRepo = _AppUnitOfWork.Repository<WooProductAttributeMap>();
            /// was not an async made it one
            WooProductAttributeMap _wooProductAttributeMap = await _wooProductAttributeMapRepo.FindFirstByAsync(wpa => wpa.WooProductAttributeId == sourceWooEntityId);
            return (_wooProductAttributeMap == null) ? Guid.Empty : _wooProductAttributeMap.ItemAttributeLookupId;
        }
        public Task<bool> SetWooEntityParent(Guid sourceChildWooEntityId, Guid sourceParentWooEntityId)
        {
            throw new NotImplementedException();
        }
        // async Task<Guid> UpdateItemAttributeVariety(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, ItemAttributeVarietyLookup currItemAttributeVariety)
        /// <summary>
        /// Update ProductAttributeTerm in the Lookup
        /// </summary>
        /// <param name="updateWooEntity">The WooEntity that is the source</param>
        /// <param name="sourceParentAttributeId"></param>
        /// <param name="updatedEntity"></param>
        /// <returns></returns>
        public async Task<Guid> UpdateEntityAsync(ProductAttributeTerm updateWooEntity, Guid sourceParentId, ItemAttributeVarietyLookup updatedEntity)
        {
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            bool _success = false;
            updatedEntity.ItemAttributeLookupId = sourceParentId;
            updatedEntity.VarietyName = updateWooEntity.name;
            updatedEntity.Notes = $"Updated Woo AttributeTerm ID {updateWooEntity.id}";
            _success = await _ItemAttributeVarietyRepository.UpdateAsync(updatedEntity) != AppUnitOfWork.CONST_WASERROR;
            return (_success ? updatedEntity.ItemAttributeVarietyLookupId : Guid.Empty);
        }
        //        async Task<Guid> UpdateProductAttributeTerm(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, WooProductAttributeTermMap sourceWooAttributeTermMap)
        /// <summary>
        /// Update the Attribute Term mapping we assume is found, using the data we got from Woo and the Product Attribute Term Mapping.
        /// Logic: Update the Mapping Using the current mapping we have.
        /// </summary>
        /// <param name="sourceWooEntity">The Woo Attribute Term that we are working on.</param>
        /// <param name="targetWooMap">The target product attribute term to item mapping.</param>
        /// <param name="sourceParentId">Parent Id of the Item's Attribute Terms.</param>
        /// <returns>The id of the item variant updated or updated.</returns>
        public async Task<Guid> UpdateWooMappingEntityAsync(ProductAttributeTerm updatedWooEntity, WooProductAttributeTermMap targetWooMap, Guid sourceParentId)
        {
            // we have found a mapping between the woo Product AttributeTerm and our AttributeTerm id so update the Attribute table just in case.
            Guid _itemAttributeTermId = Guid.Empty;
            IAppRepository<WooProductAttributeTermMap> _wooProductAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();
            _itemAttributeTermId = await AddOrUpdateEntityAsync(updatedWooEntity, targetWooMap, sourceParentId);
            /// Now update the woo AttributeTerm using the _ItemAttributeTermId returned.
            if (await _wooProductAttributeTermMapRepository.UpdateAsync(targetWooMap) == AppUnitOfWork.CONST_WASERROR)
            {   // did not updated so set _ItemAttributeTermId to ItemAttributeTermID to Guid.Empty = error
                _itemAttributeTermId = Guid.Empty;
            }
            return _itemAttributeTermId;
        }
        //  async Task<Guid> ImportAndMapAttributeTermData(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId)
        /// <summary>
        /// Import and Map Woo entity / Attribute Terms
        /// </summary>
        /// <param name="sourceEntity">Source Woo Attribute Term </param>
        /// <param name="sourceParentId">Id of the Attribute Term's parent</param>
        /// <returns></returns>
        public async Task<Guid> ImportAndMapWooEntityDataAsync(ProductAttributeTerm sourceEntity, Guid sourceParentId)
        {
            Guid _itemAttributeTermId = Guid.Empty;
            // Get repository for each database we are accessing. ItemAttributeTerm. WooProductAttributeTermMap & WooSyncLog
            IAppRepository<WooProductAttributeTermMap> _wooAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();
            // Import the AttributeTerm and set sync data
            ///first check if it exists in the mapping, just in case there has been a name change
            WooProductAttributeTermMap _wooAttributeTermMap = await _wooAttributeTermMapRepository.FindFirstByAsync(wa => wa.WooProductAttributeTermId == sourceEntity.id);
            if (_wooAttributeTermMap != null)   // the id exists so update
            {
                _itemAttributeTermId = await UpdateWooMappingEntityAsync(sourceEntity, _wooAttributeTermMap, sourceParentId);
                CurrImportCounters.TotalUpdated++;
            }
            else      // the id does not exists so add
            {
                _itemAttributeTermId = await AddEntityAsync(sourceEntity, _wooAttributeTermMap, sourceParentId);
                CurrImportCounters.TotalAdded++;
            }
            return _itemAttributeTermId;
        }
        #endregion
    }
}
