using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImportItemsComponent // for CategoryImport so file name WooImportItemsComponentAttributeImport Partial
    {
        /// <summary>
        /// All the atribute import stuff. Could we have generalised this?
        /// </summary>
        #region AttrbiuteStuff

        // Retrieve data from Woo
        public async Task<List<ProductAttribute>> GetWooAttributeData()
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

            WooProductAttribute _WooProductAttribute = new WooProductAttribute(_WooAPISettings, Logger);
            List<ProductAttribute> wooProductAttributes = await _WooProductAttribute.GetAllProductAttributes();
            return wooProductAttributes;
        }
        async Task<Guid> UpdateItemAttribute(ProductAttribute pPA, ItemAttributeLookup pItemAttribute)
        {
            IAppRepository<ItemAttributeLookup> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            pItemAttribute.AttributeName = pPA.name;
            pItemAttribute.Notes = $"Updated Woo Attribute ID {pPA.id}";
            return (await _ItemAttributeRepository.UpdateAsync(pItemAttribute) != AppUnitOfWork.CONST_WASERROR) ? pItemAttribute.ItemAttributeLookupId : Guid.Empty;  // there was an error updating

        }
        async Task<Guid> AddOrGetIDItemAttribute(ProductAttribute pPA)
        {
            IAppRepository<ItemAttributeLookup> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();

            ItemAttributeLookup _ItemAttribute = await _ItemAttributeRepository.FindFirstAsync(ic => ic.AttributeName == pPA.name);
            if (_ItemAttribute == null)
            {
                ItemAttributeLookup _newItemAttribute = new ItemAttributeLookup
                {
                    AttributeName = pPA.name,
                    Notes = $"Imported Woo Attribute ID {pPA.id}"
                };

                return (await _ItemAttributeRepository.AddAsync(_newItemAttribute) != AppUnitOfWork.CONST_WASERROR) ? _newItemAttribute.ItemAttributeLookupId : Guid.Empty;
            }
            else
            {
                return _ItemAttribute.ItemAttributeLookupId;   // we found one with the same name so assume this is the correct one.
            }
        }

        async Task<Guid> AddOrUpdateItemAttribute(ProductAttribute pPA, Guid pWooMappedItemAttributeId)
        {
            Guid _ItemAttributeId = Guid.Empty;
            IAppRepository<ItemAttributeLookup> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            // check if the Attribute existgs
            ItemAttributeLookup _ItemAttribute = await _ItemAttributeRepository.FindFirstAsync(ic => ic.ItemAttributeLookupId == pWooMappedItemAttributeId);
            if (_ItemAttribute != null)
            {
                _ItemAttributeId = await UpdateItemAttribute(pPA, _ItemAttribute);
            }
            else
            {
                _ItemAttributeId = await AddOrGetIDItemAttribute(pPA);
            }
            return _ItemAttributeId;
        }
        async Task<Guid> UpdateProductAttribute(ProductAttribute pPA, WooProductAttributeMap pWooAttributeMap)
        {
            // we have found a mapping between the woo Product Attribute and our Attribute id so update the Attrbiute table just incase.
            Guid _ItemAttributeId = Guid.Empty;
            IAppRepository<WooProductAttributeMap> _WooProductAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            _ItemAttributeId = await AddOrUpdateItemAttribute(pPA, pWooAttributeMap.ItemAttributeLookupId);
            /// Now update the woo Attribute using the _ItemAttributeId returned.
            return (await _WooProductAttributeMapRepository.UpdateAsync(pWooAttributeMap) != AppUnitOfWork.CONST_WASERROR) ? _ItemAttributeId : Guid.Empty;      // did not updated so set _ItemAttributeId to ItemAttributeID to Guid.Empty = error
        }
        async Task<Guid> AddProductAttribute(ProductAttribute pPA, WooProductAttributeMap pWooAttributeMap)
        {
            Guid _ItemAttributeId = Guid.Empty;
            IAppRepository<WooProductAttributeMap> _WooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            // Add Item Attribute if it does not exist
            _ItemAttributeId = await AddOrGetIDItemAttribute(pPA);
            if (pWooAttributeMap == null)
            {
                pWooAttributeMap = new WooProductAttributeMap
                {
                    WooProductAttributeId = (int)pPA.id,
                    ItemAttributeLookupId = _ItemAttributeId
                };
            }
            else
            {
                pWooAttributeMap.WooProductAttributeId = (int)pPA.id;
                pWooAttributeMap.ItemAttributeLookupId = _ItemAttributeId;
            }
            // return Id if we update okay
            return (await _WooAttributeMapRepository.AddAsync(pWooAttributeMap) != AppUnitOfWork.CONST_WASERROR) ? _ItemAttributeId : Guid.Empty;      // did not updated so set _ItemAttributeId to ItemAttributeID to Guid.Empty = error== 0)
        }
        async Task<Guid> ImportAndMapAttributeData(ProductAttribute pPC)
        {
            Guid _ItemAttributeId = Guid.Empty;
            // Get repostiory for each database we are accessing. ItemAttribute. WooProductAttributeMap & WooSyncLog
            IAppRepository<WooProductAttributeMap> _WooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            // Import the Attribute and set sync data
            ///first check if it exists in the mapping, just incase there has been a name change
            WooProductAttributeMap _WooAttributeMap = await _WooAttributeMapRepository.FindFirstAsync(wa => wa.WooProductAttributeId == pPC.id);
            if (_WooAttributeMap != null)   // the id exists so update
            {
                _ItemAttributeId = await UpdateProductAttribute(pPC, _WooAttributeMap);
                _importCounters.TotalUpdated++;
            }
            else      // the id does not exists so add
            {
                _ItemAttributeId = await AddProductAttribute(pPC, _WooAttributeMap);
                _importCounters.TotalAdded++;
            }

            return _ItemAttributeId;
        }
        // string
        string ProductAttributeToString(ProductAttribute pPA, Guid pImportedId)
        {
            return $"Product Attribute {pPA.name}, id: {pPA.id}, imported and Attribute Id is {pImportedId}";
        }
        // 1. Cycle through catagories and add to database if they do not exists - storing a WooReultsDate so we can filter the results later - ?
        // 3. Log each Attribute and what we do with t in the log and in the WooResults
        async Task<bool> ImportAttributeData(List<ProductAttribute> pWooProductAttributes)
        {

            _importCounters.TotalImported = 0;
            Guid _IdImported = Guid.Empty;
            // cycle through catagories and add to database if they do not exists
            foreach (var pa in pWooProductAttributes)
            {
                ImportingThis = $"Importing Attribute ({_importCounters.TotalImported}/{_importCounters.MaxRecs}): {pa.name}";
                // Import all Attributes since Woo does not signal if they are used we need to import all.
                _IdImported = await ImportAndMapAttributeData(pa);
                if (_IdImported == Guid.Empty)
                {
                    return false;
                }
                _importCounters.TotalImported++;
                _importCounters.PercentOfRecsImported = _importCounters.CalcPercentage(_importCounters.TotalImported);
                StateHasChanged();
                await LogImport((int)pa.id, ProductAttributeToString(pa, _IdImported), Models.WooSections.ProductAttributes);
            }
            return true; // if we get here no errors occured
        }

        #endregion
    }
}
