using AutoMapper;
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
using RainbowOF.Web.FrontEnd.Pages.ChildComponents.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Lookups
{
    public partial class ItemAttributeVarietiesLookupComponent : ComponentBase
    {
        // Interface Stuff
        //public GridSettings _VarietiesGridSettings = new();
        public ItemAttributeVarietyLookupView SelectedItemAttributeVarietyLookup = null;
        public BulkAction SelectedBulkAction = BulkAction.none;
        // variables / Models
        public List<ItemAttributeVarietyLookupView> VarietyDataModels = null;
        public ItemAttributeVarietyLookupView seletectedVarietyItem = null;

        public bool GroupButtonEnabled = true;
        private bool IsLoading = false;
        private string _Status = "";
        DataGrid<ItemAttributeVarietyLookupView> _VarietiesDataGrid;
        // All there workings are here
        IAttributeVarietyWooLinkedView _AttributeVarietyWooLinkedViewRepository;

        public List<ItemAttributeVarietyLookupView> SelectedItemAttributeVarietyLookups;
        [Inject]
        ApplicationState _AppState { get; set; }
        [Inject]
        public ILoggerManager appLoggerManager { get; set; }
        [Inject]
        IUnitOfWork appUnitOfWork { get; set; }
        [Inject]
        public IMapper _Mapper { get; set; }

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
            // !!!!!!! no async stuff here!!!!!
            //if ((ParentItemAttributeLookupId != Guid.Empty) && (_VarietiesDataGrid != null))
            //{
            //    var task = Task.Run(async () => await _VarietiesDataGrid.Reload());
            //    task.Wait();
            //}
        }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (ParentItemAttributeLookupId != Guid.Empty)
            {
                _AttributeVarietyWooLinkedViewRepository = new AttributeVarietyWooLinkedViewRepository(appLoggerManager, appUnitOfWork, /* _VarietiesGridSettings, */_Mapper, ParentItemAttributeLookupId);
                _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.PageSize = StartingPageSize;
                //await LoadData();
            }
            await InvokeAsync(StateHasChanged);
        }
        private async Task SetLoadStatus(string statusString)
        {
            _Status = statusString;
            await InvokeAsync(StateHasChanged);
        }
        //public async Task LoadData()
        //{
        //  //  await HandleReadDataAsync(new DataGridReadDataEventArgs<ItemAttributeVarietyLookupView>(_AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage, _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.PageSize, null, System.Threading.CancellationToken.None));
        //}

        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemAttributeVarietyLookupView> inputDataGridReadData)
        {
            if ((IsLoading) || (ParentItemAttributeLookupId == Guid.Empty))
                return;

            IsLoading = true;  // prevent re-entry
            try
            {

                if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
                {
                    DataGridParameters _dataGridParameters = _AttributeVarietyWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue);
                    if (_AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.PageSize != inputDataGridReadData.PageSize)
                    { /// page sized changed so jump back to original page
                        _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                        _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.PageSize = inputDataGridReadData.PageSize;
                        //                  await Reload();
                    }
                    await SetLoadStatus("Checking Woo status & loading Attributes");
                    await SetLoadStatus("Checking Woo status");
                    _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.WooIsActive = await _AttributeVarietyWooLinkedViewRepository.WooIsActiveAsync(_AppState);
                    await SetLoadStatus("Loading Attributes");
                    await LoadItemAttributeVarietyLookupList(_dataGridParameters);
                }
            }
            catch (Exception ex)
            {
                appLoggerManager.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            finally
            {
                _Status = string.Empty;
                IsLoading = false;
            }
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
                appLoggerManager.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            //restore the old items that were selected.
            SelectedItemAttributeVarietyLookups = _AttributeVarietyWooLinkedViewRepository.PopSelectedItems(VarietyDataModels);
            StateHasChanged();
        }
        async Task HandleCustomerSearchOnKeyUp(KeyboardEventArgs kbEventHandler)
        {
            var key = (string)kbEventHandler.Key;   // not using this but just in case
                                                    //if (_WooLinkedGridSettings.CustomFilterValue.Length> 2)
                                                    //{
            await _VarietiesDataGrid.Reload();
            //}
        }

        async Task OnVarietyRowInserting(SavedRowItem<ItemAttributeVarietyLookupView, Dictionary<string, object>> insertedItem)
        {
            var newItem = insertedItem.Item;

            await _AttributeVarietyWooLinkedViewRepository.InsertRowAsync(newItem);
            await _VarietiesDataGrid.Reload();
        }
        List<string> _listOfSymbols = null;
        List<string> GetListOSymbols()
        {
            if (_listOfSymbols == null)
            {
                _listOfSymbols = new List<string>();

                for (int i = 32; i < 255; i++)    /// start at 32 to exclude the most common
                {
                    if (Char.IsSymbol((char)i))
                        _listOfSymbols.Add(char.ToString((char)i));
                }
            }
            return _listOfSymbols;
        }
        public async Task OnVarietyRowInserted(SavedRowItem<ItemAttributeVarietyLookupView, Dictionary<string, object>> insertedItem)
        {
            var _newItem = insertedItem.Item;

            await _AttributeVarietyWooLinkedViewRepository.InsertRowAsync(_newItem);
            await _VarietiesDataGrid.Reload();
        }
        void OnItemAttributeVarietyLookupNewItemDefaultSetter(ItemAttributeVarietyLookupView newItem)
        {
            newItem = _AttributeVarietyWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        //async Task<int> UpdateItemAttributeVarietyLookup(ItemAttributeVarietyLookupView updatedItemView)
        //{
        //    int _result = await _AttributeVarietyWooLinkedViewRepository.UpdateItemAsync(updatedItemView);
        //    await _VarietiesDataGrid.Reload();
        //    return _result;
        //}

        async Task OnVarietyRowUpdatingAsync(SavedRowItem<ItemAttributeVarietyLookupView, Dictionary<string, object>> updatedItem)
        {
            var _updatedItem = updatedItem.Item;

            await _AttributeVarietyWooLinkedViewRepository.UpdateRowAsync(_updatedItem);
            await _VarietiesDataGrid.Reload();
        }
        async Task OnVarietyRowRemovingAsync(CancellableRowChange<ItemAttributeVarietyLookupView> modelItem)
        {
            // set the Selected Item Attribute for use later
            SelectedItemAttributeVarietyLookup = modelItem.Item;
            var deleteItem = modelItem;
            await _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.DeleteConfirmationWithOption.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.VarietyName}?", SelectedItemAttributeVarietyLookup.HasECommerceAttributeVarietyMap);  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmVarietyAddWooItem_Click(bool confirmClicked)
        {
            if (confirmClicked)
            {
                // they want to add the item to Woo 
                await _AttributeVarietyWooLinkedViewRepository.AddWooItemAndMapAsync(SelectedItemAttributeVarietyLookup);
                await _VarietiesDataGrid.Reload();
            }
        }
        async Task ConfirmVarietyDeleteWooItem_Click(bool confirmClicked)
        {
            // they want to delete the item to Woo 
            IsLoading = true; // tell grid not to reload we are busy
            await _AttributeVarietyWooLinkedViewRepository.DeleteWooItemAsync(SelectedItemAttributeVarietyLookup.ItemAttributeVarietyLookupId, confirmClicked);
            //  regardless of how we got here they wanted to delete the original Attribute so delete it now, but only after Woo delete if they wanted it deleted.
            await _AttributeVarietyWooLinkedViewRepository.DeleteRowAsync(SelectedItemAttributeVarietyLookup);
            IsLoading = false; // tell grid not to reload we are busy
            await _VarietiesDataGrid.Reload();
        }
        //protected async Task
        async Task ConfirmVarietyDelete_Click(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooAttribute and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    await _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.DeleteWooItemConfirmation.ShowModalAsync("Are you sure?", $"Delete {SelectedItemAttributeVarietyLookup.VarietyName} from Woo too?", "Delete", "Cancel");
                else
                    await _AttributeVarietyWooLinkedViewRepository.DeleteRowAsync(SelectedItemAttributeVarietyLookup);

            }
            await _VarietiesDataGrid.Reload();
        }
        public async Task OnVarietyRowRemoved(ItemAttributeVarietyLookupView modelItem)
        {
            //await InvokeAsync(StateHasChanged);
            await _VarietiesDataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
           // await InvokeAsync(StateHasChanged);
        }

        async Task DoGroupAction()
        {
            //if (SelectedBulkAction == BulkAction.none)
            //    return;   ----> button should be disabled 

            await  _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");

            int _done = 0;
            int _failed = 0;
            foreach (var item in SelectedItemAttributeVarietyLookups)
            {
                if (await _AttributeVarietyWooLinkedViewRepository.DoGroupActionAsync(item, SelectedBulkAction)> 0)
                    _done++;
                else
                    _failed++;
            }
            await _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {_done} items and not applied to {_failed} items.");
            await _VarietiesDataGrid.Reload();
        }
        async Task ReloadAsync()
        {
            _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage = 1;
            await _VarietiesDataGrid.Reload();
        }
        public async Task SetParentAttributeId(Guid newAtttributeID)
        {
            ParentItemAttributeLookupId = newAtttributeID;
            _AttributeVarietyWooLinkedViewRepository.SetParentAttributeId(newAtttributeID);
            await ReloadAsync();
        }
    }
}
