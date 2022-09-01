using AutoMapper;
using Blazorise.RichTextEdit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using RainbowOF.Components.Modals;
using RainbowOF.Integration.Repositories.Woo;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Items;
using RainbowOF.Web.FrontEnd.Pages.ChildComponents.Items;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemEdit : ComponentBase, IDisposable
    {
        #region Injected items
        [Inject]
        private IUnitOfWork appUnitOfWork { get; set; }
        //[Inject]
        //private ApplicationState AppState { get; set; }
        [Inject]
        internal ILoggerManager AppLoggerManager { get; set; }
        [Inject]
        internal IMapper AppMapper { get; set; }
        [Inject]
        internal NavigationManager AppNavigationManager { get; set; }
        [Inject]
        internal IJSRuntime AppJSRuntime { get; set; }
        #endregion
        #region Parameters
        [Parameter]
        public string Id { get; set; }
        #endregion
        #region interface and component variables
        private ItemView itemEditing { get; set; } = null;
        private EditContext itemEditContext { get; set; } = null;
        //public List<ItemVariant> ItemVariants = null;
        //public List<ItemCategory> ItemCategories = null;
        //public List<ItemAttribute> ItemAttributes = null;
        //public List<ItemAttributeVariety> itemAttributeVarieties = null
        public PopUpAndLogNotification PopUpRef { get; set; }
        #endregion
        #region private variables
        private string selectedItemTag { get; set; } = "MainDetail";
        private bool collapseItemDetailsVisible { get; set; } = true;
        private bool collapseItemMoreDetailsVisible { get; set; } = true;
        private bool collapseCategoriesVisible { get; set; } = true;
        private bool collapseAttributesVisible { get; set; } = true;
        private bool displayItemVariants { get; set; } = false;
        private bool collapseItemVariantsVisible { get; set; } = true;
        private bool isItemImportBusy { get; set; } = false;
        private bool doAutoECommerceSync { get; set; } = false;
        private bool itemHasChanged { get; set; } = false;
        private RichTextEdit richTextEditRef { get; set; }
        private string loadingMessage { get; set; } = "Loading item...";
        private ConfirmModal importConfirmationModal { get; set; }
        private ConfirmModal cancelConfirmationModal { get; set; }
        private Guid currentItemId { get; set; } = Guid.Empty;  // set when we get the parameter
        private Guid? lastPrimaryCategoryLookupId { get; set; } = Guid.Empty; // to store the current Primary Category, if changed reload
        private ItemTypes currentItemType { get; set; } = ItemTypes.Other;  // used to store the current item type
        private ItemVariantsComponent itemVariantsComponent { get; set; }
        class CategoryNode
        {
            public Guid CategoryId { get; set; }
            public string CategoryName { get; set; }
            public bool IsChecked { get; set; } = true;
        }
        //private read only List<CategoryNode> categoryNodes = new();
        #endregion
        //IList<CategoryNode> ExpandedNodes = new List<CategoryNode>();
        //CategoryNode selectedNode;
        #region  Constructor
        protected async override Task OnInitializedAsync()
        {
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("ItemEdit initialising.");
            if (Guid.TryParse(Id, out Guid ItemId))
            {
                if (ItemId != Guid.Empty)
                {
                    await LoadItemFromId(ItemId);
                }
            }
            // set event for edit changes
            itemEditContext = new EditContext(itemEditing);
            itemEditContext.OnFieldChanged += ItemEditContext_OnFieldChanged;
            // trap the change even so we can check if the item is saved.
            AppNavigationManager.LocationChanged += ItemEditContext_OnLocationChanged;
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("ItemEdit initialised.");
            await base.OnInitializedAsync();
        }

        #endregion
        #region Interface routines
        async Task LoadItemFromId(Guid ItemId)
        {
            itemEditing = new();
            Item entity = await appUnitOfWork.ItemRepository.FindFirstEagerLoadingItemAsync(it => it.ItemId == ItemId);
            if (entity == null)
            {
                //-> Pop up not init -> PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Item with id {ItemId} not found...");
                loadingMessage = $"Item with id {ItemId} not found...";
            }
            else
            {
                currentItemId = ItemId; // set the Item Id so we can use it from now on.
                IRepository<WooProductMap> _wooAttributeMapRepository = appUnitOfWork.Repository<WooProductMap>();
                WooProductMap _wooProductMap = await _wooAttributeMapRepository.GetByIdAsync(wcm => wcm.ItemId == ItemId);
                //  map all the items across to the view then allocate extra woo stuff if exists.
                AppMapper.Map(entity, itemEditing);
                lastPrimaryCategoryLookupId = itemEditing.PrimaryItemCategoryLookupId;
                // if it is an item that has variants then expand the variant accordion and display the Variant selection
                displayItemVariants = collapseItemVariantsVisible = itemEditing.ItemType == ItemTypes.Variable;
                currentItemType = itemEditing.ItemType;
                //-> if it has variants and is not marked as variable we display a message
                if ((!displayItemVariants) && (entity.ItemAttributes != null) && (entity.ItemAttributes.Exists(ia => ia.IsUsedForItemVariety)))
                    await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Item: {itemEditing.ItemName} is not marked as a variable type, but is allowed variants. Change to variable type");
            }
        }
        /// <summary>
        ///  Return a list of Attribute Varieties that are available for selection
        ///  Logic:
        ///     1. Get a list of all ids that are already allocated except the current item
        ///     2. set list to be the current item and any options that not already allocated
        /// </summary>
        /// <param name="currentCategoryLookupId">The one that is currently selected</param>
        /// <param name="mustForce">must the list be reloaded (used for refresh etc.)</param>
        /// <returns>List of item attribute varieties to be used.</returns>
        public List<ItemCategoryLookup> GetListOfItemsCategoryLookups(Guid CurrentItemId, bool IsForceReload = false)
        {
            List<ItemCategoryLookup> itemCategoryLookups = appUnitOfWork.GetListOfAnItemsCategories(CurrentItemId, IsForceReload);
            //if (currentCategoryLookupId == null)
            //    return _itemCategorys;  // no item selected 
            //// 1.
            //var _usedItems = ModelItemCategories.Where(mic => (mic.ItemCategoryLookupId != currentCategoryLookupId));
            //// 2. 
            //var _unselectedItems = _itemCategorys.Where(iav => !_usedItems.Any(miav => (miav.ItemCategoryLookupId == iav.ItemCategoryLookupId)));
            return itemCategoryLookups;
        }
        public List<Item> GetListOfSimilarItems(Guid currentItemId,
                                                Guid? currentItemPrimaryCategory)
        {
            bool reload = lastPrimaryCategoryLookupId != currentItemPrimaryCategory;
            if (reload) lastPrimaryCategoryLookupId = currentItemPrimaryCategory;
            List<Item> items = appUnitOfWork.GetListOfSimilarItems(currentItemId, currentItemPrimaryCategory, reload);
            return items;
        }
        public async Task OnContentChanged()
        {
            var content = await richTextEditRef.GetHtmlAsync();
            itemEditing.ItemDetail = content;
        }
        /// <summary>
        /// Handle the button click to re-import an woo item.
        /// Logic: 
        /// 1. Confirm that the item will be over written. Once confirmed the ConfirmImport is run
        public async Task ImportItem_ClickAsync()
        {
            await importConfirmationModal.ShowModalAsync("This is a confirmation", "Are you sure you import this item, data will de over written?", "Please Confirm", "Cancel");
        }
        public async Task ItemAttributeChangedToVariableAsync(bool IsNowVariable)
        {
            if (IsNowVariable)
            {
                if (itemEditing.ItemType != ItemTypes.Variable)
                {
                    // mark the item as simple and then also save the item
                    itemEditing.ItemType = ItemTypes.Variable;
                    itemHasChanged = true;
                }
                //                await InvokeAsync(StateHasChanged); //-> should pick up that the type is now variable, or an additional variant has been added
            }
            else
            {
                if (itemEditing.ItemType == ItemTypes.Variable)
                {
                    // The type for an attribute has changed. But are there any others?
                    bool ItemHasVariableAttributes = await appUnitOfWork.ItemRepository.DoesThisItemHaveVariableAttributesAsync(currentItemId);
                    if (!ItemHasVariableAttributes)
                    {
                        // mark the item as simple and then also save the item
                        itemEditing.ItemType = ItemTypes.Simple;
                        itemHasChanged = true;
                    }
                }
            }
            displayItemVariants = collapseItemVariantsVisible = itemEditing.ItemType == ItemTypes.Variable;  ///--->> move to routine so display error if there are variants and marked as simple
            await InvokeAsync(StateHasChanged); //-> should pick up that the type is now variable
        }
        /// <summary>
        /// Called by the modal that will return if the user wants to import. 
        /// Logic
        /// 1. If user cancelled then do nothing.
        /// 2. Get the app woo settings.
        /// 3. Use the ItemId to see if the item is linked via the WooMapping to a Woo product, then continue otherwise show message there is no mapping
        /// 4. Using the returned mapping, get the woo product that it is mapped to using ID
        /// 5. and import it.
        /// </summary>
        /// <returns>null</returns>
        async Task ConfirmImport_ClickAsync(bool confirmClicked)
        {
            /// 1. If user cancelled then do nothing.
            if (confirmClicked)
            {
                await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Importing Item form Woo...");
                isItemImportBusy = true;
                /// 2. Get the app woo settings.
                IRepository<WooSettings> _wooPrefsAppRepository = appUnitOfWork.Repository<WooSettings>();
                WooSettings _wooSettings = await _wooPrefsAppRepository.FindFirstAsync();
                if (_wooSettings == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "No woo settings retrieved. Please check your settings.");
                    isItemImportBusy = false;
                    return;
                }
                /// 3. Use the ItemId to see if the item is linked via the WooMapping to a Woo product, then continue otherwise show message there is no mapping
                IRepository<WooProductMap> _wooProductMapRepository = appUnitOfWork.Repository<WooProductMap>();
                WooProductMap _wooProductMap = await _wooProductMapRepository.GetByIdAsync(wpm => wpm.ItemId == currentItemId);
                if (_wooProductMap == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"No woo product is mapped to item id: {currentItemId}.");
                    isItemImportBusy = false;
                    return;
                }
                /// 4. Using the returned mapping, get the woo product that it is mapped to using ID
                WooAPISettings _wooAPISettings = new(_wooSettings);
                WooProduct _wooProduct = new(_wooAPISettings, AppLoggerManager);
                if (_wooProduct == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error creating Woo Product engine.");
                    isItemImportBusy = false;
                    return;
                }
                Product _product = await _wooProduct.GetProductByIdAsync(_wooProductMap.WooProductId);
                if (_product == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"No product with id {_wooProductMap.WooProductId} found on Woo.");
                    isItemImportBusy = false;
                    return;
                }

                var CurrWooImportProduct = new WooImportProduct(appUnitOfWork, AppLoggerManager, _wooAPISettings, AppMapper);
                /// 5. and import it.
                var _importedId = await CurrWooImportProduct.ImportAndMapWooEntityDataAsync(_product);
                // abort if there was an error - Or should we log and restart? need to restart DbContext somehow
                if (appUnitOfWork.IsInErrorState())
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error importing product with id {_wooProductMap.WooProductId} found on Woo.");
                else
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Product with id {_wooProductMap.WooProductId} found on Woo and imported.");
                // Reload item.
                await LoadItemFromId(currentItemId);
                StateHasChanged();
            }
            isItemImportBusy = false;
        }
        /// <summary>
        ///  Handel the cancel click button
        /// </summary>
        /// <returns>void</returns>
        public async Task CancelItem_ClickAsync()
        {
            await cancelConfirmationModal.ShowModalAsync("Please confirm cancellation ", "Are you sure you want to cancel editing?", "Yes", "No");
        }
        /// <summary>
        /// Go back to the previous page using Java
        /// </summary>
        /// <returns></returns>
        private async Task BackToPreviousPage()
        {
            await AppJSRuntime.InvokeVoidAsync("history.back");
        }
        /// <summary>
        /// When they select cancel then cancel
        /// </summary>
        /// <param name="mustCancel"></param>
        /// <returns></returns>
        async Task ConfirmCancel_ClickAsync(bool mustCancel)
        {
            if (mustCancel)
            {
                itemHasChanged = false; // --> cancel changes
                await BackToPreviousPage();
            }
        }
        /// <summary>
        /// Save the item being edited. This is assumed to be the item in ItemEditing
        /// </summary>
        /// <returns></returns>
        async Task SaveItem()
        {
            // if has changed? - no check here as we should check before getting here.
            //Item currItem = await AppUnitOfWork.ItemRepository.GetByIdAsync(itemEditing.ItemId);
            //if (currItem == null) // should never happen
            //{
            //    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Item {itemEditing.ItemName} no longer exists in the database.");
            //    return;
            //}
            // make sure the variants are saved.
            if ((displayItemVariants) && (itemVariantsComponent != null))
            {
                var IsSaved = await itemVariantsComponent.SaveItemVariants();
                // move only the item data across, strip the View/Editing extras
                //   ??? - was crashing removed this, seems fixed, weird          AppMapper.Map(itemEditing, currItem);  
                if (!IsSaved)
                { /* should we do something here? */ }
            }
            /////->> need to also update woo settings 

            await appUnitOfWork.ItemRepository.UpdateAsync(itemEditing);
            if (appUnitOfWork.IsInErrorState())
                await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error saving Item {itemEditing.ItemName}.");
            else
            {
                await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Item: {itemEditing.ItemName}, saved.");
                itemHasChanged = false;
            }
        }
        /// <summary>
        ///  Handel the cancel click button
        /// </summary>
        /// <returns>void</returns>
        public async Task SaveItem_ClickAsync()
        {
            if (SomePartOfItemChanged())
                await SaveItem();
            //--> we are going to return to the previous page which fires a location change even so we need to dispose to stop that 
            KillLocationChangeCheck();
            await BackToPreviousPage();
        }
        /// <summary>
        /// If a field is changed handle this 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ItemEditContext_OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            itemHasChanged = true;
        }
        private Guid? SetChangedSelectedGuid(Guid newGuid)
        {
            itemHasChanged = true;
            return newGuid;
        }
        private ItemTypes SetChangedSelectedItemType(ItemTypes newItemType)
        {
            if (newItemType != currentItemType)
            {
                currentItemType = newItemType;
                itemHasChanged = true;
                displayItemVariants = collapseItemVariantsVisible = newItemType == ItemTypes.Variable;  ///--->> move to routine so display error if there are variants and marked as simple
                currentItemType = newItemType;
                StateHasChanged(); //-> should pick up that the type is now variable
            }
            return newItemType;
        }
        private bool SomePartOfItemChanged()
        {
            var result = itemHasChanged || (displayItemVariants && (itemVariantsComponent.ItemVariantHasChanged));
            return result;
        }
        /// <summary>
        /// If the item has been edited then ask the User if they want to save, since they have probably clicked to change page without saving.
        /// </summary>
        void SaveIfWeNeedTo()
        {
            if (SomePartOfItemChanged())
            {
                SaveItem().Wait();
            }
        }
        void ItemEditContext_OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            // Location has changed check if they pressed save r if they want to cancel
            SaveIfWeNeedTo();

            //string navigationMethod = e.IsNavigationIntercepted ? "HTML" : "code";
            //System.Diagnostics.Debug.WriteLine($"Notified of navigation via {navigationMethod} to {e.Location}");
        }
        bool _locationIsKilled = false;
        void KillLocationChangeCheck()
        {
            if (!_locationIsKilled)
            {
                AppNavigationManager.LocationChanged -= ItemEditContext_OnLocationChanged;
                _locationIsKilled = true;
            }
        }
        void IDisposable.Dispose()
        {
            // itemEditContext.OnFieldChanged -= ItemEditContext_OnFieldChanged;
            // Unsubscribe from the event when our component is disposed
            KillLocationChangeCheck();
            GC.SuppressFinalize(this);
            //GC.SuppressFinalize(this);
        }
    }

    #endregion


}

