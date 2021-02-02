using Microsoft.AspNetCore.Components;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.Items;
using RainbowOF.Models.Logs;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Tools;
using RainbowOF.Web.FrontEnd.Pages.ChildComponents.Modals;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImportItemsComponent : ComponentBase
    {
        [Parameter]
        public WooSettings WooSettingsModel { get; set; }
        [Parameter]
        public ILoggerManager Logger { get; set; }

        [Inject]
        public IAppUnitOfWork _AppUnitOfWork { get; set; }

        [Inject]
        public NavigationManager UriHelper { get; set; }



        public bool _IsCatImportBusy = false;
        public bool _IsAttribImportBusy = false;
        public bool _IsAttribTermImportBusy = false;
        public bool _IsItemImportBusy = false;

        private ImportCounters _importCounters = new ImportCounters();
        public string ImportingThis = String.Empty;

        protected ShowModalMessage ShowModalStatus { get; set; }
        // Private vars
        private DateTime _LogDate = DateTime.Now;
        private StringTools _StringTools = new StringTools();

        #region GenericRoutines

        // Dispay Import Status if annof the imports are happening
        public bool IsImporting()
        {
            return (_IsCatImportBusy || _IsAttribImportBusy || _IsAttribTermImportBusy) || _IsItemImportBusy ;
        }
        // Log the status of the import
        async Task LogImport(int? pIdImported, string pParameter, Models.WooSections pSection)
        {
            IAppRepository<WooSyncLog> _WooSyncLogRepository = _AppUnitOfWork.Repository<WooSyncLog>();
            await _WooSyncLogRepository.AddAsync(new WooSyncLog
            {
                // add the parameters
                WooSyncDateTime = _LogDate,
                Result = (pIdImported != null) ? Models.WooResults.Success : (pIdImported == null) ? Models.WooResults.none : Models.WooResults.Error,
                Parameters = pParameter,
                Section = pSection,
                Notes = (pIdImported != null) && (pIdImported  > 0) ?  $"Imported id: {pIdImported}, dt: {DateTime.Now:d}" : $"dt: { DateTime.Now:d}"
            }) ;
        }

        private void InitWooImport(ref bool pIsWiaiting, string pImportThis)
        {
            // do the same as with categories but now with product AttributeTerm? 
            // Shhoud each one be a class. The issues with that is that, to update the screen is  problem
            pIsWiaiting = true;
            _importCounters.Reset();   // reset all counters
            _LogDate = DateTime.Now; // set here so all records for this import are the same DateTime.
            ImportingThis = pImportThis; 
            StateHasChanged();
        }
        #endregion
        async Task SetAndLogStatusString(bool pIsError, string pSuccessString, Models.WooSections pSection)
        {
            string _StatusMessage = pIsError ? 
                $"Error Importing: {_AppUnitOfWork.GetErrorMessage()}" : // returned in there was an error importing
                pSuccessString;
            await LogImport(pIsError ? null : 0, _StatusMessage, pSection);
            ShowModalStatus.UpdateModalMessage(_StatusMessage);
        }
        // All the code for this us found in the partial class in the file WooImportItemsComponentCategoryImport
        public async Task ImportCategories_Click()
        {
            // Retrieve data from Woo
            // cycle through catagories and add to database if they do not exists
            InitWooImport(ref _IsCatImportBusy, "Categories: Checking Woo");
            await LogImport(0, "Import of Woo Categories - Initialising", Models.WooSections.ProductCategories);
            // ItemCategoryLookupImport _ItemCategoryLookupImport = new ItemCategoryLookupImport(WooSettingsModel, _importCounters );  /// need to pass the vars in
            List<ProductCategory> _WooProductCategories = await GetWooCategoryData();
            if (_WooProductCategories == null)
            {
                ShowModalStatus.UpdateModalMessage($"Error retrieving Woo Categories. Please checkWoo API settings, or if there are any Woo Categories. View log for details.");
                await LogImport(null, "Import of Woo Categories - Failed. No products retrieved. Please check Woo API settings. View log for details.", Models.WooSections.ProductCategories);
            }
            else
            {
                _importCounters.MaxRecs = _WooProductCategories.Count;
                StateHasChanged();
                int _recsImported = await ImportCategoryData(_WooProductCategories);
                await SetAndLogStatusString((_recsImported == AppUnitOfWork.CONST_WASERROR) && (_AppUnitOfWork.IsInErrorState()),
                    $"Woo categories imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {_importCounters.TotalImported} of {_WooProductCategories.Count} of which {_recsImported} have products."+
                        $"{Environment.NewLine}{Environment.NewLine} Total Added: {_importCounters.TotalAdded}. Total Updated: {_importCounters.TotalUpdated}", 
                    Models.WooSections.ProductCategories);
            }
            StateHasChanged();
            ShowModalStatus.ShowModal();
            _IsCatImportBusy = false;
        }
        /// All routines for importing Attributes fro woo are in the partial class WooImportItemsComponentAttributeImportPartial
        public async Task ImportAttrib_Click()
        {
            InitWooImport(ref _IsAttribImportBusy,"Attributes: Checking Woo");
            await LogImport(0, "Import of Woo Attributes - Initialising", Models.WooSections.ProductAttributes);
            List<ProductAttribute> _WooProductAttributes = await GetWooAttributeData();
            if (_WooProductAttributes == null)
            {
                ShowModalStatus.UpdateModalMessage($"Woo Attributes retrieval error. Please checkWoo API settings. View log for details.");
                await LogImport(null, "Import of Woo Attributes - Failed. No products retrieved. Please check Woo API settings. View log for details.", Models.WooSections.ProductAttributes);
            }
            else
            {
                _importCounters.MaxRecs = _WooProductAttributes.Count;
                StateHasChanged();
                await SetAndLogStatusString((!await ImportAttributeData(_WooProductAttributes) && _AppUnitOfWork.IsInErrorState()) , 
                    $"Woo Attributes imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {_importCounters.TotalImported} of {_WooProductAttributes.Count}."+
                        $"{Environment.NewLine}{Environment.NewLine} Total Added: {_importCounters.TotalAdded}. Total Updated: {_importCounters.TotalUpdated}", 
                    Models.WooSections.ProductAttributes);
            }
            StateHasChanged();
            ShowModalStatus.ShowModal();
            _IsAttribImportBusy = false;
        }
        public async Task ImportAttribTerm_Click()
        {
            InitWooImport(ref _IsAttribTermImportBusy,"Attribute Terms: checking Woo");
            await LogImport(0, "Import of Woo Attribute Terms - Initialising", Models.WooSections.ProductAttributeTerms);
            /// Logic a little different here, since we have to get all the imported attributes, then find all its terms, and then import those terms into the attribute values (terms)
            List<ProductAttribute> _WooProductAttributes = await GetWooAttributeData();
            if (_WooProductAttributes == null)
            {
                ShowModalStatus.UpdateModalMessage($"Woo Attribute retrieval error. To retrieve the terms we need the Attributes. Please check Woo API settings. View log for details.");
                await LogImport(0, "Import of Woo Attribute Terms - Failed. No products retrieved. Please check Woo API settings. View log for details.", Models.WooSections.ProductAttributeTerms);
            }
            else
            {
                _importCounters.MaxRecs = _WooProductAttributes.Count;
                foreach (ProductAttribute pa in _WooProductAttributes)
                {
                    StateHasChanged();
                    if (!await ImportAttributeTermData(pa))
                        break;
                    _importCounters.TotalImported++;
                    _importCounters.PercentOfRecsImported = _importCounters.CalcPercentage(_importCounters.TotalImported);
                    StateHasChanged();
                }
                await SetAndLogStatusString((_AppUnitOfWork.IsInErrorState()), 
                    $"Woo Attributes imported: {Environment.NewLine}{Environment.NewLine} Total Attributes imported: {_importCounters.TotalImported} of {_WooProductAttributes.Count}."+
                        $"{Environment.NewLine}{Environment.NewLine} Total Terms Added: {_importCounters.TotalAdded}. Total Terms Updated: {_importCounters.TotalUpdated}", 
                    Models.WooSections.ProductAttributeTerms);
            }
            StateHasChanged();
            ShowModalStatus.ShowModal();
            _IsAttribTermImportBusy = false;
        }

        public async Task ImportItems_Click()
        {
            /// Logic a little different here, since we have to get all the imported Items, then find all its terms, and then import those terms into the Item values (terms)
            string _OnlyInstock = (WooSettingsModel.OnlyInStockItemsImported == true) ? "in-stock " : "";
            InitWooImport(ref _IsItemImportBusy, $"Items: checking Woo, and loading all {_OnlyInstock}prodcuts of type published and private (may take a while to load)");
            await LogImport(0, "Import of Woo Products - Initialising", Models.WooSections.Products);
            // cycle through prdocuts and add to database if they do not exists - only do instock items if that is what is selected            
            List<Product> _WooProducts = await GetAllWooProducts(WooSettingsModel.OnlyInStockItemsImported);
            if (_WooProducts == null)
            {
                ShowModalStatus.UpdateModalMessage($"Woo Item retrieval error. To retrieve the terms we need the Items. Please check Woo API settings. View log for details.");
                await LogImport(0, "Import of Woo Products - Failed. No products retrieved. Please check Woo API settings. View log for details.", Models.WooSections.Products);
            }
            else
            {
                if (!WooSettingsModel.AreAffliateProdcutsImported)
                    _WooProducts = _WooProducts.Where(wp => wp.type != "external").ToList();
                _importCounters.MaxRecs = _WooProducts.Count;
                StateHasChanged();
                int _recsImported = await ImportProductData(_WooProducts);
                await SetAndLogStatusString((_recsImported == AppUnitOfWork.CONST_WASERROR) && (_AppUnitOfWork.IsInErrorState()),
                    $"Woo Products imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {_importCounters.TotalImported} of {_WooProducts.Count}." +
                        $"{Environment.NewLine}{Environment.NewLine} Total Added: {_importCounters.TotalAdded}. Total Updated: {_importCounters.TotalUpdated}", 
                    Models.WooSections.Products);
            }
            StateHasChanged();
            ShowModalStatus.ShowModal();
            _IsItemImportBusy = false;
        }
        public void NavigateToLog()
        {
            UriHelper.NavigateTo("ViewWooSyncLog");
        }
    }


}


