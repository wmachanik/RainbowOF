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
        #region Grid Variables
        // Interface Stuff
        List<ItemAttributeVariety> ModelItemAttributeVarieties;
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
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public ItemAttribute SourceItemAttribute { get; set; }
        //[Parameter]
        //public bool CanUseAsync { get; set; } = true;  // issues with detail mean that if this is a detail grid we disable Async
        #endregion
        #region Initialisation
        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            _ItemAttributeVarietyGridViewRepository = new ItemAttributeVarietyGridViewRepository(_Logger, _AppUnitOfWork);
            _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef = PopUpRef;
            ModelItemAttributeVarieties = SourceItemAttribute.ItemAttributeVarieties;
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region BackEnd Routines
        public Dictionary<Guid, string> GetListOfAttributeVarieties(Guid parentAttributeLookupId, bool mustForce = false)
            => _AppUnitOfWork.GetListOfAttributeVarieties(parentAttributeLookupId, mustForce);
        // Interface Stuff
        void OnNewItemAttributeVarietyDefaultSetter(ItemAttributeVariety newItem)
        {
            newItem = _ItemAttributeVarietyGridViewRepository.NewViewEntityDefaultSetter(newItem, SourceItemAttribute.ItemId);
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
                    if ((newItemAttributeVariety.UoMId != null) && (newItemAttributeVariety.UoMId != Guid.Empty))
                    {
                        newItemAttributeVariety.UoM = await _ItemAttributeVarietyGridViewRepository.GetItemUoMByIdAsync((Guid)newItemAttributeVariety.UoMId);
                        if (newItemAttributeVariety.UoM == null)
                        {
                            _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Unit of Measure in lookup table!");
                        }
                    }
                    await _ItemAttributeVarietyGridViewRepository.InsertViewRowAsync(newItemAttributeVariety, "Attribute variety");
                }
            }
            await _DataGrid.Reload();
        }
        /// <summary>
        /// Handle the grid update
        /// </summary>
        /// <param name="updatedItem">What the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowUpdatingAsync(SavedRowItem<ItemAttributeVariety, Dictionary<string, object>> updatedItem)
        {
            var _updatedItemAttributeVariety = updatedItem.Item;
            if (_updatedItemAttributeVariety.ItemAttributeVarietyDetail == null)
            {
                _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's AttributeVariety cannot be blank...");
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
                    _Mapper.Map(_updatedItemAttributeVariety, _currentItemAttributeVariety);
                    if (_AppUnitOfWork.IsInErrorState())
                    {
                        _ItemAttributeVarietyGridViewRepository._GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating attribute variety {_AppUnitOfWork.GetErrorMessage()}, it could not be found in the table.");
                    }
                }
            }
            await _DataGrid.Reload();
        }
        void OnRowRemoving(CancellableRowChange<ItemAttributeVariety> modelItem)
        {
            // set the Selected Item AttributeVariety for use later
            SeletectedItemAttributeVariety = modelItem.Item;
            var deleteItem = modelItem;
            _ItemAttributeVarietyGridViewRepository._GridSettings.DeleteConfirmation.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemAttributeVarietyDetail.VarietyName}?");  //,"Delete","Cancel"); - passed in on init
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
            await _DataGrid.Reload();
        }
        public async Task OnRowRemovedAsync(ItemAttributeVariety modelItem)
        {
            await _DataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
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
