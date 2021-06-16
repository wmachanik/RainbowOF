using System;
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
        async Task<List<ProductAttributeTerm>> GetWooAttributeTermData(ProductAttribute currProductAttribute)
        {
            WooAPISettings _wooAPISettings = new WooAPISettings(AppWooSettings);
            //
            //    ConsumerKey = WooSettingsModel.ConsumerKey,
            //    ConsumerSecret = WooSettingsModel.ConsumerSecret,
            //    QueryURL = WooSettingsModel.QueryURL,
            //    IsSecureURL = WooSettingsModel.IsSecureURL,
            //    JSONAPIPostFix = WooSettingsModel.JSONAPIPostFix,
            //    RootAPIPostFix = WooSettingsModel.RootAPIPostFix
            //};

            IWooProductAttributeTerm _WooProductAttributeTerm = new WooProductAttributeTerm(_wooAPISettings, _Logger);
            List<ProductAttributeTerm> wooProductAttributeTerms = await _WooProductAttributeTerm.GetAttributeTermsByAtttribute(currProductAttribute);
            return wooProductAttributeTerms;
        }
        async Task<Guid> UpdateItemAttributeVariety(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, ItemAttributeVarietyLookup currItemAttributeVariety)
        {
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            bool _success = false;
            currItemAttributeVariety.ItemAttributeLookupId = sourceParentAttributeId;
            currItemAttributeVariety.VarietyName = sourcePAT.name;
            currItemAttributeVariety.Notes = $"Updated Woo AttributeTerm ID {sourcePAT.id}";
            _success = await _ItemAttributeVarietyRepository.UpdateAsync(currItemAttributeVariety) != AppUnitOfWork.CONST_WASERROR;
            return (_success ? currItemAttributeVariety.ItemAttributeVarietyLookupId : Guid.Empty);
        }
        // check if the a product if the name product.name exists if so get that ID (and update), or add it and return the id.
        async Task<Guid> AddOrGetIDItemAttributeVariety(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId)
        {
            IAppRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();

            ItemAttributeVarietyLookup _ItemAttributeVariety = await _itemAttributeVarietyRepository.FindFirstAsync(ic => ic.VarietyName == sourcePAT.name);
            if (_ItemAttributeVariety == null)
            {
                ItemAttributeVarietyLookup _newItemAttributeVariety = new ItemAttributeVarietyLookup
                {
                    ItemAttributeLookupId = sourceParentAttributeId,
                    VarietyName = sourcePAT.name,
                    Notes = $"Imported Woo Attribute Term ID {sourcePAT.id}"
                };

                int _recsAdded = await _itemAttributeVarietyRepository.AddAsync(_newItemAttributeVariety);
                return (_recsAdded != AppUnitOfWork.CONST_WASERROR) ? _newItemAttributeVariety.ItemAttributeVarietyLookupId : Guid.Empty;
            }
            else
            {
                return _ItemAttributeVariety.ItemAttributeVarietyLookupId;   // we found one with the same name so assume this is the correct one.
            }
        }

        async Task<Guid> AddOrUpdateItemAttributeVariety(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, Guid sourceWooMappedItemAttributeTermId)
        {
            Guid _itemAttributeVarietyId = Guid.Empty;
            IAppRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            // check if the AttributeTerm exists
            ItemAttributeVarietyLookup _ItemAttributeVariety = await _itemAttributeVarietyRepository.FindFirstAsync(ic => ic.ItemAttributeVarietyLookupId == sourceWooMappedItemAttributeTermId);
            if (_ItemAttributeVariety != null)
            {
                _itemAttributeVarietyId = await UpdateItemAttributeVariety(sourcePAT, sourceParentAttributeId, _ItemAttributeVariety);
            }
            else
            {
                _itemAttributeVarietyId = await AddOrGetIDItemAttributeVariety(sourcePAT, sourceParentAttributeId);
            }
            return _itemAttributeVarietyId;
        }
        async Task<Guid> UpdateProductAttributeTerm(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, WooProductAttributeTermMap sourceWooAttributeTermMap)
        {
            // we have found a mapping between the woo Product AttributeTerm and our AttributeTerm id so update the Attribute table just in case.
            Guid _itemAttributeTermId = Guid.Empty;
            IAppRepository<WooProductAttributeTermMap> _wooProductAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            _itemAttributeTermId = await AddOrUpdateItemAttributeVariety(sourcePAT, sourceParentAttributeId, sourceWooAttributeTermMap.ItemAttributeVarietyLookupId);
            /// Now update the woo AttributeTerm using the _ItemAttributeTermId returned.
            if (await _wooProductAttributeTermMapRepository.UpdateAsync(sourceWooAttributeTermMap) == AppUnitOfWork.CONST_WASERROR)
            {   // did not updated so set _ItemAttributeTermId to ItemAttributeTermID to Guid.Empty = error
                _itemAttributeTermId = Guid.Empty;
            }

            return _itemAttributeTermId;
        }
        async Task<Guid> AddProductAttributeTerm(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId, WooProductAttributeTermMap sourceWooAttributeTermMapsourceParentAttributeId)
        {
            Guid _itemAttributeTermId = Guid.Empty;
            IAppRepository<WooProductAttributeTermMap> _wooAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            // Add Item AttributeTerm if it does not exist
            _itemAttributeTermId = await AddOrGetIDItemAttributeVariety(sourcePAT, sourceParentAttributeId);
            if (sourceWooAttributeTermMapsourceParentAttributeId == null)
            {
                sourceWooAttributeTermMapsourceParentAttributeId = new WooProductAttributeTermMap
                {
                    WooProductAttributeTermId = (int)sourcePAT.id,
                    ItemAttributeVarietyLookupId = _itemAttributeTermId
                };
            }
            else
            {
                sourceWooAttributeTermMapsourceParentAttributeId.WooProductAttributeTermId = (int)sourcePAT.id;
                sourceWooAttributeTermMapsourceParentAttributeId.ItemAttributeVarietyLookupId = _itemAttributeTermId;
            }
            if (await _wooAttributeTermMapRepository.AddAsync(sourceWooAttributeTermMapsourceParentAttributeId) == AppUnitOfWork.CONST_WASERROR)
            {
                // did not add so set _ItemAttributeTermId to ItemAttributeTermID to Guid.Empty = error
                _itemAttributeTermId = Guid.Empty;
            }
            return _itemAttributeTermId;
        }
        async Task<Guid> ImportAndMapAttributeTermData(ProductAttributeTerm sourcePAT, Guid sourceParentAttributeId)
        {
            Guid _itemAttributeTermId = Guid.Empty;
            // Get repository for each database we are accessing. ItemAttributeTerm. WooProductAttributeTermMap & WooSyncLog
            IAppRepository<WooProductAttributeTermMap> _wooAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            // Import the AttributeTerm and set sync data
            ///first check if it exists in the mapping, just in case there has been a name change
            WooProductAttributeTermMap _wooAttributeTermMap = await _wooAttributeTermMapRepository.FindFirstAsync(wa => wa.WooProductAttributeTermId == sourcePAT.id);
            if (_wooAttributeTermMap != null)   // the id exists so update
            {
                _itemAttributeTermId = await UpdateProductAttributeTerm(sourcePAT, sourceParentAttributeId, _wooAttributeTermMap);
                currImportCounters.TotalUpdated++;
            }
            else      // the id does not exists so add
            {
                _itemAttributeTermId = await AddProductAttributeTerm(sourcePAT, sourceParentAttributeId, _wooAttributeTermMap);
                currImportCounters.TotalAdded++;
            }

            return _itemAttributeTermId;
        }
        // Get the Variety's Parent id using the WooMapping, if is not found then return Empty
        private Guid GetVarietysParentAttributeID(uint? sourceParentId)
        {
            IAppRepository<WooProductAttributeMap> _wooProductAttributeMapRepo = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            WooProductAttributeMap _wooProductAttributeMap = _wooProductAttributeMapRepo.FindFirst(wpa => wpa.WooProductAttributeId == sourceParentId);
            return (_wooProductAttributeMap == null) ? Guid.Empty : _wooProductAttributeMap.ItemAttributeLookupId;

        }
        string ProductAttributeTermToString(ProductAttributeTerm sourcePAT, Guid sourceImportedId)
        {
            return $"Product Attribute Term {sourcePAT.name}, id: {sourcePAT.id}, imported and Attribute Id is {sourceImportedId}";
        }
        // 1. Cycle through categories and add to database if they do not exists - storing a WooReultsDate so we can filter the results later - ?
        // 3. Log each AttributeTerm and what we do with t in the log and in the WooResults

        async Task<bool> ImportAttributeTermData(ProductAttribute sourceWooProductAttribute)
        {
            Guid _importedId = Guid.Empty;
            // cycle through categories and add to database if they do not exists
            List<ProductAttributeTerm> _wooProductAttributeTerms = await GetWooAttributeTermData(sourceWooProductAttribute);
            if (_wooProductAttributeTerms == null)
            {
                await LogImport(0, $"Attribute {(int)sourceWooProductAttribute.id}- has no attribute terms, so none imported", Models.WooSections.ProductAttributeTerms);
            }
            else
            {
                ImportingThis = $"Importing Terms for Attribute ({currImportCounters.TotalImported}/{currImportCounters.MaxRecs}): {sourceWooProductAttribute.name} with {_wooProductAttributeTerms.Count} terms ";
                Guid _varietysParentAttributeID = GetVarietysParentAttributeID(sourceWooProductAttribute.id);
                if (_varietysParentAttributeID == Guid.Empty)
                    await LogImport(0, $"Product Attribute of it {(int)sourceWooProductAttribute.id} appears not to have been imported, so cannot import terms. Please import or check log.", Models.WooSections.ProductAttributeTerms);
                else
                {
                    int _TermsDone = 0;
                    foreach (var PAT in _wooProductAttributeTerms)
                    {
                        // Import all Attribute Terms since Woo does not signal if they are used we need to import all.
                        _importedId = await ImportAndMapAttributeTermData(PAT, _varietysParentAttributeID);
                        if (_importedId == Guid.Empty)
                            return false;
                        await LogImport((int)PAT.id, ProductAttributeTermToString(PAT, _importedId), Models.WooSections.ProductAttributeTerms);
                        _TermsDone++;
                        currImportCounters.PercentOfRecsImported = currImportCounters.CalcPercentage(currImportCounters.TotalImported + (_TermsDone / (double)_wooProductAttributeTerms.Count));
                        StateHasChanged();
                    }
                }
            }
            return true;
        }
        #endregion

    }
}
