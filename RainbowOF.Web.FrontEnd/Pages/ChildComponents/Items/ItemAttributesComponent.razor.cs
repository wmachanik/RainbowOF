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
    public partial class ItemAttributesComponent : ComponentBase
    {
        #region Grid Variables
        // Interface Stuff
        List<ItemAttribute> ModelItemAttributes;
        ItemAttribute SeletectedItemAttribute;
        DataGrid<ItemAttribute> _DataGrid;
        // All there workings are here
        IItemAttributeGridRepository itemAttributeGridRepository = null;
        #endregion
        #region Injected Variables
        [Inject]
        IUnitOfWork appUnitOfWork { get; set; }
        [Inject]
        public ILoggerManager appLoggerManager { get; set; }
        [Inject]
        public IMapper _Mapper { get; set; }
        #endregion
        #region Parameters
        [Parameter, EditorRequired]
        public PopUpAndLogNotification PopUpRef { get; set; }
        //[Parameter,EditorRequired]
        [Parameter, EditorRequired]
        public Guid ParentItemId { get; set; }
        [Parameter, EditorRequired]
        public EventCallback<bool> OnAttributeChangeToVariableEvent { get; set; }
        //[Parameter]
        //public bool CanUseAsync { get; set; } = true;  // issues with detail mean that if this is a detail grid we disable Async
        #endregion
        #region Initialisation
        protected override async Task OnInitializedAsync()
        {
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("ItemAttributesComponent initialising.");
            ModelItemAttributes = await appUnitOfWork.itemRepository.GetEagerItemAttributeByItemIdAsync(ParentItemId);
            // load lookup here, force reload as could be new item    - async order by failing again, not sure why tried await task also has a error
            //await Task.Run(() =>
            //{
            appUnitOfWork.GetListOf<ItemAttributeLookup>(true, ial => ial.AttributeName);
            //});
            //!!! This must be done last so that loading is displayed until we are finished loading.
            itemAttributeGridRepository = new ItemAttributeGridRepository(appLoggerManager, appUnitOfWork);
            itemAttributeGridRepository.gridSettings.PopUpRef = PopUpRef;   // use the forms Pop Up ref
            await base.OnInitializedAsync();
            //await InvokeAsync(StateHasChanged);
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("ItemAttributesComponent initialised.");
        }
        #endregion
        #region BackEnd Routines
        /// <summary>
        ///  Return a list of Attributes that are available for selection
        ///  Logic:
        ///     1. Get a list of all ids that are already allocated except the current item
        ///     2. set list to be the current item and any options that not already allocated
        /// </summary>
        /// <param name="currentAttributeLookupId">The one that is currently selected</param>
        /// <param name="mustForce">must the list be reloaded (used for refresh etc.)</param>
        /// <returns>List of item attribute varieties to be used.</returns>
        public List<ItemAttributeLookup> GetListOfAvailableAttributes(Guid currentAttributeLookupId,
                                                                      bool IsForceReload = false)
        {
            List<ItemAttributeLookup> _itemAttributeLookups = appUnitOfWork.GetListOf<ItemAttributeLookup>(IsForceReload, ial => ial.AttributeName);
            // 1.
            var _usedItems = ModelItemAttributes.Where(miav => (miav.ItemAttributeLookupId != currentAttributeLookupId));
            // 2. 
            var _unselectedItems = _itemAttributeLookups.Where(iav => !_usedItems.Any(miav => (miav.ItemAttributeLookupId == iav.ItemAttributeLookupId)));
            return _unselectedItems.ToList();
        }
        // Interface Stuff
        void OnNewItemAttributeDefaultSetter(ItemAttribute newItem)
        {
            newItem = itemAttributeGridRepository.NewViewEntityDefaultSetter(newItem, ParentItemId);
        }
        /// <summary>
        /// Handle the grid insert 
        /// </summary>
        /// <param name="insertedItemAttribute">what the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowInsertingAsync(SavedRowItem<ItemAttribute, Dictionary<string, object>> insertedItemAttribute)
        {
            var newItemAttribute = insertedItemAttribute.Item;
            if (newItemAttribute.ItemAttributeLookupId == Guid.Empty)
            {
                await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Item's Attribute cannot be blank!");
            }
            else
            {
                await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Adding attribute to item.");
                newItemAttribute.ItemAttributeDetail = await itemAttributeGridRepository.GetItemAttributeByIdAsync(newItemAttribute.ItemAttributeLookupId);
                if (newItemAttribute.ItemAttributeDetail == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Error finding Attribute in lookup table!");
                }
                else
                {
                    await itemAttributeGridRepository.InsertViewRowAsync(newItemAttribute, "Attribute");
                    // tell the parent that we have a variable attribute now
                    if (newItemAttribute.IsUsedForItemVariety)
                        await OnAttributeChangeToVariableEvent.InvokeAsync(true);
                }
            }
            await _DataGrid.Reload();
        }
        /// <summary>
        /// Handle the grid update
        /// </summary>
        /// <param name="updatedItem">What the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowUpdatedAsync(SavedRowItem<ItemAttribute, Dictionary<string, object>> updatedItem)
        {
            ItemAttribute _updatedItemAttribute = updatedItem.Item;
            if (_updatedItemAttribute.ItemAttributeDetail == null)
            {
                await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Item's attribute cannot be blank...");
            }
            else
            {
                // At the moment we only allow update of weather or not the Attribute is a variant, so we only need update that
                await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Updating attribute  in item.");
                var _currentItemAttribute = await itemAttributeGridRepository.GetOnlyItemAttributeAsync(_updatedItemAttribute.ItemAttributeId);  //FidnFirstByAsync(ia => ia.ItemAttributeId == _updatedItemAttribute.ItemAttributeId);  // 
                if (_currentItemAttribute == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Error updating attribute , it could not be found in the table.");
                }
                else
                {
                    bool HasChangedVaraibleAttrbiuteStatus = (_currentItemAttribute.IsUsedForItemVariety != _updatedItemAttribute.IsUsedForItemVariety);
                    _currentItemAttribute.IsUsedForItemVariety = _updatedItemAttribute.IsUsedForItemVariety;
                    int _result = await itemAttributeGridRepository.UpdateOnlyItemAttributeAsync(_currentItemAttribute);
                    //await itemAttributeGridRepository.UpdateViewRowAsync(_currentItemAttribute, _currentItemAttribute.ItemAttributeDetail.AttributeName);
                    if (appUnitOfWork.IsInErrorState())
                    {
                        await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error updating attribute {appUnitOfWork.GetErrorMessage()}, it could not be found in the table.");
                    }
                    else if (HasChangedVaraibleAttrbiuteStatus)
                    {
                        // its status as a variable attribute has changed
                        await OnAttributeChangeToVariableEvent.InvokeAsync(_updatedItemAttribute.IsUsedForItemVariety);
                    }
                    // update the in memory children, but only after update just in case EF core tries to update the children (which should not be needed as they are added as views
                    //-> note needed as this we disabled the editing _updatedItemAttribute = SetAttributeDetail(_updatedItemAttribute);
                }
            }
            //await _DataGrid.Reload();
            StateHasChanged();
        }
        /// <summary>
        /// Get the details from the database on the ItemsAttribute
        /// </summary>
        /// <param name="updatedItemAttribute">ItemAttribute to search</param>
        /// <returns></returns>
        private ItemAttribute SetAttributeDetail(ItemAttribute updatedItemAttribute)
        {
            if (updatedItemAttribute.ItemAttributeLookupId == Guid.Empty)
            {
                updatedItemAttribute.ItemAttributeDetail = null; /// force it null, cannot dispose?
            }
            else
            {
                var _itemAtts = appUnitOfWork.GetListOf<ItemAttributeLookup>();
                updatedItemAttribute.ItemAttributeDetail = _itemAtts.FirstOrDefault(ic => ic.ItemAttributeLookupId == updatedItemAttribute.ItemAttributeLookupId);
            }
            return updatedItemAttribute;
        }

        async Task OnRowRemovingAsync(CancellableRowChange<ItemAttribute> modelItem)
        {
            // set the Selected Item Attribute for use later
            SeletectedItemAttribute = modelItem.Item;
            var deleteItem = modelItem;
            await itemAttributeGridRepository.gridSettings.DeleteConfirmation.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemAttributeDetail.AttributeName}?");  //,"Delete","Cancel"); - passed in on init
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
                await itemAttributeGridRepository.DeleteViewRowByIdAsync(SeletectedItemAttribute.ItemAttributeId, SeletectedItemAttribute.ItemAttributeDetail.AttributeName);
            }
            //IsLoading = false;
            await _DataGrid.Reload();
        }
        public async Task OnRowRemovedAsync(ItemAttribute modelItem)
        {
            await _DataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
        }

        #endregion

        //async Task<List<ItemAttribute>> GetItemAttributessAsync(Guid? sourceParentId, bool IsAsyncCall = true)
        //{
        //    if (IsBusy) return null;
        //    IsBusy = true;
        //    var repo = appUnitOfWork.itemAttributesRepository();

        //    List<ItemAttributes> _gotItemAttributess = null;

        //    if (IsAsyncCall)
        //        _gotItemAttributess = (await repo.GetByAsync(icl => icl.ParentAttributeId == sourceParentId)).ToList();
        //    else
        //        _gotItemAttributess = repo.GetBy(icl => icl.ParentAttributeId == sourceParentId).ToList();
        //    //await InvokeAsync(StateHasChanged);

        //    IsBusy = false;
        //    return _gotItemAttributess;
        //}
    }
}
