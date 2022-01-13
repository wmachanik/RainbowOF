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
        private IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        private ApplicationState _AppState { get; set; }
        [Inject]
        public ILoggerManager _Logger { get; set; }
        [Inject]
        private IMapper _Mapper { get; set; }
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
            _Logger.LogDebug("ItemEdit initialising.");
            if (Guid.TryParse(Id, out Guid ItemId))
            {
                if (ItemId != Guid.Empty)
                {
                    await LoadItemFromId(ItemId);
                }
            }
            _Logger.LogDebug("ItemEdit initialised.");
        }
        #endregion
        #region Interface routines
        async Task LoadItemFromId(Guid ItemId)
        {
            CurrentItemId = ItemId; // set the Item Id so we can use it from now on.

            IItemRepository _itemRepository = _AppUnitOfWork.itemRepository();
            Item entity = await _itemRepository.FindFirstEagerLoadingItemAsync(it => it.ItemId == ItemId);
            if (entity == null)
            {
                //-> Pop up not init -> PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Item with id {ItemId} not found...");
                LoadingMessage = $"Item with id {ItemId} not found...";
            }
            else
            {
                IAppRepository<WooProductMap> _wooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductMap>();
                WooProductMap _wooProductMap = await _wooAttributeMapRepository.FindFirstByAsync(wcm => wcm.ItemId == ItemId);
                //  map all the items across to the view then allocate extra woo stuff if exists.
                ItemEditting = new();
                _Mapper.Map(entity, ItemEditting);

                // if it is an item that has variants then expand the variant accordion and display the Variant selection
                displayItemVariants = collapseItemVariantsVisible = ((entity.ItemAttributes != null) && (entity.ItemAttributes.Exists(ia => ia.IsUsedForItemVariety)));
            }
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
                PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Info, $"Importing Item form Woo...");
                IsItemImportBusy = true;
                /// 2. Get the app woo settings.
                IAppRepository<WooSettings> _wooPrefsAppRepository = _AppUnitOfWork.Repository<WooSettings>();
                WooSettings _wooSettings = await _wooPrefsAppRepository.FindFirstAsync();
                if (_wooSettings == null)
                {
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "No woo settings retrieved. Please check your settings.");
                    IsItemImportBusy = false;
                    return;
                }
                /// 2. Use the ItemId to see if the item is linked via the WooMapping to a Woo product, then continue otherwise show message there is no mapping
                IAppRepository<WooProductMap> _wooProductMapRepository = _AppUnitOfWork.Repository<WooProductMap>();
                WooProductMap _wooProductMap = await _wooProductMapRepository.FindFirstByAsync(wpm => wpm.ItemId == CurrentItemId);
                if (_wooProductMap == null)
                {
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"No woo product is mapped to item id: {CurrentItemId}.");
                    IsItemImportBusy = false;
                    return;
                }
                /// 3. Using the returned mapping, get the woo product that it is mapped to using ID
                WooAPISettings _wooAPISettings = new WooAPISettings(_wooSettings);
                WooProduct _wooProduct = new WooProduct(_wooAPISettings, _Logger);
                if (_wooProduct == null)
                {
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error creating Woo Product engine.");
                    IsItemImportBusy = false;
                    return;
                }
                Product _product = await _wooProduct.GetProductByIdAsync(_wooProductMap.WooProductId);
                if (_product == null)
                {
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"No product with id {_wooProductMap.WooProductId} found on Woo.");
                    IsItemImportBusy = false;
                    return;
                }

                var _wooImportProduct = new WooImportProduct(_AppUnitOfWork, _Logger, _wooAPISettings, _Mapper);
                /// 4. and import it.
                var _importedId = await _wooImportProduct.ImportAndMapWooEntityDataAsync(_product);
                // abort if there was an error - Or should we log and restart? need to restart DbContext somehow
                if (_AppUnitOfWork.IsInErrorState())
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error importing product with id {_wooProductMap.WooProductId} found on Woo.");
                else
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Product with id {_wooProductMap.WooProductId} found on Woo and imported.");
                // Reload item.
                await LoadItemFromId(CurrentItemId);
                StateHasChanged();
            }
            IsItemImportBusy = false;
        }
    }
    #endregion
    /*
            //ItemEditting.ItemName = entity.ItemName;
            //ItemEditting.SKU = entity.SKU;
            //ItemEditting.IsEnabled = entity.IsEnabled;
            //ItemEditting.ItemDetail = entity.ItemDetail;
            //ItemEditting.PrimaryItemCategoryLookupId = ((entity.PrimaryItemCategoryLookupId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.PrimaryItemCategoryLookupId;
            //ItemEditting.ParentItemId = ((entity.ParentItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.ParentItemId;
            //ItemEditting.ReplacementItemId = ((entity.ReplacementItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.ReplacementItemId;
            //ItemEditting.ItemAbbreviatedName = entity.ItemAbbreviatedName;
            //ItemEditting.ParentItem = entity.ParentItem;
            //ItemEditting.ReplacementItem = entity.ReplacementItem;
            //ItemEditting.ItemCategories = entity.ItemCategories;
            //ItemEditting.ItemAttributes = entity.ItemAttributes;
            //ItemEditting.ItemImages = entity.ItemImages;              
            //ItemEditting.SortOrder = entity.SortOrder;
            //ItemEditting.BasePrice = entity.BasePrice;
            //ItemEditting.ManageStock = entity.ManageStock;
            //ItemEditting.QtyInStock = entity.QtyInStock;
            //ItemEditting.CanUpdateECommerceMap = (_wooProductMap == null) ? null : _wooProductMap.CanUpdate;
            //                    IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            //                    var _itemcats = (await _itemCategoryLookupRepository.GetAllAsync()).ToList().OrderBy(ic=>ic.FullCategoryName); // forces the parents at the top
            //                    bool _isChecked;

            ////var BunchOfCats = _itemcats.Select(c => new { c.ItemCategoryLookupId, c.CategoryName}).ToList();

            //                    if (ItemEditting.ItemCategories != null)
            //                    {
            //                        categoryNodes = new();
            //                        foreach (var itemCat in _itemcats)
            //                        {
            //                            _isChecked = (entity.ItemCategories != null) && (entity.ItemCategories.Exists(ic => ic.ItemCategoryLookupId == itemCat.ItemCategoryLookupId));
            //                            CategoryNode _categoryNode= new CategoryNode
            //                            {
            //                                CategoryId = itemCat.ItemCategoryLookupId,
            //                                CategoryName = itemCat.CategoryName,
            //                                IsChecked = _isChecked,
            //                                ChildrenCategories = null
            //                            };
            //                            if (itemCat.ParentCategoryId == null)
            //                            {
            //                                categoryNodes.Add(_categoryNode);
            //                            }
            //                            else   //(itemCat.ItemCategoryDetail.ParentCategoryId != null)
            //                            {
            //                                var allcats = categoryNodes.Select
            //                                var catNode = categoryNodes.Find(cn => cn.CategoryId == itemCat.ParentCategoryId);
            //                                if (catNode == null)
            //                                {

            //                                    catNode = categoryNodes.Find(cn => cn.ChildrenCategories.Find(ccn=> ccn.CategoryId == itemCat.ParentCategoryId).CategoryId == itemCat.ParentCategoryId);// find the child node
            //                                    if (catNode != null) catNode = catNode.ChildrenCategories.Find(cn => cn.CategoryId == itemCat.ParentCategoryId);  // get the child
            //                                }
            //                                if (catNode != null)
            //                                {
            //                                    if (catNode.ChildrenCategories == null) catNode.ChildrenCategories = new();
            //                                    catNode.ChildrenCategories.Add(_categoryNode);
            //                                }
            //                            }
            //                        }
            //                        // ExpandedNodes.Concat(categoryNodes);
            //                    }

    */


}

