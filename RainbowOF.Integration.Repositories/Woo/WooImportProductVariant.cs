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
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Integration.Repositories.Woo
{
    public class WooImportVariation : IWooImportWithAParent<ItemVariant, Variation, WooProductVariantMap>
    {
        #region Local Variables
        private IMapper appMapper { get; set; }
        #endregion
        #region Public Variables
        public IUnitOfWork AppUnitOfWork { get; set; }
        public ILoggerManager AppLoggerManager { get; set; }
        public WooSettings AppWooSettings { get; set; }
        public ImportCounters CurrImportCounters { get; set; } = new ImportCounters();
        #endregion
        #region Constructor
        public WooImportVariation(IUnitOfWork AppUnitOfWork, ILoggerManager logger, WooSettings AppWooSettings, IMapper mapper)
        {
            this.AppUnitOfWork = AppUnitOfWork;
            AppLoggerManager = logger;
            this.AppWooSettings = AppWooSettings;
            appMapper = mapper;
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("WooImportVariation initialised.");
        }
        #endregion
        #region Private Variables
        //private WooImportProduct CurrWooImportProduct { get; set; } = null;
        //private WooImportProduct LocalWooImportProduct
        //{
        //    get
        //    {
        //        if (CurrWooImportProduct == null)
        //        {
        //            CurrWooImportProduct = new();
        //        }
        //        return CurrWooImportProduct;
        //    }
        //    set { CurrWooImportProduct = value; }
        //}
        #endregion
        #region Support Methods
        /// <summary>
        /// Map the Woo Product Variant to our Item Variant variable
        /// </summary>
        /// <param name="sourceWooVariant"></param>
        /// <param name="currItemVariant"></param>
        /// <returns></returns>
        public async Task<ItemVariant> MapWooProductVariantInfoAsync(Variation sourceWooVariant, ItemVariant currItemVariant)
        {
            if (currItemVariant == null)
                currItemVariant = new();

            if (sourceWooVariant.description == String.Empty)
                sourceWooVariant.description = sourceWooVariant.attributes[0].option;
            if (sourceWooVariant.sku == String.Empty)
                sourceWooVariant.sku = sourceWooVariant.attributes[0].option;

            appMapper.Map(sourceWooVariant, currItemVariant);
            //--> now we have mapped the main stuff we nee d to map the attributes
            currItemVariant = await MapWooProductVariantAttributesAsync(sourceWooVariant, currItemVariant);
            return currItemVariant;
        }
        /// <summary>
        /// Using the source woo Variant map the attributes and attribute options to the Item variant
        /// Logic: for each source variant option find the attribute in the lookup table and map it top the ItemVariant associated attribute, 
        ///     and then also using the string description the associated term to that attribute.
        /// </summary>
        /// <param name="sourceWooVariant">The source woo product</param>
        /// <param name="currentItemVariant">The Item Variant with everything else mapped.</param>
        /// <returns></returns>
        private async Task<ItemVariant> MapWooProductVariantAttributesAsync(Variation sourceWooVariant, ItemVariant currentItemVariant)
        {
            //if (currItemVariant.ItemVariantName.Contains("250"))
            //    currItemVariant.IsEnabled = true;
            if (sourceWooVariant.attributes.Count > 0)
            {
                foreach (var _wooVariant in sourceWooVariant.attributes)  // loop through all variants and add
                {
                    currentItemVariant = await AddWooVariantAsItemVariantAsync(currentItemVariant, _wooVariant);
                }
            }
            return currentItemVariant;
        }
        /// <summary>
        /// Add Woo Variant as an Item's variant. Logic:
        /// 1. See if the a variant exists, if so update it
        /// 2. Else add it to the variant list
        /// </summary>
        /// <param name="currentItemVariant"></param>
        /// <param name="curentWooVariant"></param>
        /// <returns></returns>
        private async Task<ItemVariant> AddWooVariantAsItemVariantAsync(ItemVariant currentItemVariant, VariationAttribute curentWooVariant)
        {
            IRepository<WooProductAttributeMap> wooProductAttributeMapRepo = AppUnitOfWork.Repository<WooProductAttributeMap>();
            IRepository<ItemAttributeVarietyLookup> itemAttributeVarietyLookupRepo = AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            WooProductAttributeMap wooProductAttributeMap = await wooProductAttributeMapRepo.GetByIdAsync(wpa => wpa.WooProductAttributeId == curentWooVariant.id);
            if (wooProductAttributeMap == null)   // should never be null if so there is an issue - not sure what to do with this at the moment
                return currentItemVariant;

            ItemAttributeVarietyLookup itemAttributeVarietyLookup = await itemAttributeVarietyLookupRepo
                    .GetByIdAsync(iavl =>
                                         (iavl.ItemAttributeLookupId == wooProductAttributeMap.ItemAttributeLookupId)
                                      && (iavl.VarietyName == curentWooVariant.option)
                                      );
            // if the lookup exists we need to check if it has the associated variant too, if not we add a new item, otherwise we can ignore since there is nothing to update
            //--> if an item variant id exists  (and has been added) then this is and update, so update otherwise check if it exists
            bool IsItThere = (currentItemVariant
                                .ItemVariantAssociatedLookups?.
                                    Exists(iavl => currentItemVariant.ItemVariantAssociatedLookups
                                                                            .Exists(iv => (iv.ItemVariantId != Guid.Empty)
                                                                                       && (iv.ItemVariantAssociatedLookupId == iavl.ItemVariantAssociatedLookupId))
                                           ) ?? false
                );
            if (IsItThere)  // update it then
            {
                return UpdateThisItemVariant(currentItemVariant, wooProductAttributeMap, itemAttributeVarietyLookup);
            }
            // now see if it is there but not mapped.
            Guid compareToAttributeVarietyId = itemAttributeVarietyLookup?.ItemAttributeVarietyLookupId ?? Guid.Empty;
            IsItThere = (currentItemVariant
                                .ItemVariantAssociatedLookups?
                                    .Exists(iavl =>
                                                (iavl.AssociatedAttributeLookupId == wooProductAttributeMap.ItemAttributeLookupId)
                                             && (
                                                   (iavl.AssociatedAttributeVarietyLookupId == compareToAttributeVarietyId)
                                                //                                              || (iavl.AssociatedAttributeVarietyLookupId == Guid.Empty)
                                                )
                                             )
                                ?? false);   // (currentItemVariant.ItemVariantAssociatedLookups != null) && there is one already
            if (!IsItThere)
            {
                currentItemVariant = AddThisItemVariant(currentItemVariant, wooProductAttributeMap, itemAttributeVarietyLookup);
            }
            return currentItemVariant;
        }
        private static ItemVariant AddThisItemVariant(ItemVariant currentItemVariant, WooProductAttributeMap wooProductAttributeMap, ItemAttributeVarietyLookup itemAttributeVarietyLookup)
        {
            if (currentItemVariant.ItemVariantAssociatedLookups == null)
                currentItemVariant.ItemVariantAssociatedLookups = new List<ItemVariantAssociatedLookup>();  // is this the first variant?

            currentItemVariant.ItemVariantAssociatedLookups.Add(new ItemVariantAssociatedLookup
            {
                ItemVariantId = currentItemVariant.ItemVariantId,
                AssociatedAttributeLookupId = wooProductAttributeMap.ItemAttributeLookupId,
                AssociatedAttributeVarietyLookupId = (itemAttributeVarietyLookup?.ItemAttributeVarietyLookupId ?? null)   // can be null since then it means all
            });
            return currentItemVariant;
        }

        private static ItemVariant UpdateThisItemVariant(ItemVariant currentItemVariant, WooProductAttributeMap wooProductAttributeMap, ItemAttributeVarietyLookup itemAttributeVarietyLookup)
        {
            var thisVariantVariety = currentItemVariant.ItemVariantAssociatedLookups?.
                                                        FirstOrDefault(iavl => currentItemVariant.ItemVariantAssociatedLookups.Exists(iv => iv.ItemVariantAssociatedLookupId == iavl.ItemVariantAssociatedLookupId));

            thisVariantVariety.ItemVariantId = currentItemVariant.ItemVariantId;

            thisVariantVariety.AssociatedAttributeLookupId = wooProductAttributeMap.ItemAttributeLookupId;
            thisVariantVariety.AssociatedAttributeVarietyLookupId = (itemAttributeVarietyLookup?.ItemAttributeVarietyLookupId ?? null);  // can be null

            return currentItemVariant;
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
            WooAPISettings _wooAPISettings = new(AppWooSettings);
            WooProductVariation _wooVariant = new(_wooAPISettings, AppLoggerManager);
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
            IRepository<WooProductVariantMap> _wooProductVariantMapRepo = AppUnitOfWork.Repository<WooProductVariantMap>();
            /// was not an async made it one
            WooProductVariantMap _wooProductVariantMap = await _wooProductVariantMapRepo.GetByIdAsync(wpa => wpa.WooProductVariantId == sourceWooEntityId);
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
            IRepository<WooProductVariantMap> _wooVariantMapRepository = AppUnitOfWork.Repository<WooProductVariantMap>();
            // Add Item Variant if it does not exist otherwise update
            Guid _itemVariantId = await AddOrGetEntityIDAsync(newWooEntity, sourceParentId);
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
            IRepository<ItemVariant> _itemVariantRepository = AppUnitOfWork.Repository<ItemVariant>();
            ItemVariant _itemVariant = null;
            if (sourceEntity.description != string.Empty)
                _itemVariant = await _itemVariantRepository.GetByIdAsync(ic => (ic.ItemId == sourceParentId) && (ic.ItemVariantName == sourceEntity.description));
            if (_itemVariant == null)
            {
                ItemVariant _newItemVariant = await MapWooProductVariantInfoAsync(sourceEntity, null);
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
            ItemVariant _itemVariant = await AppUnitOfWork.ItemRepository.GetItemVariantEagerByItemVariantIdAsync(sourceWooMappedEntity.ItemVariantId);
            if (_itemVariant != null)
            {
                // make sure the mapped variant is pointing to the correct parent and that the data is valid if not delete the mapping and then add.
                if (_itemVariant.ItemId == sourceParentId) // && (_itemVariant.ItemVariantAssociatedLookups.All(iva => iva.ItemVariantId == sourceWooMappedEntity.ItemVariantId)))
                {
                    _itemVariantId = await UpdateEntityAsync(sourceEntity, sourceParentId, _itemVariant);
                }
                else
                {
                    IRepository<WooProductVariantMap> _wooProductVariantMapRepository = AppUnitOfWork.Repository<WooProductVariantMap>();
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
            IRepository<ItemVariant> _itemVariantRepository = AppUnitOfWork.Repository<ItemVariant>();
            updatedEntity = await MapWooProductVariantInfoAsync(updatedWooEntity, updatedEntity);
            bool _success = await _itemVariantRepository.UpdateAsync(updatedEntity) != UnitOfWork.CONST_WASERROR;
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
            IRepository<WooProductVariantMap> _wooProductVariantMapRepository = AppUnitOfWork.Repository<WooProductVariantMap>();
            Guid _ItemVariantId = await AddOrUpdateEntityAsync(updatedWooEntity, targetWooMap, sourceParentId);
            if (_ItemVariantId == Guid.Empty)
                return Guid.Empty;  // error so exit;
            /// Now update the woo ItemVariant using the _ItemVariantId returned.
            if (await _wooProductVariantMapRepository.UpdateAsync(targetWooMap) == UnitOfWork.CONST_WASERROR)
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
            IRepository<WooProductVariantMap> wooProductVariantMapRepository = AppUnitOfWork.Repository<WooProductVariantMap>();
            // Import the ItemVariant and set sync data
            ///first check if it exists in the mapping, just in case there has been a name change
            WooProductVariantMap _wooItemVariantMap = await wooProductVariantMapRepository.GetByIdAsync(wa => wa.WooProductVariantId == sourceEntity.id);
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
        public async Task<ImportCounters> ImportProductVariantsAsync(uint sourceProductParentId, Guid sourceItemParentId)
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
