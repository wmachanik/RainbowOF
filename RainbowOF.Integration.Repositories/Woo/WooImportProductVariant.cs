using AutoMapper;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.Items;
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
    public class WooImportVariation : IWooImportWithAParent<ItemVariant, Variation, WooProductVariantMap>
    {
        #region Local Variables
        private IMapper _Mapper { get; set; }
        #endregion
        #region Public Variables
        public IAppUnitOfWork _AppUnitOfWork { get; set; }
        public ILoggerManager _Logger { get; set; }
        public WooSettings _AppWooSettings { get; set; }
        public ImportCounters CurrImportCounters { get; set; } = new ImportCounters();
        #endregion
        #region Constructor
        public WooImportVariation(IAppUnitOfWork appUnitOfWork, ILoggerManager logger, WooSettings appWooSettings, IMapper mapper)
        {
            _AppUnitOfWork = appUnitOfWork;
            _Logger = logger;
            _AppWooSettings = appWooSettings;
            _Mapper = mapper;
            _Logger.LogDebug("WooImportVariation initialised.");
        }
        #endregion
        #region Private Variables
        protected WooImportProduct _WooImportProduct { get; set; } = null;
        private WooImportProduct LocalWooImportProduct
        {
            get
            {
                if (_WooImportProduct == null)
                {
                    _WooImportProduct = new();
                }
                return _WooImportProduct;
            }
            set { _WooImportProduct = value; }
        }
        #endregion
        #region Support Methods
        /// <summary>
        /// Map the Woo Product Variant to our Item Variant variable
        /// </summary>
        /// <param name="sourceWooVariant"></param>
        /// <param name="currItemVariant"></param>
        /// <returns></returns>
        public async Task<ItemVariant> MapWooProductVariantInfo(Variation sourceWooVariant, ItemVariant currItemVariant)
        {
            if (currItemVariant == null)
                currItemVariant = new();

            if (sourceWooVariant.description == String.Empty)
                sourceWooVariant.description = sourceWooVariant.attributes[0].option;
            if (sourceWooVariant.sku == String.Empty)
                sourceWooVariant.sku = sourceWooVariant.attributes[0].option;

            _Mapper.Map(sourceWooVariant, currItemVariant);
            //--> now we have mapped the main stuff we nee d to map the attributes
            currItemVariant = await MapWooProductVariantAttributes(sourceWooVariant, currItemVariant);
            return currItemVariant;
        }
        /// <summary>
        /// Using the source woo Variant map the attributes and attribute options to the Item variant
        /// Logic: for each source variant option find the attribute in the lookup table and map it top the ItemVariant associated attribute, 
        ///     and then also using the string description the associated term to that attribute.
        /// </summary>
        /// <param name="sourceWooVariant">The source woo product</param>
        /// <param name="currItemVariant">The Item Variant with everything else mapped.</param>
        /// <returns></returns>
        private async Task<ItemVariant> MapWooProductVariantAttributes(Variation sourceWooVariant, ItemVariant currItemVariant)
        {

            //if (currItemVariant.ItemVariantName.Contains("250"))
            //    currItemVariant.IsEnabled = true;

            IAppRepository<WooProductAttributeMap> _wooProductAttributeMap = _AppUnitOfWork.Repository<WooProductAttributeMap>();
            IAppRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyLookup = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            if (sourceWooVariant.attributes.Count > 0)
            {
                foreach (var _wooVariant in sourceWooVariant.attributes)  // loop through all variants and add
                {
                    {
                        var _productAttribute = await _wooProductAttributeMap.FindFirstByAsync(wpa => wpa.WooProductAttributeId == _wooVariant.id);
                        if (_productAttribute != null)  // should never be null if so there is an issue - not sure what to do with this at the moment
                        {
                            currItemVariant.AssocatedAttributeLookupId = _productAttribute.ItemAttributeLookupId;
                            var _productAttributeVarietty = await _itemAttributeVarietyLookup
                                .FindFirstByAsync(iavl =>
                                                            (iavl.ItemAttributeLookupId == _productAttribute.ItemAttributeLookupId)
                                                         && (iavl.VarietyName == _wooVariant.option)
                                                  );
                            if (_productAttributeVarietty != null)  // should never be null if so there is an issue - not sure what to do with this at the moment 
                            {
                                currItemVariant.AssociatedAttributeVarietyLookupId = _productAttributeVarietty.ItemAttributeVarietyLookupId;
                            }
                        }
                    }
                }
            }
            return currItemVariant;
        }
        #endregion
        #region Interface Methods
        /// <summary>
        /// Uses the Woo Parent Id to request all the variants associated to that Product
        /// Logic:  Do the Woo REST call to retrieve the variants for this parent product.
        ///         If nothing returned, return null.
        /// </summary>
        /// <param name="parentAttributeId">The id of the parent product.</param>
        /// <returns>List<Variation> of Product Variants</returns>
        public async Task<List<Variation>> GetWooEntityDataAsync(uint parentAttributeId)
        {
            WooAPISettings _wooAPISettings = new WooAPISettings(_AppWooSettings);
            WooProductVariation _wooVariant = new(_wooAPISettings, _Logger);
            List<Variation> wooVariants = await _wooVariant.GetProductVariationsByProductIdAsync(parentAttributeId);
            return wooVariants;
        }
        /// <summary>
        /// Looks to see if the current Woo Variant is Mapped using Variant Id.
        /// Logic: Find the first instance of the of the Variant id in the mapping, if it exists return it otherwise return Guid.Empty.
        /// </summary>
        /// <param name="sourceWooEntityId">The id of the product variant.  </param>
        /// <returns>Guid of Item found in mapping or Guid.Empty if not.</returns>
        public async Task<Guid> GetWooMappedEntityIdByIdAsync(uint sourceWooEntityId)
        {
            IAppRepository<WooProductVariantMap> _wooProductVariantMapRepo = _AppUnitOfWork.Repository<WooProductVariantMap>();
            /// was not an async made it one
            WooProductVariantMap _wooProductVariantMap = await _wooProductVariantMapRepo.FindFirstByAsync(wpa => wpa.WooProductVariantId == sourceWooEntityId);
            return (_wooProductVariantMap == null) ? Guid.Empty : _wooProductVariantMap.ItemVariantId;
        }
        /// <summary>
        /// Using the Woo Product Variant as a source add Item to our tables.
        /// Logic:  To get here we assume the variant does not exist so we need to create a new Variant map the data, 
        ///         and save the variant. Return the Id of the variant added, or Guid.Empty if not.
        /// </summary>
        /// <param name="newWooEntity">New Woo Product Variant information to use.</param>
        /// <param name="sourceWooMap">The source Woo Product Variant Map.</param>
        /// <param name="sourceParentId">The Id of the Parent Item for this Variant.</param>
        /// <returns>Guid of Item Added.</returns>
        public async Task<Guid> AddEntityAsync(Variation newWooEntity, WooProductVariantMap sourceWooMap, Guid sourceParentId)
        {
            Guid _itemVariantId = Guid.Empty;
            IAppRepository<WooProductVariantMap> _wooVariantMapRepository = _AppUnitOfWork.Repository<WooProductVariantMap>();
            // Add Item Variant if it does not exist otherwise update
            _itemVariantId = await AddOrGetEntityIDAsync(newWooEntity, sourceParentId);
            if (_itemVariantId == Guid.Empty)  // error adding
                return _itemVariantId;         // do not add variant map as there is an issue.
            if (sourceWooMap == null)
            {
                sourceWooMap = new WooProductVariantMap
                {
                    WooProductVariantId = (int)newWooEntity.id,
                    ItemVariantId = _itemVariantId
                };
                if (await _wooVariantMapRepository.AddAsync(sourceWooMap) == null)
                {
                    // did not add so set _ItemVariantId to ItemVariantID to Guid.Empty = error
                    _itemVariantId = Guid.Empty;
                }
            }
            else
            {
                sourceWooMap.WooProductVariantId = (int)newWooEntity.id;
                sourceWooMap.ItemVariantId = _itemVariantId;
                await _wooVariantMapRepository.UpdateAsync(sourceWooMap);  // no error checking since update is just in case, could probably skip.
            }
            return _itemVariantId;
        }
        /// <summary>
        /// Return the Id of a an added or found Variant with details that we have of this variant
        /// Logic: Check if the Item variant of the same name or SKU exists in the Item database, if so update check if it has the 
        ///         same parent and return, otherwise add and return it.
        /// </summary>
        /// <param name="sourceEntity">Woo Product Variant information to use</param>
        /// <param name="sourceParentId">The Id of our Parent Item</param>
        /// <returns>Guid of Item Added or found or Guid.Empty if not</returns>
        public async Task<Guid> AddOrGetEntityIDAsync(Variation sourceEntity, Guid sourceParentId)
        {
            IAppRepository<ItemVariant> _itemVariantRepository = _AppUnitOfWork.Repository<ItemVariant>();
            ItemVariant _itemVariant = null;
            if (sourceEntity.description != string.Empty)
                _itemVariant = await _itemVariantRepository.FindFirstByAsync(ic => (ic.ItemId == sourceParentId) && (ic.ItemVariantName == sourceEntity.description));
            if (_itemVariant == null)
            {
                ItemVariant _newItemVariant = await MapWooProductVariantInfo(sourceEntity, null);
                _newItemVariant.ItemId = sourceParentId;  // the mapping copies the variant info but does not copy the Parent Item Id, as it does not exist in the source Entity
                var _recsAdded = await _itemVariantRepository.AddAsync(_newItemVariant);
                return (_recsAdded != null) ? _newItemVariant.ItemVariantId : Guid.Empty;
            }
            else
            {
                return _itemVariant.ItemVariantId;   // we found one with the same name so assume this is the correct one.
            }
        }
        /// <summary>
        /// Add or update (if it exists) an item variant
        /// Logic:  See if an Item variant of this Id exists in the item variant table.
        ///         If so, update it otherwise added it, returning the Id.
        /// </summary>
        /// <param name="sourceEntity">Source Product Variant information to use.</param>
        /// <param name="sourceWooMappedEntityId">Id of the Variant we found, is it still there?</param>
        /// <param name="sourceParentId">The Id of the Parent Item for this Variant.</param>
        /// <returns>The Id of the Parent Item for this Variant.</returns>
        public async Task<Guid> AddOrUpdateEntityAsync(Variation sourceEntity, WooProductVariantMap sourceWooMappedEntity, Guid sourceParentId)
        {
            Guid _itemVariantId = Guid.Empty;
            IAppRepository<ItemVariant> _ItemVariantRepository = _AppUnitOfWork.Repository<ItemVariant>();
            // check if the AttributeTerm exists
            ItemVariant _itemVariant = await _ItemVariantRepository.FindFirstByAsync(ic => ic.ItemVariantId == sourceWooMappedEntity.ItemVariantId);
            if (_itemVariant != null)
            {
                // make sure the mapped variant is pointing to the correct parent if not delete the mapping and then add.
                if (_itemVariant.ItemId == sourceParentId)
                {
                    _itemVariantId = await UpdateEntityAsync(sourceEntity, sourceParentId, _itemVariant);
                }
                else
                {
                    IAppRepository<WooProductVariantMap> _wooProductVariantMapRepository = _AppUnitOfWork.Repository<WooProductVariantMap>();
                    await _wooProductVariantMapRepository.DeleteByAsync(wpv => wpv.ItemVariantId == sourceWooMappedEntity.ItemVariantId);
                    _itemVariantId = await AddEntityAsync(sourceEntity, sourceWooMappedEntity, sourceParentId);
                }
            }
            else
            {
                _itemVariantId = await AddEntityAsync(sourceEntity, sourceWooMappedEntity, sourceParentId);
            }
            return _itemVariantId;
        }
        /// <summary>
        /// Find child and parent id of product variant and set them for item variant.
        /// Logic:  With a record of the Child and Parent of a Product variant, find the equivalent in our 
        ///         database and make sure they reference one another.
        /// </summary>
        /// <param name="sourceWooEntityWithParent">Parent and Child Id of the Woo Variant</param>
        /// <returns>success or failure</returns>
        public Task<bool> FindAndSetParentEntityAsync(WooItemWithParent sourceWooEntityWithParent)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Update the Item Variant we assume is found, using the data we got from Woo on the Product Variant
        /// Logic: Copy data across and update the Item Variant Table.
        /// </summary>
        /// <param name="updatedWooEntity">The Woo Variant that we are working on.</param>
        /// <param name="sourceParentId">The Id of the Item that this is the variant of.</param>
        /// <param name="updatedEntity">The Item Variant that exists in the current tables</param>
        /// <returns>The Guid of the item variant updated.</returns>
        public async Task<Guid> UpdateEntityAsync(Variation updatedWooEntity, Guid sourceParentId, ItemVariant updatedEntity)
        {
            IAppRepository<ItemVariant> _itemVariantRepository = _AppUnitOfWork.Repository<ItemVariant>();
            bool _success = false;
            updatedEntity = await MapWooProductVariantInfo(updatedWooEntity, updatedEntity);
            _success = await _itemVariantRepository.UpdateAsync(updatedEntity) != AppUnitOfWork.CONST_WASERROR;
            return (_success ? updatedEntity.ItemVariantId : Guid.Empty);
        }
        /// <summary>
        /// Update the Item Variant mapping we assume is found, using the data we got from Woo and the Product Variant Mapping.
        /// Logic: Update the Mapping Using the current mapping we have.
        /// </summary>
        /// <param name="sourceWooEntity">The Woo Variant that we are working on.</param>
        /// <param name="targetWooMap">The target product variant to item mapping.</param>
        /// <param name="sourceParentId">Parent Id of the Item's Variant.</param>
        /// <returns>The id of the item variant updated or updated.</returns>
        public async Task<Guid> UpdateWooMappingEntityAsync(Variation updatedWooEntity, WooProductVariantMap targetWooMap, Guid sourceParentId)
        {
            // we have found a mapping between the woo Product AttributeTerm and our AttributeTerm id so update the Attribute table just in case.
            Guid _ItemVariantId = Guid.Empty;
            IAppRepository<WooProductVariantMap> _wooProductVariantMapRepository = _AppUnitOfWork.Repository<WooProductVariantMap>();
            _ItemVariantId = await AddOrUpdateEntityAsync(updatedWooEntity, targetWooMap, sourceParentId);
            if (_ItemVariantId == Guid.Empty)
                return Guid.Empty;  // error so exit;
            /// Now update the woo ItemVariant using the _ItemVariantId returned.
            if (await _wooProductVariantMapRepository.UpdateAsync(targetWooMap) == AppUnitOfWork.CONST_WASERROR)
            {   // did not updated so set _ItemVariantId to ItemVariantID to Guid.Empty = error
                _ItemVariantId = Guid.Empty;
            }
            return _ItemVariantId;
        }
        /// <summary>
        /// Import the Woo Product Variant that is passed.
        /// Logic:  Check if the product variant does not exists add and copy woo details to new record increase Counter.added. 
        ///         If product does exist and CanUpdate is true then update by copying the Woo Details that are not keys, increase Counter.update.
        /// </summary>
        /// <param name="sourceEntity">Source Woo Product Variant.</param>
        /// <param name="sourceParentId">Parent Id of the Items Variant.</param>
        /// <returns>Guid Item Id Updated</returns>
        public async Task<Guid> ImportAndMapWooEntityDataAsync(Variation sourceEntity, Guid sourceParentId)
        {
            Guid _itemItemVariantId = Guid.Empty;
            // Get repository for each database we are accessing. ItemItemVariant. WooProductItemVariantMap & WooSyncLog
            IAppRepository<WooProductVariantMap> _wooProductariantMapRepository = _AppUnitOfWork.Repository<WooProductVariantMap>();
            // Import the ItemVariant and set sync data
            ///first check if it exists in the mapping, just in case there has been a name change
            WooProductVariantMap _wooItemVariantMap = await _wooProductariantMapRepository.FindFirstByAsync(wa => wa.WooProductVariantId == sourceEntity.id);
            if (_wooItemVariantMap != null)   // the id exists so update
            {
                _itemItemVariantId = await UpdateWooMappingEntityAsync(sourceEntity, _wooItemVariantMap, sourceParentId);
                CurrImportCounters.TotalUpdated++;
            }
            else      // the id does not exists so add
            {
                _itemItemVariantId = await AddEntityAsync(sourceEntity, _wooItemVariantMap, sourceParentId);
                CurrImportCounters.TotalAdded++;
            }
            return _itemItemVariantId;
        }
        /// <summary>
        /// Import all the variant for the Item passed in. Can be used on the fly or in a full import.
        /// Logic: Get all the variants for this product.Loop through the variants and add.If variant exists and CanUpdate is true 
        /// then update by copying the Woo Details that are not keys, otherwise add.
        /// </summary>
        /// <param name="sourceProductParentId">Id of the Woo Product to import</param>
        /// <param name="sourceItemParentId">Id of Parent Item</param>
        /// <returns>number of variants imported or error</returns>
        public async Task<ImportCounters> ImportProductVariants(uint sourceProductParentId, Guid sourceItemParentId)
        {
            CurrImportCounters.Reset();   // start with a new counter
            List<Variation> _variations = await GetWooEntityDataAsync(sourceProductParentId);
            if (_variations == null)
                return null;
            CurrImportCounters.MaxRecs = _variations.Count;
            foreach (var variant in _variations)
            {
                if ((await ImportAndMapWooEntityDataAsync(variant, sourceItemParentId)) == Guid.Empty)
                {
                    // if there is an error abort, and return a null?
                    return null;
                }
            }
            ////-> not sure if we should abort if the one import fails.
            return CurrImportCounters;
        }
        #endregion
    }
}
