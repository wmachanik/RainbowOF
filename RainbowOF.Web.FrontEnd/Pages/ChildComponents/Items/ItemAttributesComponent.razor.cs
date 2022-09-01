using AutoMapper;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Items;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Items
{
    public partial class ItemAttributesComponent : ComponentBase
    {
        #region Grid Variables
        // Interface Stuff
        private List<ItemAttribute> modelItemAttributes { get; set; }
        private ItemAttribute seletectedItemAttribute { get; set; }
        private DataGrid<ItemAttribute> dataGrid { get; set; }
        // All there workings are here
        private IItemAttributeGridRepository itemAttributeGridRepository { get; set; } = null;
        #endregion
        #region Injected Variables
        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        //[Inject]
        //IDbContextFactory<ApplicationDbContext> AppDbContext { get; set; }
        [Inject]
        public ILoggerManager AppLoggerManager { get; set; }
        [Inject]
        public IMapper AppMapper { get; set; }
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
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("ItemAttributesComponent initialising.");
            //-- not sure why but when we await this the variant async initialise is called during the database query, so have changed it to allow sync calls
            modelItemAttributes = AppUnitOfWork.ItemAttributeRepo.GetEagerItemAttributeByItemIdAsync(ParentItemId);   ///, false /*no async*/).Result /* for no async*/;
            modelItemAttributes ??= new();  // if what was returned was null then create a new one so a grid will be displayed
            AppUnitOfWork.GetListOf<ItemAttributeLookup>(true, ial => ial.AttributeName);
            //!!! This must be done last so that loading is displayed until we are finished loading.
            itemAttributeGridRepository = new ItemAttributeGridRepository(AppLoggerManager, AppUnitOfWork, AppUnitOfWork.ItemAttributeRepo);
            itemAttributeGridRepository.CurrGridSettings.PopUpRef = PopUpRef;   // use the forms Pop Up ref
            await base.OnInitializedAsync();
            //---> not needed await InvokeAsync(StateHasChanged);
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("ItemAttributesComponent initialised.");
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
            List<ItemAttributeLookup> _itemAttributeLookups = AppUnitOfWork.GetListOf<ItemAttributeLookup>(IsForceReload, ial => ial.AttributeName);
            // 1.
            var _usedItems = modelItemAttributes.Where(miav => (miav.ItemAttributeLookupId != currentAttributeLookupId));
            // 2. 
            var _unselectedItems = _itemAttributeLookups.Where(iav => !_usedItems.Any(miav => (miav.ItemAttributeLookupId == iav.ItemAttributeLookupId)));
            return _unselectedItems.ToList();
        }
        // Interface Stuff
        void OnNewItemAttributeDefaultSetter(ItemAttribute newItem)
        {
            // newItem = 
            itemAttributeGridRepository.NewViewEntityDefaultSetter(newItem, ParentItemId);
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
                newItemAttribute.ItemAttributeDetail =   await itemAttributeGridRepository.GetItemAttributeByIdAsync(newItemAttribute.ItemAttributeLookupId);
                                                   //  await itemAttributeGridRepository.GetItemAttributeByIdAsync(newItemAttribute.ItemAttributeLookupId);
                if (newItemAttribute.ItemAttributeDetail == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Error finding Attribute in lookup table!");
                }
                else
                {
                    var _result = await itemAttributeGridRepository.InsertViewRowAsync(newItemAttribute, "Attribute");
                    if (_result == null)
                    {
                        // something went wrong with the adding so delete it from the memory 
                        modelItemAttributes.Remove(newItemAttribute);
                    }
                    // tell the parent that we have a variable attribute now
                    if (newItemAttribute.IsUsedForItemVariety)
                        await OnAttributeChangeToVariableEvent.InvokeAsync(true);
                }
            }
            await dataGrid.Reload();
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
                    //_currentItemAttribute.IsUsedForItemVariety = _updatedItemAttribute.IsUsedForItemVariety;
                    //int _result = 
                    await itemAttributeGridRepository.UpdateOnlyItemAttributeAsync(_updatedItemAttribute);
                    //await itemAttributeGridRepository.UpdateViewRowAsync(_currentItemAttribute, _currentItemAttribute.ItemAttributeDetail.AttributeName);
                    if (AppUnitOfWork.IsInErrorState())
                    {
                        await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error updating attribute {AppUnitOfWork.GetErrorMessage()}, it could not be found in the table.");
                    }
                    else if (HasChangedVaraibleAttrbiuteStatus)
                    {
                        // its status as a variable attribute has changed so tell the parent
                        await OnAttributeChangeToVariableEvent.InvokeAsync(_updatedItemAttribute.IsUsedForItemVariety);
                    }
                    // update the in memory children, but only after update just in case EF core tries to update the children (which should not be needed as they are added as views
                    //-> note needed as this we disabled the editing _updatedItemAttribute = SetAttributeDetail(_updatedItemAttribute);
                }
            }
            //await _DataGrid.Reload();
            StateHasChanged();
        }
        ///// <summary>
        ///// Get the details from the database on the ItemsAttribute
        ///// </summary>
        ///// <param name="updatedItemAttribute">ItemAttribute to search</param>
        ///// <returns></returns>
        //private ItemAttribute SetAttributeDetail(ItemAttribute updatedItemAttribute)
        //{
        //    if (updatedItemAttribute.ItemAttributeLookupId == Guid.Empty)
        //    {
        //        updatedItemAttribute.ItemAttributeDetail = null; /// force it null, cannot dispose?
        //    }
        //    else
        //    {
        //        var _itemAtts = AppUnitOfWork.GetListOf<ItemAttributeLookup>();
        //        updatedItemAttribute.ItemAttributeDetail = _itemAtts.FirstOrDefault(ic => ic.ItemAttributeLookupId == updatedItemAttribute.ItemAttributeLookupId);
        //    }
        //    return updatedItemAttribute;
        //}
        private ItemAttribute _seletectedItemAttributeToDelete; // used to remember which item to delete

        async Task OnRowRemovingAsync(CancellableRowChange<ItemAttribute> modelItem)
        {
            // set the Selected Item Attribute for use later
            _seletectedItemAttributeToDelete = modelItem.Item;
            var deleteItem = modelItem;
            await itemAttributeGridRepository.CurrGridSettings.DeleteConfirmation.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemAttributeDetail.AttributeName}?");  //,"Delete","Cancel"); - passed in on init
        }
        /// <summary>
        /// Confirm Delete Click is called when the user confirms they want to delete.
        /// </summary>
        /// <param name="confirmationOption">confirmation option by the user selection</param>
        /// <returns></returns>
        async Task ConfirmDelete_ClickAsync(bool confirmationOption)
        {
            if ((confirmationOption) && (_seletectedItemAttributeToDelete != null))
            {
                await itemAttributeGridRepository.DeleteViewRowByEntityAsync(_seletectedItemAttributeToDelete, _seletectedItemAttributeToDelete.ItemAttributeDetail.AttributeName);
            }
            await dataGrid.Reload();
        }
        public async Task OnRowRemovedAsync(ItemAttribute modelItem)
        {
            await dataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
        }

        #endregion

        //async Task<List<ItemAttribute>> GetItemAttributessAsync(Guid? sourceParentId, bool IsAsyncCall = true)
        //{
        //    if (IsBusy) return null;
        //    IsBusy = true;
        //    var repo = AppUnitOfWork.itemAttributesRepository();

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
