using AutoMapper;
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
        [Inject]
        public IMapper _Mapper { get; set; }
        #endregion
        #region Parameters
        [Parameter]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public List<ItemCategory> SourceItemCategories { get; set; }
        [Parameter]
        public Guid ParentItemId { get; set; }
        //[Parameter]
        //public bool CanUseAsync { get; set; } = true;  // issues with detail mean that if this is a detail grid we disable Async
        #endregion
        #region Initialisation
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            ModelItemCategories = SourceItemCategories;
            // call this now so that it exists in memory
            //await Task.Run(() => { 
                _AppUnitOfWork.GetListOf<ItemCategoryLookup>(true, icl => icl.FullCategoryName); 
            //}); // -> async sorts wrong because field FullCategoryName not in database
            //!! run the init of the GridView here to make sure we display loading while we load.
            _ItemCategoryGridViewRepository = new ItemCategoryGridViewRepository(_Logger, _AppUnitOfWork);
            _ItemCategoryGridViewRepository._GridSettings.PopUpRef = PopUpRef;
            await InvokeAsync(StateHasChanged);
            _Logger.LogDebug("ItemCategoriesComponent initialised.");
        }
        #endregion
        #region BackEnd Routines
        /// <summary>
        ///  Return a list of Attribute Varieties that are available for selection
        ///  Logic:
        ///     1. Get a list of all ids that are already allocated except the current item
        ///     2. set list to be the current item and any options that not already allocated
        /// </summary>
        /// <param name="currentCategoryLookupId">The one that is currently selected</param>
        /// <param name="mustForce">must the list be reloaded (used for refresh etc.)</param>
        /// <returns>List of item attribute varieties to be used.</returns>
        public List<ItemCategoryLookup> GetListOfCategories(Guid? currentCategoryLookupId = null, bool IsForceReload = false)
        {
            List<ItemCategoryLookup> _itemCategorys = _AppUnitOfWork.GetListOf<ItemCategoryLookup>(IsForceReload);
            if (currentCategoryLookupId == null)
                return _itemCategorys;
            if (currentCategoryLookupId == null)
                return _itemCategorys;  // no item selected 
            // 1.
            var _usedItems = ModelItemCategories.Where(mic => (mic.ItemCategoryLookupId != currentCategoryLookupId));
            // 2. 
            var _unselectedItems = _itemCategorys.Where(iav => !_usedItems.Any(miav => (miav.ItemCategoryLookupId == iav.ItemCategoryLookupId)));
            return _unselectedItems.ToList();
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
                PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's Category cannot be blank!");
            }
            else
            {
                PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Adding Category to Item.");
                newItemCategory.ItemCategoryDetail = await _ItemCategoryGridViewRepository.GetItemCategoryByIdAsync(newItemCategory.ItemCategoryLookupId);
                if (newItemCategory.ItemCategoryDetail == null)
                {
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Category in lookup table!");
                }
                else
                {
                    if ((newItemCategory.UoMBaseId != null) && (newItemCategory.UoMBaseId != Guid.Empty))
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
        /// Sets the child list to the items changed, either killing the list or updating it
        /// </summary>
        /// <param name="sourceItemCategory">the source item's category</param>
        /// <returns>updated item Category</returns>
        private async Task<ItemCategory> SetCategoryUoMValues(ItemCategory sourceItemCategory)
        {
            if ((sourceItemCategory.UoMBaseId != null))
            {
                // have they removed the UoM or added item, if it = Guid.Empty it may have been removed?
                if (sourceItemCategory.UoMBaseId == Guid.Empty)
                {
                    if (sourceItemCategory.ItemUoMBase != null)
                    {
                        // if we get here they have removed their UoM, so kill it.
                        sourceItemCategory.UoMBaseId = null;
                        sourceItemCategory.ItemUoMBase = null;
                    }
                }
                else
                {
                    sourceItemCategory.ItemUoMBase = await _ItemCategoryGridViewRepository.GetItemUoMByIdAsync((Guid)sourceItemCategory.UoMBaseId);
                    if (sourceItemCategory.ItemUoMBase == null)
                    {
                        PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Unit of Measure in lookup table!");
                    }
                }
            }
            return sourceItemCategory;
        }
        /// <summary>
        /// Sets the Item's child Category Lookup to the correct data we have in memory
        /// </summary>
        /// <param name="updatedItemCategory">Item category to update, used as source and is modified and returned</param>
        /// <returns>upated item category with updated item</returns>
        private ItemCategory SetCategoryDetail(ItemCategory updatedItemCategory)
        {
            if (updatedItemCategory.ItemCategoryLookupId == Guid.Empty)
            {
                updatedItemCategory.ItemCategoryDetail = null; /// force it null, cannot dispose?
            }
            else
            {
                var _ItemCats = GetListOfCategories();
                updatedItemCategory.ItemCategoryDetail = _ItemCats.FirstOrDefault(ic => ic.ItemCategoryLookupId == updatedItemCategory.ItemCategoryLookupId);
            }
            return updatedItemCategory;
        }
        //async Task OnRowUpdatingAsync(CancellableRowChange<ItemCategory, Dictionary<string, object>> updatingItem) 
        /// <summary>
        /// Handle the grid update
        /// </summary>
        /// <param name="updatedItem">What the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowUpdatedAsync(SavedRowItem<ItemCategory, Dictionary<string, object>> updatedItem)
        {
            var _updatedItemCategory = updatedItem.Item;
            if (_updatedItemCategory.ItemCategoryDetail == null)
            {
                PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's Category cannot be blank...");
            }
            else
            {
                PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Updating Category in Item.");
                var _currentItemCategory = await _ItemCategoryGridViewRepository.GetEntityByIdAsync(_updatedItemCategory.ItemCategoryId);
                if (_currentItemCategory == null)
                {
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error updating category, it could not be found in the table.");
                }
                else
                {
                    _updatedItemCategory = await SetCategoryUoMValues(_updatedItemCategory);
                    _updatedItemCategory = SetCategoryDetail(_updatedItemCategory);
                    _Mapper.Map(_updatedItemCategory, _currentItemCategory);
                    var _result = await _ItemCategoryGridViewRepository.UpdateViewRowAsync(_currentItemCategory, _currentItemCategory.ItemCategoryDetail.CategoryName);
                    if (_AppUnitOfWork.IsInErrorState() || (_result <= 0))
                        PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating category {_AppUnitOfWork.GetErrorMessage()}, it could not be found in the table.");
                    //else
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category {_updatedItemCategory.ItemCategoryDetail.CategoryName}, updated.");
                }
            }
            //await _DataGrid.Reload();
            StateHasChanged();
        }
        /*   Old update code before using mapper.
//_updatedItemCategory.ItemCategoryDetail = await _ItemCategoryGridViewRepository.GetItemCategoryByIdAsync(_updatedItemCategory.ItemCategoryLookupId);
//if (_updatedItemCategory.ItemCategoryDetail == null)
//{
//    _ItemCategoryGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Category in lookup table!");
//}
//else
//{
//    if ((_updatedItemCategory.UoMBaseId != null))
//    {
//        // have they removed the UoM or added item, if it = Guid.Empty it may have been removed?
//        if (_updatedItemCategory.UoMBaseId == Guid.Empty)
//        {
//            if (_updatedItemCategory.ItemUoMBase != null)
//            {
//                // if we get here they have removed their UoM, so kill it.
//                _updatedItemCategory.UoMBaseId = null;
//                _updatedItemCategory.ItemUoMBase = null;
//            }
//        }
//        else
//        {
//            _updatedItemCategory.ItemUoMBase = await _ItemCategoryGridViewRepository.GetItemUoMByIdAsync((Guid)_updatedItemCategory.UoMBaseId);
//            if (_updatedItemCategory.ItemUoMBase == null)
//            {
//                PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Unit of Measure in lookup table!");
//            }
//        }
//    }
//ItemCategory _currentItemCategory = await _ItemCategoryGridViewRepository.GetEntityByIdAsync(_updatedItemCategory.ItemCategoryId);
//if (_currentItemCategory != null)
//{
//    _currentItemCategory = _updatedItemCategory;
//var _result = await _ItemCategoryGridViewRepository.UpdateViewRowAsync(_updatedItemCategory, _updatedItemCategory.ItemCategoryDetail.CategoryName);
       var _result = await _ItemCategoryGridViewRepository.UpdateViewRowAsync(_currentItemCategory, _currentItemCategory.ItemCategoryDetail.CategoryName);
       //}
*/

        async Task OnRowRemovingAsync(CancellableRowChange<ItemCategory> modelItem)
        {
            // set the Selected Item Attribute for use later
            SeletectedItemCategory = modelItem.Item;
            var deleteItem = modelItem;
            await _ItemCategoryGridViewRepository._GridSettings.DeleteConfirmation.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemCategoryDetail.CategoryName}?");  //,"Delete","Cancel"); - passed in on init
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
