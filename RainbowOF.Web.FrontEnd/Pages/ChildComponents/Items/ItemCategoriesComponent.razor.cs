using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Items;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Items
{
    public partial class ItemCategoriesComponent : ComponentBase
    {
        #region Grid Variables
        // Interface Stuff
        List<ItemCategory> ModelItemCategories;
        ItemCategory SeletectedItemCategory;
        DataGrid<ItemCategory> _DataGrid;
        // All there workings are here
        IItemCategoryGridViewRepository _ItemCategoryGridViewRepository = null;
        #endregion
        #region Injected Variables
        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        public ILoggerManager _Logger { get; set; }
        #endregion
        #region Parameters
        [Parameter]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public List<ItemCategory> SourceItemCategories { get; set; }
        [Parameter]
        public Guid ParentItemId {get;set; }
        //[Parameter]
        //public bool CanUseAsync { get; set; } = true;  // issues with detail mean that if this is a detail grid we disable Async
        #endregion
        #region Initialisation
        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            _ItemCategoryGridViewRepository = new ItemCategoryGridViewRepository(_Logger, _AppUnitOfWork);
            ModelItemCategories = SourceItemCategories;
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region BackEnd Routines
        private Dictionary<Guid, string> _listOfCategories = null;
        public Dictionary<Guid, string> GetListOfCategories(bool mustForce = false)
        {
            if ((mustForce) || (_listOfCategories == null))
            {
                if (_listOfCategories != null) _listOfCategories.Clear();
                else _listOfCategories = new Dictionary<Guid, string>();

                IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
                var _itemCategories = _itemCategoryLookupRepository.GetAll()
                    .OrderBy(ic => ic.FullCategoryName)
                    .ToList();  // cannot async as part of UI for
                foreach (var _itemCategory in _itemCategories)
                {
                    _listOfCategories.Add(_itemCategory.ItemCategoryLookupId, _itemCategory.FullCategoryName);
                }
            }
            return _listOfCategories;
        }
        // Interface Stuff
        void OnNewItemCategoryDefaultSetter(ItemCategory newItem)
        {
            newItem = _ItemCategoryGridViewRepository.NewViewEntityDefaultSetter(newItem, ParentItemId);
        }

        /// <summary>
        /// Handle the grid insert 
        /// </summary>
        /// <param name="insertedItemCategory">what the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowInsertingAsync(SavedRowItem<ItemCategory, Dictionary<string, object>> insertedItemCategory)
        {
            var newItemCategory = insertedItemCategory.Item;
            if (newItemCategory.ItemCategoryLookupId == Guid.Empty)
            {
                _ItemCategoryGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's Category cannot be blank!");
            }
            else
            {
                _ItemCategoryGridViewRepository._GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Adding Category to Item.");
                newItemCategory.ItemCategoryDetail = await _ItemCategoryGridViewRepository.GetItemCategoryByIdAsync(newItemCategory.ItemCategoryLookupId);
                if (newItemCategory.ItemCategoryDetail == null)
                {
                    _ItemCategoryGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Category in lookup table!");
                }
                else
                {
                    if ((newItemCategory.UoMBaseId!= null)&& (newItemCategory.UoMBaseId != Guid.Empty))
                    {
                        newItemCategory.ItemUoMBase = await _ItemCategoryGridViewRepository.GetItemUoMByIdAsync((Guid)newItemCategory.UoMBaseId);
                        if (newItemCategory.ItemUoMBase == null)
                        {
                            PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Unit of Measure in lookup table!");
                        }
                    }
                    await _ItemCategoryGridViewRepository.InsertViewRowAsync(newItemCategory, "Category");
                }
            }
            await _DataGrid.Reload();
        }
        /// <summary>
        /// Handle the grid update
        /// </summary>
        /// <param name="updatedItem">What the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowUpdatingAsync(SavedRowItem<ItemCategory, Dictionary<string, object>> updatedItem)
        {
            var updateItemCategory = updatedItem.Item;
            if (updateItemCategory.ItemCategoryDetail == null)
            {
                PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's Category cannot be blank...");
            }
            else
            {
                _ItemCategoryGridViewRepository._GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Updating Category in Item.");
                updateItemCategory.ItemCategoryDetail = await _ItemCategoryGridViewRepository.GetItemCategoryByIdAsync(updateItemCategory.ItemCategoryLookupId);
                if (updateItemCategory.ItemCategoryDetail == null)
                {
                    _ItemCategoryGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Category in lookup table!");
                }
                else
                {
                    if ((updateItemCategory.UoMBaseId != null) && (updateItemCategory.UoMBaseId != Guid.Empty))
                    {
                        updateItemCategory.ItemUoMBase = await _ItemCategoryGridViewRepository.GetItemUoMByIdAsync((Guid)updateItemCategory.UoMBaseId);
                        if (updateItemCategory.ItemUoMBase == null)
                        {
                            PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Unit of Measure in lookup table!");
                        }
                    }
                    await _ItemCategoryGridViewRepository.UpdateViewRowAsync(updatedItem.Item, updatedItem.Item.ItemCategoryDetail.CategoryName);
                }
            }
            await _DataGrid.Reload();
        }
        void OnRowRemoving(CancellableRowChange<ItemCategory> modelItem)
        {
            // set the Selected Item Attribute for use later
            SeletectedItemCategory = modelItem.Item;
            var deleteItem = modelItem;
            _ItemCategoryGridViewRepository._GridSettings.DeleteConfirmation.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemCategoryDetail.CategoryName}?");  //,"Delete","Cancel"); - passed in on init
        }
        /// <summary>
        /// Confirm Delete Click is called when the user confirms they want to delete.
        /// </summary>
        /// <param name="confirmationOption">confirmation option by the user selection</param>
        /// <returns></returns>
        async Task ConfirmDelete_ClickAsync(bool confirmationOption)
        {
            if (confirmationOption)
            {
                await _ItemCategoryGridViewRepository.DeleteViewRowByIdAsync(SeletectedItemCategory.ItemCategoryId, SeletectedItemCategory.ItemCategoryDetail.CategoryName);
            }
            //IsLoading = false;
            await _DataGrid.Reload();
        }
        public async Task OnRowRemovedAsync(ItemCategory modelItem)
        {
            await _DataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
        }


        //async Task<List<ItemCategory>> GetItemCategoriessAsync(Guid? sourceParentId, bool IsAsyncCall = true)
        //{
        //    if (IsBusy) return null;
        //    IsBusy = true;
        //    var repo = _AppUnitOfWork.itemCategoriesRepository();

        //    List<ItemCategories> _gotItemCategoriess = null;

        //    if (IsAsyncCall)
        //        _gotItemCategoriess = (await repo.GetByAsync(icl => icl.ParentCategoryId == sourceParentId)).ToList();
        //    else
        //        _gotItemCategoriess = repo.GetBy(icl => icl.ParentCategoryId == sourceParentId).ToList();
        //    //await InvokeAsync(StateHasChanged);

        //    IsBusy = false;
        //    return _gotItemCategoriess;
        //}
        #endregion
    }
}
