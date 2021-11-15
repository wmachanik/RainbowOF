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
        IItemAttributeGridViewRepository _ItemAttributeGridViewRepository = null;
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
        public List<ItemAttribute> SourceItemAttributes { get; set; }
        [Parameter]
        public Guid ParentItemId {get;set; }
        //[Parameter]
        //public bool CanUseAsync { get; set; } = true;  // issues with detail mean that if this is a detail grid we disable Async
        #endregion
        #region Initialisation
        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            _ItemAttributeGridViewRepository = new ItemAttributeGridViewRepository(_Logger, _AppUnitOfWork);
            _ItemAttributeGridViewRepository._GridSettings.PopUpRef = PopUpRef;   // use the forms Pop Up ref
            ModelItemAttributes = SourceItemAttributes;
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region BackEnd Routines
        public Dictionary<Guid, string> GetListOfAttributes(bool IsForceReload = false)
            =>  _AppUnitOfWork.GetListOfAttributes(IsForceReload);
        // Interface Stuff
        void OnNewItemAttributeDefaultSetter(ItemAttribute newItem)
        {
            newItem = _ItemAttributeGridViewRepository.NewViewEntityDefaultSetter(newItem, ParentItemId);
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
                PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's Attribute cannot be blank!");
            }
            else
            {
                PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Adding Attribute to Item.");
                newItemAttribute.ItemAttributeDetail = await _ItemAttributeGridViewRepository.GetItemAttributeByIdAsync(newItemAttribute.ItemAttributeLookupId);
                if (newItemAttribute.ItemAttributeDetail == null)
                {
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Attribute in lookup table!");
                }
                else
                {
                    // do we need to add other ids here to save?
                    //if ((newItemAttribute.ItemAttributeVarieties!= null))
                    //{

                    //    newItemAttribute.ItemUoMBase = await _ItemAttributeGridViewRepository.GetItemUoMByIdAsync((Guid)newItemAttribute.UoMBaseId);
                    //    if (newItemAttribute.ItemUoMBase == null)
                    //    {
                    //        PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error finding Unit of Measure in lookup table!");
                    //    }
                    //}
                    await _ItemAttributeGridViewRepository.InsertViewRowAsync(newItemAttribute, "Attribute");
                }
            }
            await _DataGrid.Reload();
        }
        /// <summary>
        /// Handle the grid update
        /// </summary>
        /// <param name="updatedItem">What the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowUpdatingAsync(SavedRowItem<ItemAttribute, Dictionary<string, object>> updatedItem)
        {
            var _updatedItemAttribute = updatedItem.Item;
            if (_updatedItemAttribute.ItemAttributeDetail == null)
            {
                PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's attribute cannot be blank...");
            }
            else
            {
                // load item for database to see it exists, could later add a check using row version to see if it has changed.
                PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Updating attribute  in item.");
                var _currentItemAttribute = await _ItemAttributeGridViewRepository.GetEntityByIdAsync(_updatedItemAttribute.ItemAttributeId);
                if (_currentItemAttribute == null)
                {
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error updating attribute , it could not be found in the table.");
                }
                else
                {
                    _Mapper.Map(_updatedItemAttribute, _currentItemAttribute);
                    if (_AppUnitOfWork.IsInErrorState())
                    {
                        PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating attribute  {_AppUnitOfWork.GetErrorMessage()}, it could not be found in the table.");
                    }
                }
            }
            //await _DataGrid.Reload();
        }
        void OnRowRemoving(CancellableRowChange<ItemAttribute> modelItem)
        {
            // set the Selected Item Attribute for use later
            SeletectedItemAttribute = modelItem.Item;
            var deleteItem = modelItem;
            _ItemAttributeGridViewRepository._GridSettings.DeleteConfirmation.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemAttributeDetail.AttributeName}?");  //,"Delete","Cancel"); - passed in on init
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
                await _ItemAttributeGridViewRepository.DeleteViewRowByIdAsync(SeletectedItemAttribute.ItemAttributeId, SeletectedItemAttribute.ItemAttributeDetail.AttributeName);
            }
            //IsLoading = false;
            await _DataGrid.Reload();
        }
        public async Task OnRowRemovedAsync(ItemAttribute modelItem)
        {
            await _DataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
        }


        //async Task<List<ItemAttribute>> GetItemAttributessAsync(Guid? sourceParentId, bool IsAsyncCall = true)
        //{
        //    if (IsBusy) return null;
        //    IsBusy = true;
        //    var repo = _AppUnitOfWork.itemAttributesRepository();

        //    List<ItemAttributes> _gotItemAttributess = null;

        //    if (IsAsyncCall)
        //        _gotItemAttributess = (await repo.GetByAsync(icl => icl.ParentAttributeId == sourceParentId)).ToList();
        //    else
        //        _gotItemAttributess = repo.GetBy(icl => icl.ParentAttributeId == sourceParentId).ToList();
        //    //await InvokeAsync(StateHasChanged);

        //    IsBusy = false;
        //    return _gotItemAttributess;
        //}
        #endregion
    }
}
