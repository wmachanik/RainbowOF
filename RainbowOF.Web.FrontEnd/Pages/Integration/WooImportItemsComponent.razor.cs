using Microsoft.AspNetCore.Components;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.Items;
using RainbowOF.Models.Logs;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Tools;
using RainbowOF.Components.Modals;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;
using RainbowOF.Tools.Services;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImportItemsComponent : ComponentBase
    {
        [Parameter]
        public WooSettings AppWooSettings { get; set; }
        [Inject]
        public ILoggerManager _Logger { get; set; }
        [Inject]
        public IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        public NavigationManager _UriHelper { get; set; }
        [Inject]
        private ApplicationState _AppState { get; set; }

        public bool IsCatImportBusy = false;
        public bool IsAttribImportBusy = false;
        public bool IsAttribTermImportBusy = false;
        public bool IsItemImportBusy = false;
        public bool IsCustomerImportBusy = false;

        private ImportCounters currImportCounters = new ImportCounters();
        public string ImportingThis = String.Empty;

        //public ShowModalMessage componentShowModalStatus { get; set; }
        protected PopUpAndLogNotification componentPopUpRef;
        // Private vars
        private DateTime _LogDate = DateTime.Now;
        private StringTools _StringTools = new StringTools();

        #region GenericRoutines

        // Display Import Status if any of the imports are happening
        public bool IsImporting()
        {
            return (IsCatImportBusy || IsAttribImportBusy || IsAttribTermImportBusy || IsItemImportBusy || IsCustomerImportBusy);
        }
        // Log the status of the import
        async Task LogImport(int? importedId, string sourceParameter, Models.WooSections importedSection)
        {
            IAppRepository<WooSyncLog> _WooSyncLogRepository = _AppUnitOfWork.Repository<WooSyncLog>();
            await _WooSyncLogRepository.AddAsync(new WooSyncLog
            {
                // add the parameters
                WooSyncDateTime = _LogDate,
                Result = (importedId != null) ? Models.WooResults.Success : (importedId == null) ? Models.WooResults.none : Models.WooResults.Error,
                Parameters = sourceParameter,
                Section = importedSection,
                Notes = (importedId != null) && (importedId  > 0) ?  $"Imported id: {importedId}, DT: {DateTime.Now:d}" : $"DT: { DateTime.Now:d}"
            }) ;
        }

        private void InitWooImport(ref bool taskIsWiaiting, string importThis)
        {
            // do the same as with categories but now with product AttributeTerm? 
            // Should each one be a class. The issues with that is that, to update the screen is  problem
            taskIsWiaiting = true;
            currImportCounters.Reset();   // reset all counters
            _LogDate = DateTime.Now; // set here so all records for this import are the same DateTime.
            ImportingThis = importThis; 
            StateHasChanged();
        }
        #endregion
        async Task SetAndLogStatusString(bool isError, string successString, Models.WooSections imprtedSection)
        {
            string _StatusMessage = isError ? 
                $"Error Importing: {_AppUnitOfWork.GetErrorMessage()}" : // returned in there was an error importing
                successString;
            await LogImport(isError ? null : 0, _StatusMessage, imprtedSection);
            componentPopUpRef.ShowQuickNotification(isError ? PopUpAndLogNotification.NotificationType.Error : PopUpAndLogNotification.NotificationType.Info, _StatusMessage);
        }
        // All the code for this us found in the partial class in the file WooImportItemsComponentCategoryImport
        public async Task ImportCategories_Click()
        {
            // Retrieve data from Woo
            // cycle through categories and add to database if they do not exists
            InitWooImport(ref IsCatImportBusy, "Categories: Checking Woo");
            await LogImport(0, "Import of Woo Categories - Initialising", Models.WooSections.ProductCategories);
            // ItemCategoryLookupImport _ItemCategoryLookupImport = new ItemCategoryLookupImport(WooSettingsModel, _importCounters );  /// need to pass the variables in
            List<ProductCategory> _WooProductCategories = await GetWooCategoryData();
            if (_WooProductCategories == null)
            {
                _AppState.WooIsActive = false; // there was an error so save the status for the rest of the app.
                componentPopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error retrieving Woo Categories. Please checkWoo API settings, or if there are any Woo Categories. View log for details.");
                await LogImport(null, "Import of Woo Categories - Failed. No products retrieved. Please check Woo API settings. View log for details.", Models.WooSections.ProductCategories);
            }
            else
            {
                _AppState.WooIsActive = true; // this means the query to Woo had success so WooMustBeActive.
                currImportCounters.MaxRecs = _WooProductCategories.Count;
                StateHasChanged();
                int _recsImported = await ImportCategoryData(_WooProductCategories);
                await SetAndLogStatusString((_recsImported == AppUnitOfWork.CONST_WASERROR) && (_AppUnitOfWork.IsInErrorState()),
                    $"Woo categories imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {currImportCounters.TotalImported} of {_WooProductCategories.Count} of which {_recsImported} have products."+
                        $"{Environment.NewLine}{Environment.NewLine} Total Added: {currImportCounters.TotalAdded}. Total Updated: {currImportCounters.TotalUpdated}", 
                    Models.WooSections.ProductCategories);
            }
            StateHasChanged();
            IsCatImportBusy = false;
        }
        /// All routines for importing Attributes fro woo are in the partial class WooImportItemsComponentAttributeImportPartial
        public async Task ImportAttrib_Click()
        {
            InitWooImport(ref IsAttribImportBusy,"Attributes: Checking Woo");
            await LogImport(0, "Import of Woo Attributes - Initialising", Models.WooSections.ProductAttributes);
            List<ProductAttribute> _wooProductAttributes = await GetWooAttributeData();
            if (_wooProductAttributes == null)
            {
                _AppState.WooIsActive = false; // there was an error so save the status for the rest of the app.
                componentPopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Attributes retrieval error. Please checkWoo API settings. View log for details.");
                await LogImport(null, "Import of Woo Attributes - Failed. No products retrieved. Please check Woo API settings. View log for details.", Models.WooSections.ProductAttributes);
            }
            else
            {
                _AppState.WooIsActive = true; // this means the query to Woo had success so WooMustBeActive.
                currImportCounters.MaxRecs = _wooProductAttributes.Count;
                StateHasChanged();
                await SetAndLogStatusString((!await ImportAttributeData(_wooProductAttributes) && _AppUnitOfWork.IsInErrorState()) , 
                    $"Woo Attributes imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {currImportCounters.TotalImported} of {_wooProductAttributes.Count}."+
                        $"{Environment.NewLine}{Environment.NewLine} Total Added: {currImportCounters.TotalAdded}. Total Updated: {currImportCounters.TotalUpdated}", 
                    Models.WooSections.ProductAttributes);
            }
            StateHasChanged();
            IsAttribImportBusy = false;
        }
        public async Task ImportAttribTerm_Click()
        {
            InitWooImport(ref IsAttribTermImportBusy,"Attribute Terms: checking Woo");
            await LogImport(0, "Import of Woo Attribute Terms - Initialising", Models.WooSections.ProductAttributeTerms);
            /// Logic a little different here, since we have to get all the imported attributes, then find all its terms, and then import those terms into the attribute values (terms)
            List<ProductAttribute> _wooProductAttributes = await GetWooAttributeData();
            if (_wooProductAttributes == null)
            {
                _AppState.WooIsActive = false; // there was an error so save the status for the rest of the app.
                componentPopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute retrieval error. To retrieve the terms we need the Attributes. Please check Woo API settings. View log for details.");
                await LogImport(0, "Import of Woo Attribute Terms - Failed. No products retrieved. Please check Woo API settings. View log for details.", Models.WooSections.ProductAttributeTerms);
            }
            else
            {
                _AppState.WooIsActive = true; // this means the query to Woo had success so WooMustBeActive.
                currImportCounters.MaxRecs = _wooProductAttributes.Count;
                foreach (ProductAttribute pa in _wooProductAttributes)
                {
                    StateHasChanged();
                    if (!await ImportAttributeTermData(pa))
                        break;
                    currImportCounters.TotalImported++;
                    currImportCounters.PercentOfRecsImported = currImportCounters.CalcPercentage(currImportCounters.TotalImported);
                    StateHasChanged();
                }
                await SetAndLogStatusString((_AppUnitOfWork.IsInErrorState()), 
                    $"Woo Attributes imported: {Environment.NewLine}{Environment.NewLine} Total Attributes imported: {currImportCounters.TotalImported} of {_wooProductAttributes.Count}."+
                        $"{Environment.NewLine}{Environment.NewLine} Total Terms Added: {currImportCounters.TotalAdded}. Total Terms Updated: {currImportCounters.TotalUpdated}", 
                    Models.WooSections.ProductAttributeTerms);
            }
            StateHasChanged();
            IsAttribTermImportBusy = false;
        }

        public async Task ImportItems_Click()
        {
            /// Logic a little different here, since we have to get all the imported Items, then find all its terms, and then import those terms into the Item values (terms)
            string _OnlyInstock = (AppWooSettings.OnlyInStockItemsImported == true) ? "in-stock " : "";
            InitWooImport(ref IsItemImportBusy, $"Items: checking Woo, and loading all {_OnlyInstock} products of type published and private (may take a while to load)");
            await LogImport(0, "Import of Woo Products - Initialising", Models.WooSections.Products);
            // cycle through products and add to database if they do not exists - only do in stock items if that is what is selected            
            List<Product> _wooProducts = await GetAllWooProducts(AppWooSettings.OnlyInStockItemsImported);
            if (_wooProducts == null)
            {
                componentPopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Item retrieval error. To retrieve the terms we need the Items. Please check Woo API settings. View log for details.");
                await LogImport(0, "Import of Woo Products - Failed. No products retrieved. Please check Woo API settings. View log for details.", Models.WooSections.Products);
            }
            else
            {
                if (!AppWooSettings.AreAffiliateProdcutsImported)
                    _wooProducts = _wooProducts.Where(wp => wp.type != "external").ToList();
                currImportCounters.MaxRecs = _wooProducts.Count;
                StateHasChanged();
                int _recsImported = await ImportProductData(_wooProducts);
                await SetAndLogStatusString((_recsImported == AppUnitOfWork.CONST_WASERROR) && (_AppUnitOfWork.IsInErrorState()),
                    $"Woo Products imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {currImportCounters.TotalImported} of {_wooProducts.Count}." +
                        $"{Environment.NewLine}{Environment.NewLine} Total Added: {currImportCounters.TotalAdded}. Total Updated: {currImportCounters.TotalUpdated}", 
                    Models.WooSections.Products);
            }
            StateHasChanged();
            IsItemImportBusy = false;
        }
        public void NavigateToLog()
        {
            _UriHelper.NavigateTo("ViewWooSyncLog");
        }
    }


}


