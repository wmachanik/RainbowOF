using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Lookups;
using RainbowOF.Tools;
using RainbowOF.Tools.Services;
using RainbowOF.ViewModels.Common;
using RainbowOF.ViewModels.Lookups;
using RainbowOF.Web.FrontEnd.Pages.ChildComponents.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemAttributeVarietiesComponent : ComponentBase
    {
        // Interface Stuff
        public GridSettings _VarietiesGridSettings = new();
        public ItemAttributeVarietyLookupView SelectedItemAttributeVarietyLookup = null;
        public BulkAction SelectedBulkAction = BulkAction.none;
        // variables / Models
        public List<ItemAttributeVarietyLookupView> VarietyDataModels = null;
        public ItemAttributeVarietyLookupView seletectedVarietyItem = null;
        //public const string disabledStr = "N";
        //public const string enabledStr = "Y";

        public bool GroupButtonEnabled = true;
        private bool IsLoading = false;
        private string _Status = "";
        DataGrid<ItemAttributeVarietyLookupView> _VarietiesDataGrid;


        // All there workings are here
        IAttributeVarietyWooLinkedView _AttributeVarietyWooLinkedViewRepository;

        public List<ItemAttributeVarietyLookupView> SelectedItemAttributeVarietyLookups;
        [Inject]
        ApplicationState _appState { get; set; }
        [Inject]
        public ILoggerManager _logger { get; set; }
        [Inject]
        IAppUnitOfWork _appUnitOfWork { get; set; }


        [Parameter]
        public Guid ParentItemAttributeLookupId { get; set; } = Guid.Empty;
        [Parameter]
        public int StartingPageSize { get; set; } = 5;
        [Parameter]
        public bool IsSubGrid { get; set; } = false;
        //public List<ItemAttributeVarietyLookup> ItemAttributeVarietyLookups { get; set; }
        //[Parameter]

        RainbowOF.Components.Modals.ColorSelector colorFGSelector { get; set; }
        RainbowOF.Components.Modals.ColorSelector colorBGSelector { get; set; }

        protected override void OnParametersSet()
        {
            if ((ParentItemAttributeLookupId != Guid.Empty) && (_VarietiesDataGrid != null))
                _VarietiesDataGrid.Reload();
        }
        protected override async Task OnInitializedAsync()
        {
            if (ParentItemAttributeLookupId != Guid.Empty)
            {
                _VarietiesGridSettings.PageSize = StartingPageSize;
                _AttributeVarietyWooLinkedViewRepository = new AttributeVarietyWooLinkedViewRepository(_logger, _appUnitOfWork, _VarietiesGridSettings, ParentItemAttributeLookupId);
                //await LoadData();
            }
            await InvokeAsync(StateHasChanged);
        }
        private async Task SetLoadStatus(string statusString)
        {
            _Status = statusString;
            await InvokeAsync(StateHasChanged);
        }
        public async Task LoadData()
        {
            await HandleReadDataAsync(new DataGridReadDataEventArgs<ItemAttributeVarietyLookupView>(_VarietiesGridSettings.CurrentPage, _VarietiesGridSettings.PageSize, null, System.Threading.CancellationToken.None));
        }

        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemAttributeVarietyLookupView> inputDataGridReadData)
        {
            if (/*(IsLoading) || */(ParentItemAttributeLookupId == Guid.Empty))
                return;

            IsLoading = true;  // prevent re-entry

            if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
            {
                DataGridParameters _dataGridParameters = _AttributeVarietyWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, _VarietiesGridSettings.CustomFilterValue);
                if (_VarietiesGridSettings.PageSize != inputDataGridReadData.PageSize)
                { /// page sized changed so jump back to original page
                    _VarietiesGridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                    _VarietiesGridSettings.PageSize = inputDataGridReadData.PageSize;
                    //                  await Reload();
                }
                await SetLoadStatus("Checking Woo status & loading Attributes");
                try
                {
                    await SetLoadStatus("Checking Woo status");
                    _VarietiesGridSettings.WooIsActive = await _AttributeVarietyWooLinkedViewRepository.WooIsActive(_appState);
                    await SetLoadStatus("Loading Attributes");
                    await LoadItemAttributeVarietyLookupList(_dataGridParameters);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error running async tasks: {ex.Message}");
                    throw;
                }
                _Status = string.Empty;
            }
            IsLoading = false;
        }
        private async Task LoadItemAttributeVarietyLookupList(DataGridParameters currentDataGridParameters)
        {
            // store old select items list
            try
            {
                var response = await _AttributeVarietyWooLinkedViewRepository.LoadViewItemsPaginatedAsync(currentDataGridParameters);
                if (VarietyDataModels == null)
                    VarietyDataModels = new List<ItemAttributeVarietyLookupView>(response);  // not sure why we have to use a holding variable just following the demo code
                else
                {
                    VarietyDataModels.Clear();
                    VarietyDataModels.AddRange(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            //restore the old items that were selected.
            SelectedItemAttributeVarietyLookups = _AttributeVarietyWooLinkedViewRepository.PopSelectedItems(VarietyDataModels);
            StateHasChanged();
        }
        async Task HandleCustomerSearchOnKeyUp(KeyboardEventArgs kbEventHandler)
        {
            var key = (string)kbEventHandler.Key;   // not using this but just in case
                                                    //if (_gridSettings.CustomFilterValue.Length > 2)
                                                    //{
            await _VarietiesDataGrid.Reload();
            //}
        }

        async Task OnVarietyRowInserting(SavedRowItem<ItemAttributeVarietyLookupView, Dictionary<string, object>> pInsertedItem)
        {
            var newItem = pInsertedItem.Item;

            await _AttributeVarietyWooLinkedViewRepository.InsertRowAsync(newItem);
            await _VarietiesDataGrid.Reload();
        }
        List<string> _ListOfSymbols = null;
        List<string> GetListOSymbols()
        {
            if (_ListOfSymbols == null)
            {
                _ListOfSymbols = new List<string>();

                for (int i = 32; i < 255; i++)    /// start at 32 to exclude the most common
                {
                    if (Char.IsSymbol((char)i))
                        _ListOfSymbols.Add(char.ToString((char)i));
                }
            }
            return _ListOfSymbols;
        }
        public async Task OnVarietyRowInserted(SavedRowItem<ItemAttributeVarietyLookupView, Dictionary<string, object>> insertedItem)
        {
            var newItem = insertedItem.Item;

            await _AttributeVarietyWooLinkedViewRepository.InsertRowAsync(newItem);
            await _VarietiesDataGrid.Reload();
        }
        void OnItemAttributeVarietyLookupNewItemDefaultSetter(ItemAttributeVarietyLookupView newItem)
        {
            newItem = _AttributeVarietyWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        async Task<int> UpdateItemAttributeVarietyLookup(ItemAttributeVarietyLookupView UpdatedItemView)
        {
            int _result = await _AttributeVarietyWooLinkedViewRepository.UpdateItemAsync(UpdatedItemView);
            await _VarietiesDataGrid.Reload();
            return _result;
        }

        async Task OnVarietyRowUpdating(SavedRowItem<ItemAttributeVarietyLookupView, Dictionary<string, object>> pUpdatedItem)
        {
            await _AttributeVarietyWooLinkedViewRepository.UpdateRowAsync(pUpdatedItem.Item);
            await _VarietiesDataGrid.Reload();
        }
        void OnVarietyRowRemoving(CancellableRowChange<ItemAttributeVarietyLookupView> modelItem)
        {
            // set the Selected Item Attribute for use later
            SelectedItemAttributeVarietyLookup = modelItem.Item;
            var deleteItem = modelItem;
            _VarietiesGridSettings.DeleteConfirmation.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.VarietyName}?", SelectedItemAttributeVarietyLookup.HasWooAttributeVarietyMap);  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmVarietyAddWooItem_Click(bool confirm)
        {
            if (confirm)
            {
                // they want to add the item to Woo 
                await _AttributeVarietyWooLinkedViewRepository.AddWooItemAndMapAsync(SelectedItemAttributeVarietyLookup);
                await _VarietiesDataGrid.Reload();
            }
        }
        async Task ConfirmVarietyDeleteWooItem_Click(bool confirm)
        {
            // they want to delete the item to Woo 
            await _AttributeVarietyWooLinkedViewRepository.DeleteWooItemAsync(SelectedItemAttributeVarietyLookup.ItemAttributeVarietyLookupId, confirm);
            //  regardless of how we got here they wanted to delete the original Attribute so delete it now, but only after Woo delete if they wanted it deleted.
            await _AttributeVarietyWooLinkedViewRepository.DeleteRowAsync(SelectedItemAttributeVarietyLookup);
            await _VarietiesDataGrid.Reload();
        }
        //protected async Task
        async Task ConfirmVarietyDelete_Click(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooAttribute and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    _VarietiesGridSettings.DeleteWooItemConfirmation.ShowModal("Are you sure?", $"Delete {SelectedItemAttributeVarietyLookup.VarietyName} from Woo too?", "Delete", "Cancel");
                else
                    await _AttributeVarietyWooLinkedViewRepository.DeleteRowAsync(SelectedItemAttributeVarietyLookup);

            }
            await _VarietiesDataGrid.Reload();
        }
        public async Task OnVarietyRowRemoved(ItemAttributeVarietyLookupView modelItem)
        {
            await InvokeAsync(StateHasChanged);
            await _VarietiesDataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
        }

        async Task DoGroupAction()
        {
            //if (SelectedBulkAction == BulkAction.none)
            //    return;   ----> button should be disabled 

            _VarietiesGridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");

            int done = 0;
            int failed = 0;
            foreach (var item in SelectedItemAttributeVarietyLookups)
            {
                if (await _AttributeVarietyWooLinkedViewRepository.DoGroupActionAsync(item, SelectedBulkAction) > 0)
                    done++;
                else
                    failed++;
            }
            _VarietiesGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {done} items and not applied to {failed} items.");
            await _VarietiesDataGrid.Reload();
        }

        //UoMSelecteListComponent UoMSelectListRef;
        //protected void ConfirmUoMIdChanged(Guid changedUoMId)
        //{
        ////    UoMSelectListRef.SourceUoMId = changedUoMId;
        //}
                //var newItem = pInsertedItem.Item;
        //IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _appUnitOfWork.Repository<ItemAttributeVarietyLookup>();
        //// first check if we do not already have a Attribute like this.
        //if (await _ItemAttributeVarietyLookupRepository.FindFirstAsync(iavl => iavl.VarietyName == newItem.VarietyName) == null)
        //{
        //    int _recsAdded = await _ItemAttributeVarietyLookupRepository.AddAsync(newItem);
        //    if (_recsAdded != AppUnitOfWork.CONST_WASERROR)
        //        _VarietiesGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newItem.VarietyName} - {_appUnitOfWork.GetErrorMessage()}", "Attribute variety Added");
        //    else
        //        _VarietiesGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newItem.VarietyName} - {_appUnitOfWork.GetErrorMessage()}", "Error adding Attribute variety");
        //}
        //else
        //    _VarietiesGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newItem.VarietyName} already exists, so could not be added.");
        //await LoadVarieties();   // reload the list so the latest item is displayed

        //public ItemAttributeVarietyLookup OnItemAttributeVarietyLookupNewItemDefaultSetter(ItemAttributeVarietyLookup newVariety)
        //{
        //    // can figure out how to get the parent id so used the default
        //    if (newVariety == null)
        //        newVariety = new ItemAttributeVarietyLookup();

        //    newVariety.ItemAttributeVarietyLookupId = ParentItemAttributeLookupId;  // the selected one should be the parent
        //    newVariety.VarietyName = "Variety (must be unique)";
        //    newVariety.SortOrder = 0;
        //    newVariety.Notes = $"Added {DateTime.Now.Date}";

        //    return newVariety;
        //}
        //public async Task<int> UpdateItemAttributeVarietyLookup(ItemAttributeVarietyLookup updatedVariety)
        //{
        //    int _recsUpdted = 0;
        //    IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _appUnitOfWork.Repository<ItemAttributeVarietyLookup>();
        //    // first check it exists - it could have been deleted 
        //    ItemAttributeVarietyLookup _UpdatedLookup = await _ItemAttributeVarietyLookupRepository.GetByIdAsync(updatedVariety.ItemAttributeVarietyLookupId);

        //    if (_UpdatedLookup == null)
        //    {
        //        _VarietiesGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute variety: {updatedVariety.VarietyName} is no longer found, was it deleted?");
        //        return AppUnitOfWork.CONST_WASERROR;
        //    }
        //    else
        //    {
        //        _UpdatedLookup.ItemAttributeVarietyLookupId = updatedVariety.ItemAttributeVarietyLookupId;
        //        _UpdatedLookup.VarietyName = updatedVariety.VarietyName;
        //        _UpdatedLookup.UoMId = (updatedVariety.UoMId == Guid.Empty) ? null : updatedVariety.UoMId;
        //        _UpdatedLookup.SortOrder = updatedVariety.SortOrder;
        //        _UpdatedLookup.Symbol = updatedVariety.Symbol;
        //        _UpdatedLookup.FGColour = (updatedVariety.FGColour == ItemAttributeVarietyLookup.CONST_NULL_COLOUR) ? null : updatedVariety.FGColour;
        //        _UpdatedLookup.BGColour = (updatedVariety.BGColour == ItemAttributeVarietyLookup.CONST_NULL_COLOUR) ? null : updatedVariety.BGColour;
        //        _UpdatedLookup.Notes = updatedVariety.Notes;
        //        //                if (!_UpdatedLookup.Equals(pUpdatedVariety))
        //        _recsUpdted = await _ItemAttributeVarietyLookupRepository.UpdateAsync(_UpdatedLookup);   //_ItemAttributeVarietyLookupRepository.Update(_UpdatedLookup); // 
        //        if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
        //            _VarietiesGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{updatedVariety.VarietyName} - {_appUnitOfWork.GetErrorMessage()}", "Error adding Attribute variety");
        //    }
        //    return _recsUpdted;
        //}
        //private bool IsDuplicate(ItemAttributeVarietyLookup pItem)
        //{
        //    // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
        //    var _exists = attribVarieties.FindAll(av => av.VarietyName == pItem.VarietyName);
        //    return ((_exists != null) && (_exists.Count > 1));
        //}
        //private bool IsValid(ItemAttributeVarietyLookup pItem)
        //{
        //    // check that there is a loop back on PaerentId
        //    return true;  /// may need a check here
        //}
        //async Task OnVarietyRowUpdated(SavedRowItem<ItemAttributeVarietyLookup, Dictionary<string, object>> updatedItem)
        //{
        //    ItemAttributeVarietyLookup updatedItem = updatedItem.Item;
        //    if (IsDuplicate(updatedItem))
        //        _VarietiesGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute variety Name: {updatedItem.VarietyName} - already exists, cannot be updated", "Exists already");
        //    else
        //    {
        //        if (IsValid(updatedItem))
        //        {
        //            // update and check for errors 
        //            if (await UpdateItemAttributeVarietyLookup(updatedItem) != AppUnitOfWork.CONST_WASERROR)
        //            {
        //                //if ((updatedItem.HasWooAttributeVarietyMap) && (await UpdateWooAttributeVarietyMap(updatedItem) == AppUnitOfWork.CONST_WASERROR))
        //                //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"WooAttributeVariety map for Item: {updatedItem.AttributeVarietyName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating");
        //                ////else
        //                ////    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"AttributeVariety: {updatedItem.AttributeVarietyName} was updated.");
        //            }
        //        }
        //        else
        //            _VarietiesGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute variety Item {updatedItem.VarietyName} not valid", "Error updating");

        //    }
        //    //await LoadItemAttributeVarietyLookupList();   // reload the list so the latest item is displayed
        //}
        //void OnVarietyRowRemoving(CancellableRowChange<ItemAttributeVarietyLookup> varItem)
        //{
        //    // set the Selected Item AttributeVariety for use later
        //    SelectedItemAttributeVarietyLookup = varItem.Item;
        //    var deleteItem = varItem;
        //    _VarietiesGridSettings.DeleteConfirmation.ShowModal("Variety delete confirmation", $"Are you sure you want to delete variety: {deleteItem.Item.VarietyName}?");  //,"Delete","Cancel"); - passed in on init
        //}

        ////protected async Task
        //async Task ConfirmVarietyDelete_Click(bool deleteConfirmed)
        //{
        //    if (deleteConfirmed)
        //    {
        //        IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _appUnitOfWork.Repository<ItemAttributeVarietyLookup>();

        //        var _recsDelete = await _ItemAttributeVarietyLookupRepository.DeleteByIdAsync(SelectedItemAttributeVarietyLookup.ItemAttributeVarietyLookupId);

        //        if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
        //            _VarietiesGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"AttributeVariety: {SelectedItemAttributeVarietyLookup.VarietyName} is no longer found, was it deleted?");
        //        else
        //            _VarietiesGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"AttributeVariety: {SelectedItemAttributeVarietyLookup.VarietyName} was it deleted?");
        //    }
        //    //await LoadItemAttributeVarietyLookupList();   // reload the list so the latest item is displayed
        //}
        async Task Reload()
        {
            _VarietiesGridSettings.CurrentPage = 1;
            await _VarietiesDataGrid.Reload();
        }

        public async Task SetParentAttributeId(Guid newAtttributeID)
        {
            ParentItemAttributeLookupId = newAtttributeID;
            _AttributeVarietyWooLinkedViewRepository.SetParentAttributeId(newAtttributeID);
            await Reload();
        }
    }
}
