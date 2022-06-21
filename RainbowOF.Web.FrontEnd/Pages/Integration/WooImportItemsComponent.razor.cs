using AutoMapper;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Integration.Repositories.Woo;
using RainbowOF.Models.Logs;
using RainbowOF.Models.System;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.Tools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImportItemsComponent : ComponentBase
    {
        #region Parameters
        [Parameter]
        public WooSettings AppWooSettings { get; set; }
        #endregion
        #region Injections
        [Inject]
        public ILoggerManager appLoggerManager { get; set; }
        [Inject]
        public IUnitOfWork appUnitOfWork { get; set; }
        [Inject]
        public NavigationManager appUriHelper { get; set; }
        [Inject]
        private ApplicationState appState { get; set; }
        [Inject]
        private IMapper appMapper { get; set; }
        #endregion
        #region UI variables
        public bool IsCatImportBusy = false;
        public bool IsAttribImportBusy = false;
        public bool IsAttribTermImportBusy = false;
        public bool IsItemImportBusy = false;
        public bool IsCustomerImportBusy = false;
        public string ImportingThis = String.Empty;
        #endregion
        #region Private and class variables
        private ImportCounters currImportCounters = new ImportCounters();
        //public ShowModalMessage componentShowModalStatus { get; set; }
        protected PopUpAndLogNotification componentPopUpRef;
        // Private variables
        private DateTime _LogDate = DateTime.Now;
        //private StringTools _StringTools = new StringTools();
        #endregion
        #region GenericRoutines
        // Display Import Status if any of the imports are happening
        public bool IsImporting()
        {
            return (IsCatImportBusy || IsAttribImportBusy || IsAttribTermImportBusy || IsItemImportBusy || IsCustomerImportBusy);
        }
        #endregion
        #region GeneralImportUITools
        // Log the status of the import
        async Task LogImport(int? importedId, string sourceParameter, WooSections importedSection)
        {
            IRepository<WooSyncLog> _WooSyncLogRepository = appUnitOfWork.Repository<WooSyncLog>();
            await _WooSyncLogRepository.AddAsync(new WooSyncLog
            {
                // add the parameters
                WooSyncDateTime = _LogDate,
                Result = (importedId != null) ? WooResults.Success : (importedId == null) ? WooResults.none : WooResults.Error,
                Parameters = sourceParameter,
                Section = (RainbowOF.Models.WooSections)importedSection,
                Notes = (importedId != null) && (importedId> 0) ? $"Imported id: {importedId}, DT: {DateTime.Now:d}" : $"DT: { DateTime.Now:d}"
            });
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Import logged, section: {importedSection.ToString()}, source: {sourceParameter}");
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
            appLoggerManager.LogInfo($"Import started {_LogDate}: {importThis}");
        }
        private void FinWooImport(ref bool taskWasBusy, string finishedStatus)
        {
            taskWasBusy = false;
            ImportingThis = finishedStatus;
            appLoggerManager.LogInfo($"Import complete {DateTime.Now}: {finishedStatus}.");
            StateHasChanged();
        }
        async Task SetAndLogStatusString(bool isError, string successString, RainbowOF.Models.Logs.WooSections imprtedSection)
        {
            string _StatusMessage = isError ?
                $"Error Importing: {appUnitOfWork.GetErrorMessage()}" : // returned in there was an error importing
                successString;
            await LogImport(isError ? null : 0, _StatusMessage, imprtedSection);
            await componentPopUpRef.ShowQuickNotificationAsync(isError ? PopUpAndLogNotification.NotificationType.Error : PopUpAndLogNotification.NotificationType.Info, _StatusMessage);
        }
        #endregion
        #region ImportCategories
        string ProductCatToString(ProductCategory sourcePC, Guid importedId) => $"Product Category {sourcePC.name}, id: {sourcePC.id}, imported and Category Id is {importedId}";
        /// <summary>
        /// Cycle through categories and add to database if they do not exists.
        /// Store a WooReultsDate so we can filter the results later.
        /// log each category and what we do with t in the log and in the WooResults.
        /// </summary>
        /// <param name="sourceWooProductCategories">Woo Product Categories to import</param>
        /// <returns>number> 0 for number imported or 0 for error</returns>
        public async Task<int> ImportCategoryDataAsync(WooImportProductCategory currWooImportProductCategories,
            List<ProductCategory> sourceWooProductCategories)
        {
            int _numImported = 0;
            Guid _importedId;
            //List<WooItemWithParent> _categoriessWithParents = new List<WooItemWithParent>();

            //// Load the current itemCategoriers
            // cycle through categories and add to database if they do not exists
            foreach (var pc in sourceWooProductCategories)
            {
                currWooImportProductCategories.CurrImportCounters = currImportCounters;
                ImportingThis = $"Importing Category ({currImportCounters.TotalImported}/{currImportCounters.MaxRecs}): {pc.name}";
                // Import the categories that have count > 0
                if (pc.count> 0)
                {
                    // set the values as per
                    _importedId = await currWooImportProductCategories.ImportAndMapWooEntityDataAsync(pc);
                    _numImported++;
                    await LogImport((int)pc.id, ProductCatToString(pc, _importedId), WooSections.ProductCategories);
                }
                if (appUnitOfWork.IsInErrorState())
                    return 0;
                // need to copy data across
                currImportCounters = currWooImportProductCategories.CurrImportCounters;
                currImportCounters.TotalImported++;
                currImportCounters.CalcAndSetPercentage(currImportCounters.TotalImported);
                StateHasChanged();
            }
            // Now we loop through all the Attributes that have parents and find them
            foreach (var CategoryWithAParent in currWooImportProductCategories.EntityWithParents)
            {
                bool _isDone = !await currWooImportProductCategories.FindAndSetParentEntityAsync(CategoryWithAParent);
                await LogImport((int)CategoryWithAParent.ChildId,
                    $"Setting of Parent of Child Category id: {CategoryWithAParent.ChildId} " +
                    $"to Parent Id {CategoryWithAParent.ParentId} status: {_isDone}",
                    WooSections.ProductCategories);
                if (appUnitOfWork.IsInErrorState())   // was there an error that was database related?
                    return UnitOfWork.CONST_WASERROR;
            }
            return _numImported;
        }
        public async Task ImportCategories_Click()
        {
            // Retrieve data from Woo
            // cycle through categories and add to database if they do not exists
            InitWooImport(ref IsCatImportBusy, "Categories: Checking Woo");
            await LogImport(0, "Import of Woo Categories - Initialising", WooSections.ProductCategories);
            WooImportProductCategory _wooImportProductCategories = new WooImportProductCategory(appUnitOfWork, appLoggerManager, AppWooSettings);
            List<ProductCategory> _WooProductCategories = await _wooImportProductCategories.GetWooEntityDataAsync();
            if (_WooProductCategories == null)
            {
                appState.WooIsActive = false; // there was an error so save the status for the rest of the app.
                await componentPopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error retrieving Woo Categories. Please checkWoo API settings, or if there are any Woo Categories. View log for details.");
                await LogImport(null, "Import of Woo Categories - Failed. No products retrieved. Please check Woo API settings. View log for details.", WooSections.ProductCategories);
            }
            else
            {
                appState.WooIsActive = true; // this means the query to Woo had success so WooMustBeActive.
                currImportCounters.MaxRecs = _WooProductCategories.Count;
                StateHasChanged();
                int _recsImported = await ImportCategoryDataAsync(_wooImportProductCategories, _WooProductCategories);
                await SetAndLogStatusString((_recsImported == UnitOfWork.CONST_WASERROR) && (appUnitOfWork.IsInErrorState()),
                                $"Woo categories imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {currImportCounters.TotalImported} of {_WooProductCategories.Count} of which {_recsImported} have products." +
                                    $"{Environment.NewLine}{Environment.NewLine} Total Added: {currImportCounters.TotalAdded}. Total Updated: {currImportCounters.TotalUpdated}",
                                WooSections.ProductCategories);
            }
            FinWooImport(ref IsCatImportBusy, $"{currImportCounters.TotalImported}, Categories imported.");
        }
        #endregion
        #region ImportAttributes
        WooImportProductAttribute _wooImportProductAttributes { get; set; } = null;
        WooImportProductAttribute CurrentWooImportProductAttributes
        {
            get
            {
                if (_wooImportProductAttributes == null)
                    _wooImportProductAttributes = new WooImportProductAttribute(appUnitOfWork, appLoggerManager, AppWooSettings);
                return _wooImportProductAttributes;
            }
            set
            {
                _wooImportProductAttributes = value;
            }
        }
        string ProductAttributeToString(ProductAttribute pPA, Guid pImportedId) => $"Product Attribute {pPA.name}, id: {pPA.id}, imported and Attribute Id is {pImportedId}";
        async Task<bool> ImportAttributeDataAsync(List<ProductAttribute> pWooProductAttributes)
        {
            // copy our current counter data to the counter used by the async tasks, we will get the data back later.
            CurrentWooImportProductAttributes.CurrImportCounters = currImportCounters;
            Guid _IdImported = Guid.Empty;
            // cycle through attributes and add to database if they do not exists
            foreach (var pa in pWooProductAttributes)
            {
                ImportingThis = $"Importing Attribute ({CurrentWooImportProductAttributes.CurrImportCounters.TotalImported}/" +
                    $"{CurrentWooImportProductAttributes.CurrImportCounters.MaxRecs}): {pa.name}";
                // Import all Attributes since Woo does not signal if they are used we need to import all.
                _IdImported = await CurrentWooImportProductAttributes.ImportAndMapWooEntityDataAsync(pa);
                if (_IdImported == Guid.Empty)
                {
                    return false;
                }
                CurrentWooImportProductAttributes.CurrImportCounters.TotalImported++;
                CurrentWooImportProductAttributes.CurrImportCounters.CalcAndSetPercentage(CurrentWooImportProductAttributes.CurrImportCounters.TotalImported);

                // need to copy data across
                currImportCounters = CurrentWooImportProductAttributes.CurrImportCounters;
                StateHasChanged();
                await LogImport((int)pa.id, ProductAttributeToString(pa, _IdImported), WooSections.ProductAttributes);
            }
            return true; // if we get here no errors occurred
        }

        public async Task ImportAttrib_Click()
        {
            InitWooImport(ref IsAttribImportBusy, "Attributes: Checking Woo");
            await LogImport(0, "Import of Woo Attributes - Initialising", WooSections.ProductAttributes);
            List<ProductAttribute> _wooProductAttributes = await CurrentWooImportProductAttributes.GetWooEntityDataAsync();
            if (_wooProductAttributes == null)
            {
                appState.WooIsActive = false; // there was an error so save the status for the rest of the app.
                await componentPopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Attributes retrieval error. Please checkWoo API settings. View log for details.");
                await LogImport(null, "Import of Woo Attributes - Failed. No products retrieved. Please check Woo API settings. View log for details.", WooSections.ProductAttributes);
            }
            else
            {
                appState.WooIsActive = true; // this means the query to Woo had success so WooMustBeActive.
                currImportCounters.MaxRecs = _wooProductAttributes.Count;
                StateHasChanged();
                // do the import
                await SetAndLogStatusString((!await ImportAttributeDataAsync(_wooProductAttributes) && appUnitOfWork.IsInErrorState()),
                    $"Woo Attributes imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {currImportCounters.TotalImported} of {_wooProductAttributes.Count}." +
                        $"{Environment.NewLine}{Environment.NewLine} Total Added: {currImportCounters.TotalAdded}. Total Updated: {currImportCounters.TotalUpdated}",
                    WooSections.ProductAttributes);
            }
            FinWooImport(ref IsAttribImportBusy, $"{currImportCounters.TotalImported}, Attributes imported.");
        }
        #endregion
        #region Import Attribute Terms
        WooImportProductAttributeTerm _wooImportProductAttributeTerms { get; set; } = null;
        WooImportProductAttributeTerm CurrentWooImportProductAttributeTerms
        {
            get
            {
                if (_wooImportProductAttributeTerms == null)
                    _wooImportProductAttributeTerms = new WooImportProductAttributeTerm(appUnitOfWork, appLoggerManager, AppWooSettings);
                return _wooImportProductAttributeTerms;
            }
            set
            {
                _wooImportProductAttributeTerms = value;
            }
        }
        string ProductAttributeTermToString(ProductAttributeTerm sourcePAT, Guid sourceImportedId) => $"Product Attribute Term {sourcePAT.name}, id: {sourcePAT.id}, imported and Attribute Id is {sourceImportedId}";
        // 1. Cycle through categories and add to database if they do not exists - storing a WooReultsDate so we can filter the results later - ?
        // 3. Log each AttributeTerm and what we do with t in the log and in the WooResults
        private async Task<bool> ImportAttributeTermDataAsync(ProductAttribute sourceWooProductAttribute)
        {
            CurrentWooImportProductAttributeTerms.CurrImportCounters = currImportCounters;  // copy current values across to use later.

            Guid _importedId = Guid.Empty;
            // cycle through categories and add to database if they do not exists
            List<ProductAttributeTerm> _wooProductAttributeTerms = await CurrentWooImportProductAttributeTerms.GetWooEntityDataAsync((uint)sourceWooProductAttribute.id);
            if (_wooProductAttributeTerms == null)
            {
                await LogImport(0, $"Attribute {(int)sourceWooProductAttribute.id}- has no attribute terms, so none imported", WooSections.ProductAttributeTerms);
            }
            else
            {
                ImportingThis = $"Importing Terms for Attribute ({currImportCounters.TotalImported}/{currImportCounters.MaxRecs}): {sourceWooProductAttribute.name} with {_wooProductAttributeTerms.Count} terms ";
                Guid _varietysParentAttributeID = await CurrentWooImportProductAttributeTerms.GetWooMappedEntityIdByIdAsync((uint)sourceWooProductAttribute.id);   // VarietysParentAttributeID(sourceWooProductAttribute.id)
                if (_varietysParentAttributeID == Guid.Empty)
                    await LogImport(0, $"Product Attribute of it {(int)sourceWooProductAttribute.id} appears not to have been imported, so cannot import terms. Please import or check log.", WooSections.ProductAttributeTerms);
                else
                {
                    int _TermsDone = 0;
                    foreach (var PAT in _wooProductAttributeTerms)
                    {
                        // Import all Attribute Terms since Woo does not signal if they are used we need to import all.
                        _importedId = await CurrentWooImportProductAttributeTerms.ImportAndMapWooEntityDataAsync(PAT, _varietysParentAttributeID);
                        if (_importedId == Guid.Empty)
                            return false;
                        await LogImport((int)PAT.id, ProductAttributeTermToString(PAT, _importedId), WooSections.ProductAttributeTerms);
                        _TermsDone++;
                        CurrentWooImportProductAttributeTerms.CurrImportCounters.CalcAndSetPercentage(CurrentWooImportProductAttributeTerms.CurrImportCounters.TotalImported + (_TermsDone / (double)_wooProductAttributeTerms.Count));
                        currImportCounters = CurrentWooImportProductAttributeTerms.CurrImportCounters;
                        //currImportCounters.PercentOfRecsImported = currImportCounters.CalcPercentage(currImportCounters.TotalImported); // + (_TermsDone / (double)_wooProductAttributeTerms.Count));
                        StateHasChanged();
                    }
                }
            }
            return true;
        }
        public async Task ImportAttribTerm_Click()
        {
            InitWooImport(ref IsAttribTermImportBusy, "Attribute Terms: checking Woo");
            await LogImport(0, "Import of Woo Attribute Terms - Initialising", WooSections.ProductAttributeTerms);
            /// Logic a little different here, since we have to get all the imported attributes, then find all its terms, and then import those terms into the attribute values (terms)
            List<ProductAttribute> _wooProductAttributes = await CurrentWooImportProductAttributes.GetWooEntityDataAsync();
            if (_wooProductAttributes == null)
            {
                appState.WooIsActive = false; // there was an error so save the status for the rest of the app.
                await componentPopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute retrieval error. To retrieve the terms we need the Attributes. Please check Woo API settings. View log for details.");
                await LogImport(0, "Import of Woo Attribute Terms - Failed. No products retrieved. Please check Woo API settings. View log for details.", WooSections.ProductAttributeTerms);
            }
            else
            {
                appState.WooIsActive = true; // this means the query to Woo had success so WooMustBeActive.
                currImportCounters.MaxRecs = _wooProductAttributes.Count;
                foreach (ProductAttribute pa in _wooProductAttributes)
                {
                    StateHasChanged();
                    if (!await ImportAttributeTermDataAsync(pa))
                        break;
                    currImportCounters.TotalImported++;
                    //currImportCounters.PercentOfRecsImported = currImportCounters.CalcPercentage(currImportCounters.TotalImported);
                    StateHasChanged();
                }
                await SetAndLogStatusString((appUnitOfWork.IsInErrorState()),
                    $"Woo Attributes imported: {Environment.NewLine}{Environment.NewLine} Total Attributes imported: {currImportCounters.TotalImported} of {_wooProductAttributes.Count}." +
                        $"{Environment.NewLine}{Environment.NewLine} Total Terms Added: {currImportCounters.TotalAdded}. Total Terms Updated: {currImportCounters.TotalUpdated}",
                    WooSections.ProductAttributeTerms);
            }
            FinWooImport(ref IsAttribTermImportBusy, $"{currImportCounters.TotalImported}, Attribute Terms imported.");
        }
        #endregion
        #region Import Items
        /// <summary>
        /// Get All Woo Products
        /// Set counters
        /// Import each product
        /// Once finished we need to map an Items parents.We do this by cycling through the list of items that have parents, then we find the item and parent’s GUID and take match them.
        /// </summary>

        WooImportProduct _CurrentWooImportProducts { get; set; } = null;
        protected WooImportProduct CurrentWooImportProducts
        {
            get
            {
                if (_CurrentWooImportProducts == null)
                    _CurrentWooImportProducts = new WooImportProduct(appUnitOfWork,appLoggerManager,AppWooSettings, appMapper);
                return _CurrentWooImportProducts;
            }
            set 
                { _CurrentWooImportProducts = value; }
        }
        string ProductToString(Product currWooProd, Guid importedId) 
            =>  $"Product {currWooProd.name}, id: {currWooProd.id}, imported and Item Id is {importedId}";
        async Task<int> ImportProductDataAsync(List<Product> _wooProducts)
        {
            CurrentWooImportProducts.CurrImportCounters = currImportCounters;  // copy current values across to use later.

            Guid _importedId;
            foreach (var wooProd in _wooProducts)
            {
                ImportingThis = $"Importing Product ({currImportCounters.TotalImported}/{currImportCounters.MaxRecs}): {wooProd.name} woo id: {wooProd.id}";
                await LogImport((int)wooProd.id, ImportingThis, WooSections.Products);
                // set the values as per
                _importedId = await CurrentWooImportProducts.ImportAndMapWooEntityDataAsync(wooProd);
                // abort if there was an error - Or should we log and restart? need to restart DbContext somehow
                if (appUnitOfWork.IsInErrorState())
                {
                    await LogImport((int)wooProd.id, $"Error occurred importing {wooProd.name}", WooSections.Products);
                    return UnitOfWork.CONST_WASERROR;
                }
                await LogImport((int)wooProd.id, ProductToString(wooProd, _importedId), WooSections.Products);
                CurrentWooImportProducts.CurrImportCounters.TotalImported++;
                CurrentWooImportProducts.CurrImportCounters.CalcAndSetPercentage(currImportCounters.TotalImported);
                currImportCounters = CurrentWooImportProducts.CurrImportCounters;  // copy current values across to use later.
                StateHasChanged();
            }
            // Now we loop through all the Attributes that have parents and find them
            foreach (var ProductWithAParent in CurrentWooImportProducts.EntityWithParents)
            {
                bool _isDone = await CurrentWooImportProducts.FindAndSetParentEntityAsync(ProductWithAParent);

                await LogImport((int)ProductWithAParent.ChildId,
                    $"Setting of Parent of Child Item id: {ProductWithAParent.ChildId} to Parent Id {ProductWithAParent.ParentId} " +
                    $"status: {_isDone}", WooSections.ProductCategories);
                if (appUnitOfWork.IsInErrorState())   // was there an error that was database related?
                    return UnitOfWork.CONST_WASERROR;
            }
            return CurrentWooImportProducts.CurrImportCounters.TotalImported;
        }
        public async Task ImportItems_Click()
        {
            /// Logic a little different here, since we have to get all the imported Items, then find all its terms, and then import those terms into the Item values (terms)
            string _OnlyInstock = (AppWooSettings.OnlyInStockItemsImported == true) ? "in-stock " : "";
            InitWooImport(ref IsItemImportBusy, $"Items: checking Woo, and loading all {_OnlyInstock} products of type published and private (may take a while to load)");
            await LogImport(0, "Import of Woo Products - Initialising", WooSections.Products);
            // cycle through products and add to database if they do not exists - only do in stock items if that is what is selected            
            List<Product> _wooProducts = await CurrentWooImportProducts.GetWooEntityDataAsync(AppWooSettings.OnlyInStockItemsImported);
            if (_wooProducts == null)
            {
                await componentPopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Woo Item retrieval error. To retrieve the terms we need the Items. Please check Woo API settings. View log for details.");
                await LogImport(0, "Import of Woo Products - Failed. No products retrieved. Please check Woo API settings. View log for details.", WooSections.Products);
            }
            else
            {
                if (!AppWooSettings.AreAffiliateProdcutsImported)
                    _wooProducts = _wooProducts.Where(wp => wp.type != "external").ToList();
                currImportCounters.MaxRecs = _wooProducts.Count;
                StateHasChanged();
                int _recsImported = await ImportProductDataAsync(_wooProducts);
                await SetAndLogStatusString((_recsImported == UnitOfWork.CONST_WASERROR) && (appUnitOfWork.IsInErrorState()),
                    $"Woo Products imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {currImportCounters.TotalImported} of {_wooProducts.Count}." +
                        $"{Environment.NewLine}{Environment.NewLine} Total Added: {currImportCounters.TotalAdded}. Total Updated: {currImportCounters.TotalUpdated}",
                    WooSections.Products);
            }
            FinWooImport(ref IsItemImportBusy, $"{currImportCounters.TotalImported}, Products/Items imported.");
        }
        #endregion
        public void NavigateToLog()
        {
            appUriHelper.NavigateTo("ViewWooSyncLog");
        }
    }


}


