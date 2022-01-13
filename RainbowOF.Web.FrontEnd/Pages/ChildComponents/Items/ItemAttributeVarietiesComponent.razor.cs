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
    public partial class ItemAttributeVarietiesComponent : ComponentBase
    {
        #region Private vars
        #endregion
        #region Grid Variables
        // Interface Stuff
        List<ItemAttributeVariety> ModelItemAttributeVarieties = null;
        ItemAttributeVariety SeletectedItemAttributeVariety;
        DataGrid<ItemAttributeVariety> _DataGrid;
        // All there workings are here
        IItemAttributeVarietyGridViewRepository _ItemAttributeVarietyGridViewRepository = null;
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
        [EditorRequired]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public ItemAttribute SourceItemAttribute { get; set; }
        //[Parameter]
        //public bool CanUseAsync { get; set; } = true;  // issues with detail mean that if this is a detail grid we disable Async
        #endregion
        #region Initialisation
        protected override async Task OnInitializedAsync()
        {
            _Logger.LogDebug("ItemAttributeVarietiesComponent initialising...");
            await base.OnInitializedAsync();
            ModelItemAttributeVarieties = await LoadItemAttributeVariables(SourceItemAttribute.ItemAttributeId);   // SourceItemAttribute.ItemAttributeVarieties?? new();
            // call this now so that it exists in memory
            await Task.Run(() => { _AppUnitOfWork.GetListOfAttributeVarieties(SourceItemAttribute.ItemAttributeLookupId, true); }); //
            //!! Init grid vars after all async's
            _ItemAttributeVarietyGridViewRepository = new ItemAttributeVarietyGridViewRepository(_Logger, _AppUnitOfWork);
            _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef = PopUpRef;
            //            _ItemAttributeVarietyGridViewRepository._GridSettings.DeleteConfirmation = DeleteConfirmation;  //---> needs to be local otherwise delete the wrong thing
            await InvokeAsync(StateHasChanged);
            _Logger.LogDebug("ItemAttributeVarietiesComponent initialised...");
        }
        #endregion
        #region BackEnd Routines
        /// <summary>
        /// Retrieve a list of an Items Attribute Variations using the source Item Attribute Id
        /// </summary>
        /// <param name="sourceItemAttributeParentId">the Item's ItemAttributeId for this attribute</param>
        /// <returns>List of ItemAttributeVarieties associated to the Parent ItemAttributeId </returns>
        private async Task<List<ItemAttributeVariety>> LoadItemAttributeVariables(Guid sourceItemAttributeParentId)
        {
            IAppRepository<ItemAttributeVariety> _itemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVariety>();
            var _result = await _itemAttributeVarietyRepository.GetByAsync(iav => iav.ItemAttributeId == sourceItemAttributeParentId);
            return _result.ToList();
        }
        /// <summary>
        /// Force a reload of the grid using the parameters we have
        /// </summary>
        /// <returns></returns>
        private async Task ReloadDataInGrid()
        {
            if (ModelItemAttributeVarieties != null)
            {
                ModelItemAttributeVarieties.Clear(); // would prefer to dispose, but at least kill it
            }
            ModelItemAttributeVarieties = await LoadItemAttributeVariables(SourceItemAttribute.ItemAttributeId);
            await InvokeAsync(StateHasChanged);
        }
        /// <summary>
        ///  Return a list of Attribute Varieties that are available for selection
        ///  Logic:
        ///     1. Get a list of all ids that are already allocated except the current item
        ///     2. set list to be the current item and any options that not already allocated
        /// </summary>
        /// <param name="parentAttributeLookupId">The parent Attribute for these variants</param>
        /// <param name="currentAttributeVarityLookupId">The one that is currently selected</param>
        /// <param name="mustForce">must the list be reloaded (used for refresh etc.)</param>
        /// <returns>List of item attribute varieties to be used.</returns>
        public List<ItemAttributeVarietyLookup> GetListOfAttributeVarieties(Guid parentAttributeLookupId,
                                                                            Guid currentAttributeVarityLookupId,
                                                                            bool mustForce = false)
        {
            List<ItemAttributeVarietyLookup> _itemAttributeVarietyLookups = _AppUnitOfWork.GetListOfAttributeVarieties(parentAttributeLookupId, mustForce);
            // 1.
            var _usedItems = ModelItemAttributeVarieties.Where(miav => (miav.ItemAttributeVarietyLookupId != currentAttributeVarityLookupId));
            // 2. 
            var _unselectedItems = _itemAttributeVarietyLookups.Where(iav => !_usedItems.Any(miav => (miav.ItemAttributeVarietyLookupId == iav.ItemAttributeVarietyLookupId)));
            return _unselectedItems.ToList();
        }
#region oldcode that uses a loop

//List<ItemAttributeVarietyLookup> _unselectedItems = new List<ItemAttributeVarietyLookup>(_itemAttributeVarietyLookups); // clone this list
//// manually delete those we have besides the one we re on
//foreach (var _availableVarietty in _itemAttributeVarietyLookups)
//{
//    foreach (var _modelItem in ModelItemAttributeVarieties)
//    {
//        if ((_availableVarietty.ItemAttributeVarietyLookupId != currentAttributeVarityLookupId) &&(_availableVarietty.ItemAttributeVarietyLookupId == _modelItem.ItemAttributeVarietyLookupId))
//        {
//            _unselectedItems.Remove(_availableVarietty);
//        }
//    }
//}

//var result = peopleList2.Where(p => !peopleList1.Any(p2 => p2.ID == p.ID));
#endregion

        // Interface Stuff
        void OnNewItemAttributeVarietyDefaultSetter(ItemAttributeVariety newItem)
        {
            newItem = _ItemAttributeVarietyGridViewRepository.NewViewEntityDefaultSetter(newItem, SourceItemAttribute.ItemAttributeId);
        }
        /// <summary>
        /// Handle the grid insert 
        /// </summary>
        /// <param name="insertedItemAttributeVariety">what the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowInsertingAsync(SavedRowItem<ItemAttributeVariety, Dictionary<string, object>> insertedItemAttributeVariety)
        {
            var newItemAttributeVariety = insertedItemAttributeVariety.Item;
            if (newItemAttributeVariety.ItemAttributeVarietyLookupId == Guid.Empty)
            {
                _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's attribute variety cannot be blank!");
            }
            else
            {
                _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Adding attribute variety to item.");
                newItemAttributeVariety.ItemAttributeVarietyDetail = await _ItemAttributeVarietyGridViewRepository.GetItemAttributeVarietyByIdAsync(newItemAttributeVariety.ItemAttributeVarietyLookupId);
                if (newItemAttributeVariety.ItemAttributeVarietyDetail == null)
                {
                    _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding attribute variety in lookup table!");
                }
                else
                {
                    //if ((newItemAttributeVariety.UoMId != null) && (newItemAttributeVariety.UoMId != Guid.Empty))
                    //{
                    //    newItemAttributeVariety.UoM = await _ItemAttributeVarietyGridViewRepository.GetItemUoMByIdAsync((Guid)newItemAttributeVariety.UoMId);
                    //    if (newItemAttributeVariety.UoM == null)
                    //    {
                    //        _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Unit of Measure in lookup table!");
                    //    }
                    //}
                    await _ItemAttributeVarietyGridViewRepository.InsertViewRowAsync(newItemAttributeVariety, "Attribute variety");

                }
            }
            // await _DataGrid.Reload();
            await ReloadDataInGrid();
        }
        /// <summary>
        /// Handle the grid update
        /// </summary>
        /// <param name="updatedItem">What the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowUpdatedAsync(SavedRowItem<ItemAttributeVariety, Dictionary<string, object>> updatedItem)
        {
            var _updatedItemAttributeVariety = updatedItem.Item;
            if (_updatedItemAttributeVariety.ItemAttributeVarietyDetail == null)
            {
                _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's attribute variety cannot be blank...");
            }
            else
            {
                // load item for database to see it exists, could later add a check using row version to see if it has changed.
                _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Updating attribute variety in item.");
                var _currentItemAttributeVariety = await _ItemAttributeVarietyGridViewRepository.GetEntityByIdAsync(_updatedItemAttributeVariety.ItemAttributeVarietyId);
                if (_currentItemAttributeVariety == null)
                {
                    _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error updating attribute variety, it could not be found in the table.");
                }
                else
                {
                    //_updatedItemAttributeVariety = await SetAttributeVarietyUoMValues(_updatedItemAttributeVariety);
                    _Mapper.Map(_updatedItemAttributeVariety, _currentItemAttributeVariety);
                    var _result = await _ItemAttributeVarietyGridViewRepository.UpdateViewRowAsync(_currentItemAttributeVariety, _currentItemAttributeVariety.ItemAttributeVarietyDetail.VarietyName);
                    if (_AppUnitOfWork.IsInErrorState())
                    {
                        _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating attribute variety {_AppUnitOfWork.GetErrorMessage()}, it could not be found in the table.");
                    }
                    // update in memory item to reflect changes, done after saving, we could read it back from the database, but since we have the stuff in memory we update it here after the db update.
                    _updatedItemAttributeVariety = SetAttributeVarietyDetail(_updatedItemAttributeVariety);
                }
            }
            //await _DataGrid.Reload();
            await ReloadDataInGrid();
            //StateHasChanged();
        }
        /// <summary>
        /// Sets the Item's child Attribute Variety to the correct data we have in memory
        /// </summary>
        /// <param name="updatedItemAttributeVariety">Item category to update, used as source and is modified and returned</param>
        /// <returns>updated item category with updated item</returns>
        private ItemAttributeVariety SetAttributeVarietyDetail(ItemAttributeVariety updatedItemAttributeVariety)
        {
            if (updatedItemAttributeVariety.ItemAttributeVarietyLookupId == Guid.Empty)
            {
                updatedItemAttributeVariety.ItemAttributeVarietyDetail = null; /// force it null, cannot dispose?
            }
            else
            {
                var _ItemAtts = _AppUnitOfWork.GetListOfAttributeVarieties(SourceItemAttribute.ItemAttributeLookupId);
                updatedItemAttributeVariety.ItemAttributeVarietyDetail = _ItemAtts.FirstOrDefault(ic => ic.ItemAttributeVarietyLookupId == updatedItemAttributeVariety.ItemAttributeVarietyLookupId);
            }
            return updatedItemAttributeVariety;
        }

        async Task OnRowRemovingAsync(CancellableRowChange<ItemAttributeVariety> modelItem)
        {
            // set the Selected Item AttributeVariety for use later
            SeletectedItemAttributeVariety = modelItem.Item;
            var deleteItem = modelItem;
            await _ItemAttributeVarietyGridViewRepository._GridSettings.DeleteConfirmation.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemAttributeVarietyDetail.VarietyName}?");  //,"Delete","Cancel"); - passed in on init
            await ReloadDataInGrid();
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
                await _ItemAttributeVarietyGridViewRepository.DeleteViewRowByIdAsync(SeletectedItemAttributeVariety.ItemAttributeVarietyId, SeletectedItemAttributeVariety.ItemAttributeVarietyDetail.VarietyName);
            }
            //IsLoading = false;
            // await _DataGrid.Reload();
            await ReloadDataInGrid();
        }
        public async Task OnRowRemovedAsync(ItemAttributeVariety modelItem)
        {
            //            await _DataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await ReloadDataInGrid();
            //            await InvokeAsync(StateHasChanged);
        }


        //async Task<List<ItemAttributeVariety>> GetItemAttributeVarietiessAsync(Guid? sourceParentId, bool IsAsyncCall = true)
        //{
        //    if (IsBusy) return null;
        //    IsBusy = true;
        //    var repo = _AppUnitOfWork.itemAttributeVarietiesRepository();

        //    List<ItemAttributeVarieties> _gotItemAttributeVarietiess = null;

        //    if (IsAsyncCall)
        //        _gotItemAttributeVarietiess = (await repo.GetByAsync(icl => icl.ParentAttributeVarietyId == sourceParentId)).ToList();
        //    else
        //        _gotItemAttributeVarietiess = repo.GetBy(icl => icl.ParentAttributeVarietyId == sourceParentId).ToList();
        //    //await InvokeAsync(StateHasChanged);

        //    IsBusy = false;
        //    return _gotItemAttributeVarietiess;
        //}
        #endregion
    }
}
