using AutoMapper;
using Blazorise.RichTextEdit;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Integration.Repositories.Woo;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Items;
using RainbowOF.Tools;
using RainbowOF.Tools.Services;
using RainbowOF.ViewModels.Items;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemEdit : ComponentBase
    {
        #region Injected items
        [Inject]
        private IUnitOfWork appUnitOfWork { get; set; }
        [Inject]
        private ApplicationState appState { get; set; }
        [Inject]
        public ILoggerManager appLoggerManager { get; set; }
        [Inject]
        private IMapper appMapper { get; set; }
        #endregion
        #region Parameters
        [Parameter]
        public string Id { get; set; }
        #endregion
        #region interface and component variables
        public ItemView ItemEditting = null;
        //public List<ItemVariant> ItemVariants = null;
        //public List<ItemCategory> ItemCategories = null;
        //public List<ItemAttribute> ItemAttributes = null;
        //public List<ItemAttributeVariety> itemAttributeVarieties = null
        public PopUpAndLogNotification PopUpRef { get; set; }

        public string SelectedItemTag = "MainDetail";
        public bool collapseItemDetailsVisible = true;
        public bool collapseItemMoreDetailsVisible = true;
        public bool collapseCategoriesVisible = true;
        public bool collapseAttributesVisible = true;
        public bool displayItemVariants = false;
        public bool collapseItemVariantsVisible = true;
        public bool IsItemImportBusy = false;
        public bool DoAutoECommerceSync = false;

        private RichTextEdit richTextEditRef;
        public string LoadingMessage = "Loading item...";
        ConfirmModal ImportConfirmationModal;
        #endregion
        #region private variables
        private Guid CurrentItemId = Guid.Empty;  // set when we get the parameter
        private Guid? LastPrimaryCategoryLookupId = Guid.Empty; // to store the current Primary Category, if changed reload
        class CategoryNode
        {
            public Guid CategoryId { get; set; }
            public string CategoryName { get; set; }
            public bool IsChecked { get; set; } = true;
        }
        private List<CategoryNode> categoryNodes = new();
        #endregion
        //IList<CategoryNode> ExpandedNodes = new List<CategoryNode>();
        //CategoryNode selectedNode;
        #region  Constructor
        protected async override Task OnInitializedAsync()
        {
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("ItemEdit initialising.");
            if (Guid.TryParse(Id, out Guid ItemId))
            {
                if (ItemId != Guid.Empty)
                {
                    await LoadItemFromId(ItemId);
                }
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("ItemEdit initialised.");
            await base.OnInitializedAsync();
        }
        #endregion
        #region Interface routines
        async Task LoadItemFromId(Guid ItemId)
        {
            CurrentItemId = ItemId; // set the Item Id so we can use it from now on.
            Item entity = await appUnitOfWork.itemRepository.FindFirstEagerLoadingItemAsync(it => it.ItemId == ItemId);
            if (entity == null)
            {
                //-> Pop up not init -> PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Item with id {ItemId} not found...");
                LoadingMessage = $"Item with id {ItemId} not found...";
            }
            else
            {
                IRepository<WooProductMap> _wooAttributeMapRepository = appUnitOfWork.Repository<WooProductMap>();
                WooProductMap _wooProductMap = await _wooAttributeMapRepository.GetByIdAsync(wcm => wcm.ItemId == ItemId);
                //  map all the items across to the view then allocate extra woo stuff if exists.
                ItemEditting = new();
                appMapper.Map(entity, ItemEditting);
                LastPrimaryCategoryLookupId = ItemEditting.PrimaryItemCategoryLookupId;
                // if it is an item that has variants then expand the variant accordion and display the Variant selection
                displayItemVariants = collapseItemVariantsVisible = ItemEditting.ItemType == ItemTypes.Variable;
                //-> if it has variants and is not marked as variable we display a message
                if ((!entity.ItemAttributes.Exists(ia => ia.IsUsedForItemVariety)) && (entity.ItemAttributes != null))
                    await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Item: {ItemEditting.ItemName} is not marked as a variable type, but has variants. Change to variable type"); 
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
            bool reload = LastPrimaryCategoryLookupId != currentItemPrimaryCategory;
            if (reload) LastPrimaryCategoryLookupId = currentItemPrimaryCategory;
            List<Item> items = appUnitOfWork.GetListOfSimilarItems(CurrentItemId, currentItemPrimaryCategory, reload);
            return items;
        }
        public async Task OnContentChanged()
        {
            var content = await richTextEditRef.GetHtmlAsync();
            ItemEditting.ItemDetail = content;
        }
        /// <summary>
        /// Handle the button click to re-import an woo item.
        /// Logic: 
        /// 1. Confirm that the item will be over written. Once confirmed the ConfirmImport is run
        public async Task ImportItem_Click()
        {
            await ImportConfirmationModal.ShowModalAsync("This is a confirmation", "Are you sure you import this item, data will de over written?", "Please Confirm", "Cancel");
        }
        public async Task ItemAttributeChangedToVariableAsync(bool IsNowVariable)
        {
            if (IsNowVariable)
            {
                if (ItemEditting.ItemType != ItemTypes.Variable)
                {
                    ItemEditting.ItemType = ItemTypes.Variable;
                }
//                await InvokeAsync(StateHasChanged); //-> should pick up that the type is now variable, or an additional variant has been added
            }
            else
            {
                if (ItemEditting.ItemType != ItemTypes.Simple)
                {
                    // The type for an attribute has changed. But are there any others?
                    bool ItemHasVariableAttributes = await appUnitOfWork.itemRepository.DoesThisItemHaveVariableAttributes(CurrentItemId);
                    if (!ItemHasVariableAttributes)
                        ItemEditting.ItemType = ItemTypes.Simple;
                }
            }
            displayItemVariants = collapseItemVariantsVisible = ItemEditting.ItemType == ItemTypes.Variable;  ///--->> move to routine so display error if there are variants and marked as simple
            await InvokeAsync(StateHasChanged); //-> should pick up that the type is now variable
        }
        /// <summary>
        /// Called by the modal that will return if the user wants to import. 
        /// Logic
        /// 1. If user cancelled then do nothing.
        /// 2. Get the app woo settings.
        /// 2. Use the ItemId to see if the item is linked via the WooMapping to a Woo product, then continue otherwise show message there is no mapping
        /// 3. Using the returned mapping, get the woo product that it is mapped to using ID
        /// 4. and import it.
        /// </summary>
        /// <returns>null</returns>
        async Task ConfirmImport_ClickAsync(bool confirmClicked)
        {
            /// 1. If user cancelled then do nothing.
            if (confirmClicked)
            {
                await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Importing Item form Woo...");
                IsItemImportBusy = true;
                /// 2. Get the app woo settings.
                IRepository<WooSettings> _wooPrefsAppRepository = appUnitOfWork.Repository<WooSettings>();
                WooSettings _wooSettings = await _wooPrefsAppRepository.FindFirstAsync();
                if (_wooSettings == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "No woo settings retrieved. Please check your settings.");
                    IsItemImportBusy = false;
                    return;
                }
                /// 2. Use the ItemId to see if the item is linked via the WooMapping to a Woo product, then continue otherwise show message there is no mapping
                IRepository<WooProductMap> _wooProductMapRepository = appUnitOfWork.Repository<WooProductMap>();
                WooProductMap _wooProductMap = await _wooProductMapRepository.GetByIdAsync(wpm => wpm.ItemId == CurrentItemId);
                if (_wooProductMap == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"No woo product is mapped to item id: {CurrentItemId}.");
                    IsItemImportBusy = false;
                    return;
                }
                /// 3. Using the returned mapping, get the woo product that it is mapped to using ID
                WooAPISettings _wooAPISettings = new WooAPISettings(_wooSettings);
                WooProduct _wooProduct = new WooProduct(_wooAPISettings, appLoggerManager);
                if (_wooProduct == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error creating Woo Product engine.");
                    IsItemImportBusy = false;
                    return;
                }
                Product _product = await _wooProduct.GetProductByIdAsync(_wooProductMap.WooProductId);
                if (_product == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"No product with id {_wooProductMap.WooProductId} found on Woo.");
                    IsItemImportBusy = false;
                    return;
                }

                var _wooImportProduct = new WooImportProduct(appUnitOfWork, appLoggerManager, _wooAPISettings, appMapper);
                /// 4. and import it.
                var _importedId = await _wooImportProduct.ImportAndMapWooEntityDataAsync(_product);
                // abort if there was an error - Or should we log and restart? need to restart DbContext somehow
                if (appUnitOfWork.IsInErrorState())
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error importing product with id {_wooProductMap.WooProductId} found on Woo.");
                else
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Product with id {_wooProductMap.WooProductId} found on Woo and imported.");
                // Reload item.
                await LoadItemFromId(CurrentItemId);
                StateHasChanged();
            }
            IsItemImportBusy = false;
        }
    }
    #endregion


}

