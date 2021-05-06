﻿using System;
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

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImportItemsComponent // for CategoryImport so file name WooImportItemsComponentAttributeTermsImport Partial
    {
        /// <summary>
        /// All the attribute term import stuff. Attributes Terms in Woo are Attributed Varieties to us. Could we have generalised this for each item import with an object?
        /// </summary>
        #region AttrbiuteStuff

        // Retrieve data from Woo
        async Task<List<ProductAttributeTerm>> GetWooAttributeTermData(ProductAttribute pProductAttribute)
        {
            WooAPISettings _WooAPISettings = new WooAPISettings(WooSettingsModel);
            //
            //    ConsumerKey = WooSettingsModel.ConsumerKey,
            //    ConsumerSecret = WooSettingsModel.ConsumerSecret,
            //    QueryURL = WooSettingsModel.QueryURL,
            //    IsSecureURL = WooSettingsModel.IsSecureURL,
            //    JSONAPIPostFix = WooSettingsModel.JSONAPIPostFix,
            //    RootAPIPostFix = WooSettingsModel.RootAPIPostFix
            //};

            IWooProductAttributeTerm _WooProductAttributeTerm = new WooProductAttributeTerm(_WooAPISettings, Logger);
            List<ProductAttributeTerm> wooProductAttributeTerms = await _WooProductAttributeTerm.GetAttributeTermsByAtttribute(pProductAttribute);
            return wooProductAttributeTerms;
        }
        async Task<Guid> UpdateItemAttributeVariety(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, ItemAttributeVarietyLookup pItemAttributeVariety)
        {
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            bool _success = false;
            pItemAttributeVariety.ItemAttributeLookupId = sourceParentAttributeId;
            pItemAttributeVariety.VarietyName = sourcePAT.name;
            pItemAttributeVariety.Notes = $"Updated Woo AttributeTerm ID {sourcePAT.id}";
            _success = await _ItemAttributeVarietyRepository.UpdateAsync(pItemAttributeVariety) != AppUnitOfWork.CONST_WASERROR;
            return (_success ? pItemAttributeVariety.ItemAttributeVarietyLookupId : Guid.Empty);
        }
        // check if the a product if the name product.name exists if so get that ID (and update), or add it and return the id.
        async Task<Guid> AddOrGetIDItemAttributeVariety(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId)
        {
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();

            ItemAttributeVarietyLookup _ItemAttributeVariety = await _ItemAttributeVarietyRepository.FindFirstAsync(ic => ic.VarietyName == sourcePAT.name);
            if (_ItemAttributeVariety == null)
            {
                ItemAttributeVarietyLookup _newItemAttributeVariety = new ItemAttributeVarietyLookup
                {
                    ItemAttributeLookupId = sourceParentAttributeId,
                    VarietyName = sourcePAT.name,
                    Notes = $"Imported Woo Attribute Term ID {sourcePAT.id}"
                };

                int _recsAdded = await _ItemAttributeVarietyRepository.AddAsync(_newItemAttributeVariety);
                return (_recsAdded != AppUnitOfWork.CONST_WASERROR) ? _newItemAttributeVariety.ItemAttributeVarietyLookupId : Guid.Empty;
            }
            else
            {
                return _ItemAttributeVariety.ItemAttributeVarietyLookupId;   // we found one with the same name so assume this is the correct one.
            }
        }

        async Task<Guid> AddOrUpdateItemAttributeVariety(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, Guid sourceWooMappedItemAttributeTermId)
        {
            Guid _ItemAttributeVarietyId = Guid.Empty;
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            // check if the AttributeTerm exists
            ItemAttributeVarietyLookup _ItemAttributeVariety = await _ItemAttributeVarietyRepository.FindFirstAsync(ic => ic.ItemAttributeVarietyLookupId == sourceWooMappedItemAttributeTermId);
            if (_ItemAttributeVariety != null)
            {
                _ItemAttributeVarietyId = await UpdateItemAttributeVariety(sourcePAT, sourceParentAttributeId, _ItemAttributeVariety);
            }
            else
            {
                _ItemAttributeVarietyId = await AddOrGetIDItemAttributeVariety(sourcePAT, sourceParentAttributeId);
            }
            return _ItemAttributeVarietyId;
        }
        async Task<Guid> UpdateProductAttributeTerm(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, WooProductAttributeTermMap sourceWooAttributeTermMap)
        {
            // we have found a mapping between the woo Product AttributeTerm and our AttributeTerm id so update the Attribute table just in case.
            Guid _ItemAttributeTermId = Guid.Empty;
            IAppRepository<WooProductAttributeTermMap> _WooProductAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            _ItemAttributeTermId = await AddOrUpdateItemAttributeVariety(sourcePAT, sourceParentAttributeId, sourceWooAttributeTermMap.ItemAttributeVarietyLookupId);
            /// Now update the woo AttributeTerm using the _ItemAttributeTermId returned.
            if (await _WooProductAttributeTermMapRepository.UpdateAsync(sourceWooAttributeTermMap) == AppUnitOfWork.CONST_WASERROR)
            {   // did not updated so set _ItemAttributeTermId to ItemAttributeTermID to Guid.Empty = error
                _ItemAttributeTermId = Guid.Empty;
            }

            return _ItemAttributeTermId;
        }
        async Task<Guid> AddProductAttributeTerm(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, WooProductAttributeTermMap sourceWooAttributeTermMapsourceParentAttributeId)
        {
            Guid _ItemAttributeTermId = Guid.Empty;
            IAppRepository<WooProductAttributeTermMap> _WooAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            // Add Item AttributeTerm if it does not exist
            _ItemAttributeTermId = await AddOrGetIDItemAttributeVariety(sourcePAT, sourceParentAttributeId);
            if (sourceWooAttributeTermMapsourceParentAttributeId == null)
            {
                sourceWooAttributeTermMapsourceParentAttributeId = new WooProductAttributeTermMap
                {
                    WooProductAttributeTermId = (int)sourcePAT.id,
                    ItemAttributeVarietyLookupId = _ItemAttributeTermId
                };
            }
            else
            {
                sourceWooAttributeTermMapsourceParentAttributeId.WooProductAttributeTermId = (int)sourcePAT.id;
                sourceWooAttributeTermMapsourceParentAttributeId.ItemAttributeVarietyLookupId = _ItemAttributeTermId;
            }
            if (await _WooAttributeTermMapRepository.AddAsync(sourceWooAttributeTermMapsourceParentAttributeId) == AppUnitOfWork.CONST_WASERROR)
            {
                // did not add so set _ItemAttributeTermId to ItemAttributeTermID to Guid.Empty = error
                _ItemAttributeTermId = Guid.Empty;
            }

            return _ItemAttributeTermId;
        }
        async Task<Guid> ImportAndMapAttributeTermData(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId)
        {
            Guid _ItemAttributeTermId = Guid.Empty;
            // Get repository for each database we are accessing. ItemAttributeTerm. WooProductAttributeTermMap & WooSyncLog
            IAppRepository<WooProductAttributeTermMap> _WooAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            // Import the AttributeTerm and set sync data
            ///first check if it exists in the mapping, just in case there has been a name change
            WooProductAttributeTermMap _WooAttributeTermMap = await _WooAttributeTermMapRepository.FindFirstAsync(wa => wa.WooProductAttributeTermId == sourcePAT.id);
            if (_WooAttributeTermMap != null)   // the id exists so update
            {
                _ItemAttributeTermId = await UpdateProductAttributeTerm(sourcePAT, sourceParentAttributeId, _WooAttributeTermMap);
                _importCounters.TotalUpdated++;
            }
            else      // the id does not exists so add
            {
                _ItemAttributeTermId = await AddProductAttributeTerm(sourcePAT, sourceParentAttributeId, _WooAttributeTermMap);
                _importCounters.TotalAdded++;
            }

            return _ItemAttributeTermId;
        }
        // Get the Variety's Parent id using the WooMapping, if is not found then return Empty
        private Guid GetVarietysParentAttributeID(uint? sourceParentId)
        {
            IAppRepository<WooProductAttributeMap> _WooProductAttributeMapRepo = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            WooProductAttributeMap _WooProductAttributeMap = _WooProductAttributeMapRepo.FindFirst(wpa => wpa.WooProductAttributeId == sourceParentId);
            return (_WooProductAttributeMap == null) ? Guid.Empty : _WooProductAttributeMap.ItemAttributeLookupId;

        }
        string ProductAttributeTermToString(ProductAttributeTerm sourcePAT, Guid sourceImportedId)
        {
            return $"Product Attribute Term {sourcePAT.name}, id: {sourcePAT.id}, imported and Attribute Id is {sourceImportedId}";
        }
        // 1. Cycle through categories and add to database if they do not exists - storing a WooReultsDate so we can filter the results later - ?
        // 3. Log each AttributeTerm and what we do with t in the log and in the WooResults

        async Task<bool> ImportAttributeTermData(ProductAttribute sourceWooProductAttribute)
        {
            Guid _IdImported = Guid.Empty;
            // cycle through categories and add to database if they do not exists
            List<ProductAttributeTerm> _WooProductAttributeTerms = await GetWooAttributeTermData(sourceWooProductAttribute);
            if (_WooProductAttributeTerms == null)
            {
                await LogImport(0, $"Attribute {(int)sourceWooProductAttribute.id}- has no attribute terms, so none imported", Models.WooSections.ProductAttributeTerms);
            }
            else
            {
                ImportingThis = $"Importing Terms for Attribute ({_importCounters.TotalImported}/{_importCounters.MaxRecs}): {sourceWooProductAttribute.name} with {_WooProductAttributeTerms.Count} terms ";
                Guid _VarietysParentAttributeID = GetVarietysParentAttributeID(sourceWooProductAttribute.id);
                if (_VarietysParentAttributeID == Guid.Empty)
                    await LogImport(0, $"Product Attribute of it {(int)sourceWooProductAttribute.id} appears not to have been imported, so cannot import terms. Please import or check log.", Models.WooSections.ProductAttributeTerms);
                else
                {
                    int _TermsDone = 0;
                    foreach (var PAT in _WooProductAttributeTerms)
                    {
                        // Import all Attribute Terms since Woo does not signal if they are used we need to import all.
                        _IdImported = await ImportAndMapAttributeTermData(PAT, _VarietysParentAttributeID);
                        if (_IdImported == Guid.Empty)
                            return false;
                        await LogImport((int)PAT.id, ProductAttributeTermToString(PAT, _IdImported), Models.WooSections.ProductAttributeTerms);
                        _TermsDone++;
                        _importCounters.PercentOfRecsImported = _importCounters.CalcPercentage(_importCounters.TotalImported + (_TermsDone / (double)_WooProductAttributeTerms.Count));
                        StateHasChanged();
                    }
                }
            }
            return true;
        }
        #endregion

    }
}