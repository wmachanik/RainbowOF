﻿using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Integration.Repositories.Woo
{
    public class WooAttributeImport : IWooImport<ItemAttributeLookup, ProductAttribute, WooProductAttributeMap>
    {
        public WooAttributeImport(IAppUnitOfWork appUnitOfWork, ILoggerManager logger) //, WooSettings appWooSettings) 
        {
            _AppUnitOfWork = appUnitOfWork;
            _Logger = logger;
      //      _AppWooSettings = appWooSettings;
        }

        public IAppUnitOfWork _AppUnitOfWork { get; set; }
        public ILoggerManager _Logger { get; set; }
        //public WooSettings _AppWooSettings { get; set ; }
        public ImportCounters CurrImportCounters { get; set; } = new();

        //  async Task<Guid> AddOrGetIDItemAttribute(ProductAttribute pPA)
        /// <summary>
        /// Check if Lookup item with Entity.Name exists if so return it, if not add it
        /// </summary>
        /// <param name="sourceEntity">Woo Product Attribute to use as a source</param>
        /// <returns>ID of lookup found or added</returns>
        public async Task<Guid> AddOrGetEntityID(ProductAttribute sourceEntity)
        {
            IAppRepository<ItemAttributeLookup> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();

            ItemAttributeLookup _ItemAttribute = await _ItemAttributeRepository.FindFirstAsync(ic => ic.AttributeName == sourceEntity.name);
            if (_ItemAttribute == null)
            {
                ItemAttributeLookup _newItemAttribute = new()
                {
                    AttributeName = sourceEntity.name,
                    Notes = $"Imported Woo Attribute ID {sourceEntity.id}"
                };
                return (await _ItemAttributeRepository.AddAsync(_newItemAttribute) != AppUnitOfWork.CONST_WASERROR) ? _newItemAttribute.ItemAttributeLookupId : Guid.Empty;
            }
            else
            {
                return _ItemAttribute.ItemAttributeLookupId;   // we found one with the same name so assume this is the correct one.
            }
        }
        //async Task<Guid> AddOrUpdateItemAttribute(ProductAttribute pPA, Guid pWooMappedItemAttributeId)
        /// <summary>
        /// Using the source Entity if it exists Update it, otherwise add it
        /// </summary>
        /// <param name="sourceEntity">Woo Product Attribute to use as a source</param>
        /// <param name="sourceWooMappedEntityId">Id that we have on our side</param>
        /// <returns></returns>
        public async Task<Guid> AddOrUpdateEntity(ProductAttribute sourceEntity, Guid sourceWooMappedEntityId)
        {
            Guid _ItemAttributeId = Guid.Empty;
            IAppRepository<ItemAttributeLookup> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            // check if the Attribute exists
            ItemAttributeLookup _ItemAttribute = await _ItemAttributeRepository.FindFirstAsync(ic => ic.ItemAttributeLookupId == sourceWooMappedEntityId);
            if (_ItemAttribute != null)
            {
                _ItemAttributeId = await UpdateEntity(sourceEntity, _ItemAttribute);
            }
            else
            {
                _ItemAttributeId = await AddOrGetEntityID(sourceEntity);
            }
            return _ItemAttributeId;
        }
        //async Task<Guid> AddProductAttribute(ProductAttribute pPA, WooProductAttributeMap pWooAttributeMap)
        /// <summary>
        /// Add Product Attribute to our Mapping Data
        /// </summary>
        /// <param name="newEntity">The woo product we believe we do not have</param>
        /// <returns>Success=> LookupId or ErrorCode => Guid.Empty</returns>
        public async Task<Guid> AddWooEntityToMapping(ProductAttribute newEntity, WooProductAttributeMap sourceWooMap)
        {
            Guid _ItemAttributeId = Guid.Empty;
            IAppRepository<WooProductAttributeMap> _WooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            // Add Item Attribute if it does not exist
            _ItemAttributeId = await AddOrGetEntityID(newEntity);
            if (sourceWooMap == null)
            {
                sourceWooMap = new WooProductAttributeMap
                {
                    WooProductAttributeId = (int)newEntity.id,
                    ItemAttributeLookupId = _ItemAttributeId,
                    CanUpdate = true
                };
            }
            else
            {
                sourceWooMap.WooProductAttributeId = (int)newEntity.id;
                sourceWooMap.ItemAttributeLookupId = _ItemAttributeId;
            }
            // return Id if we update okay
            return (await _WooAttributeMapRepository.AddAsync(sourceWooMap) != AppUnitOfWork.CONST_WASERROR) 
                ? _ItemAttributeId : Guid.Empty;      // did not updated so set _ItemAttributeId to ItemAttributeID to Guid.Empty = error== 0)
        }

        public Task<List<ProductAttribute>> GetWooEntityData(bool? onlyInStock = true)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> GetWooMappedEntityById(int sourceWooEntityId)
        {
            throw new NotImplementedException();
        }
        //        async Task<bool> ImportAttributeData(List<ProductAttribute> pWooProductAttributes)
        /// <summary>
        /// Import and Map all Woo ProductAttribute data
        /// </summary>
        /// <param name="sourceEntity">Source Product</param>
        /// <returns></returns>
        public async Task<Guid> ImportAndMapWooEntityData(ProductAttribute sourceEntity)
        {
            Guid _ItemAttributeId = Guid.Empty;
            // Get repository for each database we are accessing. ItemAttribute. WooProductAttributeMap & WooSyncLog
            IAppRepository<WooProductAttributeMap> _WooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();
            // Import the Attribute and set sync data
            ///first check if it exists in the mapping, just in case there has been a name change
            WooProductAttributeMap _wooAttributeMap = await _WooAttributeMapRepository.FindFirstAsync(wa => wa.WooProductAttributeId == sourceEntity.id);
            if (_wooAttributeMap != null)   // the id exists so update
            {
                if (_wooAttributeMap.CanUpdate)
                    _ItemAttributeId = await updatedWooEntity(sourceEntity, _wooAttributeMap);
                CurrImportCounters.TotalUpdated++;
            }
            else      // the id does not exists so add
            {
                _ItemAttributeId = await AddWooEntityToMapping(sourceEntity, _wooAttributeMap);
                CurrImportCounters.TotalAdded++;
            }

            return _ItemAttributeId;
        }
        //        async Task<Guid> UpdateItemAttribute(ProductAttribute pPA, ItemAttributeLookup pItemAttribute)
        /// <summary>
        /// Update the Lookup Attribute using date from the Woo Product
        /// </summary>
        /// <param name="updatedWooEntity"></param>
        /// <param name="updatedEntity"></param>
        /// <returns></returns>
        public async Task<Guid> UpdateEntity(ProductAttribute updatedWooEntity, ItemAttributeLookup updatedEntity)
        {
            IAppRepository<ItemAttributeLookup> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            updatedEntity.AttributeName = updatedWooEntity.name;
            updatedEntity.Notes = $"Updated Woo Attribute ID {updatedWooEntity.id}";
            return (await _ItemAttributeRepository.UpdateAsync(updatedEntity) != AppUnitOfWork.CONST_WASERROR)
                ? updatedEntity.ItemAttributeLookupId : Guid.Empty;  // there was an error updating
        }
        // async Task<Guid> UpdateProductAttribute(ProductAttribute pPA, WooProductAttributeMap pWooAttributeMap)
        /// <summary>
        /// Update the Woo Entity using mapping data.
        /// </summary>
        /// <param name="updatedWooEntity">the entity that has been updated</param>
        /// <param name="targetWooMap">the target to be updated</param>
        /// <returns></returns>
        public async Task<Guid> UpdatedWooEntity(ProductAttribute updatedWooEntity, WooProductAttributeMap targetWooMap)
        {
            // we have found a mapping between the woo Product Attribute and our Attribute id so update the Attribute table just in case.
            Guid _ItemAttributeId = Guid.Empty;
            IAppRepository<WooProductAttributeMap> _WooProductAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            _ItemAttributeId = await AddOrUpdateEntity(updatedWooEntity, targetWooMap.ItemAttributeLookupId);
            /// Now update the woo Attribute using the _ItemAttributeId returned.
            return (await _WooProductAttributeMapRepository.UpdateAsync(targetWooMap) != AppUnitOfWork.CONST_WASERROR) 
                ? _ItemAttributeId : Guid.Empty;      // did not updated so set _ItemAttributeId to ItemAttributeID to Guid.Empty = error
        }

    }
}
