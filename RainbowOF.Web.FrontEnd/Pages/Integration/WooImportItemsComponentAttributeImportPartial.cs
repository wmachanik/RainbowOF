using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Integration.Repositories.Woo;
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
        /// All the attribute import stuff. Could we have generalised this?
        /// </summary>
        #region AttrbiuteStuff

        // Retrieve data from Woo
        //---> Moved to Interface and called GetWooEntityData
        //public async Task<List<ProductAttribute>> GetWooAttributeData()
        //{
        //    WooAPISettings _WooAPISettings = new WooAPISettings(AppWooSettings); 

        //    IWooProductAttribute _WooProductAttribute = new WooProductAttribute(_WooAPISettings, _Logger);
        //    List<ProductAttribute> wooProductAttributes = await _WooProductAttribute.GetAllProductAttributes();
        //    return wooProductAttributes;
        //}
        //async Task<Guid> UpdateItemAttribute(ProductAttribute pPA, ItemAttributeLookup pItemAttribute)
        //{
        //    IAppRepository<ItemAttributeLookup> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
        //    pItemAttribute.AttributeName = pPA.name;
        //    pItemAttribute.Notes = $"Updated Woo Attribute ID {pPA.id}";
        //    return (await _ItemAttributeRepository.UpdateAsync(pItemAttribute) != AppUnitOfWork.CONST_WASERROR) ? pItemAttribute.ItemAttributeLookupId : Guid.Empty;  // there was an error updating
        //}
        //async Task<Guid> AddOrGetIDItemAttribute(ProductAttribute pPA)
        //{
        //    IAppRepository<ItemAttributeLookup> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();

        //    ItemAttributeLookup _ItemAttribute = await _ItemAttributeRepository.FindFirstAsync(ic => ic.AttributeName == pPA.name);
        //    if (_ItemAttribute == null)
        //    {
        //        ItemAttributeLookup _newItemAttribute = new ItemAttributeLookup
        //        {
        //            AttributeName = pPA.name,
        //            Notes = $"Imported Woo Attribute ID {pPA.id}"
        //        };

        //        return (await _ItemAttributeRepository.AddAsync(_newItemAttribute) != AppUnitOfWork.CONST_WASERROR) ? _newItemAttribute.ItemAttributeLookupId : Guid.Empty;
        //    }
        //    else
        //    {
        //        return _ItemAttribute.ItemAttributeLookupId;   // we found one with the same name so assume this is the correct one.
        //    }
        //}

        //async Task<Guid> AddOrUpdateItemAttribute(ProductAttribute pPA, Guid pWooMappedItemAttributeId)
        //{
        //    Guid _ItemAttributeId = Guid.Empty;
        //    IAppRepository<ItemAttributeLookup> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
        //    // check if the Attribute exists
        //    ItemAttributeLookup _ItemAttribute = await _ItemAttributeRepository.FindFirstAsync(ic => ic.ItemAttributeLookupId == pWooMappedItemAttributeId);
        //    if (_ItemAttribute != null)
        //    {
        //        _ItemAttributeId = await UpdateItemAttribute(pPA, _ItemAttribute);
        //    }
        //    else
        //    {
        //        _ItemAttributeId = await AddOrGetIDItemAttribute(pPA);
        //    }
        //    return _ItemAttributeId;
        //}
        //async Task<Guid> UpdateProductAttribute(ProductAttribute pPA, WooProductAttributeMap pWooAttributeMap)
        //{
        //    // we have found a mapping between the woo Product Attribute and our Attribute id so update the Attribute table just in case.
        //    Guid _ItemAttributeId = Guid.Empty;
        //    IAppRepository<WooProductAttributeMap> _WooProductAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

        //    _ItemAttributeId = await AddOrUpdateItemAttribute(pPA, pWooAttributeMap.ItemAttributeLookupId);
        //    /// Now update the woo Attribute using the _ItemAttributeId returned.
        //    return (await _WooProductAttributeMapRepository.UpdateAsync(pWooAttributeMap) != AppUnitOfWork.CONST_WASERROR) ? _ItemAttributeId : Guid.Empty;      // did not updated so set _ItemAttributeId to ItemAttributeID to Guid.Empty = error
        //}
        //async Task<Guid> AddProductAttribute(ProductAttribute pPA, WooProductAttributeMap pWooAttributeMap)
        //{
        //    Guid _ItemAttributeId = Guid.Empty;
        //    IAppRepository<WooProductAttributeMap> _WooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

        //    // Add Item Attribute if it does not exist
        //    _ItemAttributeId = await AddOrGetIDItemAttribute(pPA);
        //    if (pWooAttributeMap == null)
        //    {
        //        pWooAttributeMap = new WooProductAttributeMap
        //        {
        //            WooProductAttributeId = (int)pPA.id,
        //            ItemAttributeLookupId = _ItemAttributeId,
        //            CanUpdate = true
        //        };
        //    }
        //    else
        //    {
        //        pWooAttributeMap.WooProductAttributeId = (int)pPA.id;
        //        pWooAttributeMap.ItemAttributeLookupId = _ItemAttributeId;
        //    }
        //    // return Id if we update okay
        //    return (await _WooAttributeMapRepository.AddAsync(pWooAttributeMap) != AppUnitOfWork.CONST_WASERROR) ? _ItemAttributeId : Guid.Empty;      // did not updated so set _ItemAttributeId to ItemAttributeID to Guid.Empty = error== 0)
        //}
        //async Task<Guid> ImportAndMapAttributeData(ProductAttribute pPC)
        //{
        //    Guid _ItemAttributeId = Guid.Empty;
        //    // Get repository for each database we are accessing. ItemAttribute. WooProductAttributeMap & WooSyncLog
        //    IAppRepository<WooProductAttributeMap> _WooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

        //    // Import the Attribute and set sync data
        //    ///first check if it exists in the mapping, just in case there has been a name change
        //    WooProductAttributeMap _wooAttributeMap = await _WooAttributeMapRepository.FindFirstAsync(wa => wa.WooProductAttributeId == pPC.id);
        //    if (_wooAttributeMap != null)   // the id exists so update
        //    {
        //        if (_wooAttributeMap.CanUpdate)
        //            _ItemAttributeId = await UpdateProductAttribute(pPC, _wooAttributeMap);
        //        currImportCounters.TotalUpdated++;
        //    }
        //    else      // the id does not exists so add
        //    {
        //        _ItemAttributeId = await AddProductAttribute(pPC, _wooAttributeMap);
        //        currImportCounters.TotalAdded++;
        //    }

        //    return _ItemAttributeId;
        //}
        //// string
        //string ProductAttributeToString(ProductAttribute pPA, Guid pImportedId) => $"Product Attribute {pPA.name}, id: {pPA.id}, imported and Attribute Id is {pImportedId}";

        //async Task<bool> ImportAttributeData(List<ProductAttribute> pWooProductAttributes)
        //{
        //    WooImportProductAttributes _wooProductAttributeImport = new WooImportProductAttributes(_AppUnitOfWork, _Logger, AppWooSettings);
        //    // copy our current counter data to the counter used by the async tasks, we will get the data back later.
        //    _wooProductAttributeImport.CurrImportCounters = currImportCounters;

        //    Guid _IdImported = Guid.Empty;
        //    // cycle through attributes and add to database if they do not exists
        //    foreach (var pa in pWooProductAttributes)
        //    {
        //        ImportingThis = $"Importing Attribute ({_wooProductAttributeImport.CurrImportCounters.TotalImported}/{_wooProductAttributeImport.CurrImportCounters.MaxRecs}): {pa.name}";
        //        // Import all Attributes since Woo does not signal if they are used we need to import all.
        //        _IdImported = await _wooProductAttributeImport.ImportAndMapWooEntityData(pa);
        //        if (_IdImported == Guid.Empty)
        //        {
        //            return false;
        //        }
        //        _wooProductAttributeImport.CurrImportCounters.TotalImported++;
        //        _wooProductAttributeImport.CurrImportCounters.PercentOfRecsImported = _wooProductAttributeImport.CurrImportCounters.CalcPercentage(_wooProductAttributeImport.CurrImportCounters.TotalImported);

        //        // need to copy data across
        //        currImportCounters = _wooProductAttributeImport.CurrImportCounters;
        //        StateHasChanged();
        //        await LogImport((int)pa.id, ProductAttributeToString(pa, _IdImported), Models.WooSections.ProductAttributes);
        //    }
        //    return true; // if we get here no errors occurred
        //}

        #endregion
    }
}
