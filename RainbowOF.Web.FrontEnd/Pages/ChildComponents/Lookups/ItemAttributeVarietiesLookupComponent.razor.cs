using AutoMapper;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Lookups;
using RainbowOF.Tools;
using RainbowOF.Tools.Services;
using RainbowOF.ViewModels.Common;
using RainbowOF.ViewModels.Lookups;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Lookups
{
    public partial class ItemAttributeVarietiesLookupComponent : ComponentBase
    {
        #region Injected and parameter values
        [Inject]
        public ApplicationState AppState { get; set; }
        [Inject]
        public ILoggerManager AppLoggerManager { get; set; }
        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        [Inject]
        public IMapper AppMapper { get; set; }
        [Parameter]
        public Guid ParentItemAttributeLookupId { get; set; } = Guid.Empty;
        [Parameter]
        public int StartingPageSize { get; set; } = 5;
        [Parameter]
        public bool IsSubGrid { get; set; } = false;
        #endregion
        #region Private and local variables
        //private GridSettings varietiesGridSettings = new();
        private ItemAttributeVarietyLookupView selectedItemAttributeVarietyLookup { get; set; } = null;
        private BulkAction selectedBulkAction { get; set; } = BulkAction.none;
        // variables / Models
        private List<ItemAttributeVarietyLookupView> varietyDataModels { get; set; } = null;
        //private ItemAttributeVarietyLookupView seletectedVarietyItem { get; set; } = null;

        //private bool groupButtonEnabled { get; set; } = true;
        private bool isLoading { get; set; } = false;
        private string currStatus { get; set; } = "";
        private DataGrid<ItemAttributeVarietyLookupView> varietiesDataGrid { get; set; }
        // All there workings are here
        private IAttributeVarietyWooLinkedView attributeVarietyWooLinkedViewRepository { get; set; }
        private List<ItemAttributeVarietyLookupView> selectedItemAttributeVarietyLookups { get; set; }
        //public List<ItemAttributeVarietyLookup> ItemAttributeVarietyLookups { get; set; }
        //[Parameter]
        private RainbowOF.Components.Modals.ColorSelector colorFGSelector { get; set; }
        private RainbowOF.Components.Modals.ColorSelector colorBGSelector { get; set; }
        #endregion
        #region Initialisation
        //protected override void OnParametersSet()
        //{
        //    // !!!!!!! no async stuff here!!!!!
        //    //if ((ParentItemAttributeLookupId != Guid.Empty) && (varietiesDataGrid != null))
        //    //{
        //    //    var task = Task.Run(async () => await varietiesDataGrid.Reload());
        //    //    task.Wait();
        //    //}
        //}
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (ParentItemAttributeLookupId != Guid.Empty)
            {
                attributeVarietyWooLinkedViewRepository = new AttributeVarietyWooLinkedViewRepository(AppLoggerManager, AppUnitOfWork, /* _VarietiesGridSettings, */AppMapper, ParentItemAttributeLookupId);
                attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.PageSize = StartingPageSize;
                //await LoadData();
            }
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Back end code
        private async Task SetLoadStatus(string statusString)
        {
            currStatus = statusString;
            await InvokeAsync(StateHasChanged);
        }
        //public async Task LoadData()
        //{
        //  //  await HandleReadDataAsync(new DataGridReadDataEventArgs<ItemAttributeVarietyLookupView>(_AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage, _AttributeVarietyWooLinkedViewRepository._WooLinkedGridSettings.PageSize, null, System.Threading.CancellationToken.None));
        //}

        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemAttributeVarietyLookupView> inputDataGridReadData)
        {
            if ((isLoading) || (ParentItemAttributeLookupId == Guid.Empty))
                return;

            isLoading = true;  // prevent re-entry
            try
            {

                if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
                {
                    DataGridParameters _dataGridParameters = attributeVarietyWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.CustomFilterValue);
                    if (attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.PageSize != inputDataGridReadData.PageSize)
                    { /// page sized changed so jump back to original page
                        attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                        attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.PageSize = inputDataGridReadData.PageSize;
                        //                  await Reload();
                    }
                    await SetLoadStatus("Checking Woo status & loading Attributes");
                    await SetLoadStatus("Checking Woo status");
                    attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.WooIsActive = await attributeVarietyWooLinkedViewRepository.WooIsActiveAsync(AppState);
                    await SetLoadStatus("Loading Attributes");
                    await LoadItemAttributeVarietyLookupList(_dataGridParameters);
                }
            }
            catch (Exception ex)
            {
                AppLoggerManager.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            finally
            {
                currStatus = string.Empty;
                isLoading = false;
            }
        }
        private async Task LoadItemAttributeVarietyLookupList(DataGridParameters currentDataGridParameters)
        {
            // store old select items list
            try
            {
                var response = await attributeVarietyWooLinkedViewRepository.LoadViewItemsPaginatedAsync(currentDataGridParameters);
                if (varietyDataModels == null)
                    varietyDataModels = new List<ItemAttributeVarietyLookupView>(response);  // not sure why we have to use a holding variable just following the demo code
                else
                {
                    varietyDataModels.Clear();
                    varietyDataModels.AddRange(response);
                }
            }
            catch (Exception ex)
            {
                AppLoggerManager.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            //restore the old items that were selected.
            selectedItemAttributeVarietyLookups = attributeVarietyWooLinkedViewRepository.PopSelectedItems(varietyDataModels);
            StateHasChanged();
        }
        //async Task HandleCustomerSearchOnKeyUp(KeyboardEventArgs kbEventHandler)
        //{
        //    var key = (string)kbEventHandler.Key;   // not using this but just in case
        //                                            //if (_WooLinkedGridSettings.CustomFilterValue.Length> 2)
        //                                            //{
        //    await varietiesDataGrid.Reload();
        //    //}
        //}

        async Task OnVarietyRowInserting(SavedRowItem<ItemAttributeVarietyLookupView, Dictionary<string, object>> insertedItem)
        {
            var newItem = insertedItem.Item;

            await attributeVarietyWooLinkedViewRepository.InsertRowAsync(newItem);
            await varietiesDataGrid.Reload();
        }
        private List<string> _listOfSymbols = null;
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

            await attributeVarietyWooLinkedViewRepository.InsertRowAsync(_newItem);
            await varietiesDataGrid.Reload();
        }
        void OnItemAttributeVarietyLookupNewItemDefaultSetter(ItemAttributeVarietyLookupView newItem)
        {
            //newItem =
            attributeVarietyWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        //async Task<int> UpdateItemAttributeVarietyLookup(ItemAttributeVarietyLookupView updatedItemView)
        //{
        //    int _result = await _AttributeVarietyWooLinkedViewRepository.UpdateItemAsync(updatedItemView);
        //    await varietiesDataGrid.Reload();
        //    return _result;
        //}

        async Task OnVarietyRowUpdatingAsync(SavedRowItem<ItemAttributeVarietyLookupView, Dictionary<string, object>> updatedItem)
        {
            var _updatedItem = updatedItem.Item;

            await attributeVarietyWooLinkedViewRepository.UpdateRowAsync(_updatedItem);
            await varietiesDataGrid.Reload();
        }
        async Task OnVarietyRowRemovingAsync(CancellableRowChange<ItemAttributeVarietyLookupView> modelItem)
        {
            // set the Selected Item Attribute for use later
            selectedItemAttributeVarietyLookup = modelItem.Item;
            var deleteItem = modelItem;
            await attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.DeleteConfirmationWithOption.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.VarietyName}?", selectedItemAttributeVarietyLookup.HasECommerceAttributeVarietyMap);  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmVarietyAddWooItem_Click(bool confirmClicked)
        {
            if (confirmClicked)
            {
                // they want to add the item to Woo 
                await attributeVarietyWooLinkedViewRepository.AddWooItemAndMapAsync(selectedItemAttributeVarietyLookup);
                await varietiesDataGrid.Reload();
            }
        }
        async Task ConfirmVarietyDeleteWooItem_Click(bool confirmClicked)
        {
            // they want to delete the item to Woo 
            isLoading = true; // tell grid not to reload we are busy
            await attributeVarietyWooLinkedViewRepository.DeleteWooItemAsync(selectedItemAttributeVarietyLookup.ItemAttributeVarietyLookupId, confirmClicked);
            //  regardless of how we got here they wanted to delete the original Attribute so delete it now, but only after Woo delete if they wanted it deleted.
            await attributeVarietyWooLinkedViewRepository.DeleteRowAsync(selectedItemAttributeVarietyLookup);
            isLoading = false; // tell grid not to reload we are busy
            await varietiesDataGrid.Reload();
        }
        //protected async Task
        async Task ConfirmVarietyDelete_Click(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooAttribute and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    await attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.DeleteWooItemConfirmation.ShowModalAsync("Are you sure?", $"Delete {selectedItemAttributeVarietyLookup.VarietyName} from Woo too?", "Delete", "Cancel");
                else
                    await attributeVarietyWooLinkedViewRepository.DeleteRowAsync(selectedItemAttributeVarietyLookup);

            }
            await varietiesDataGrid.Reload();
        }
        public async Task OnVarietyRowRemoved(ItemAttributeVarietyLookupView modelItem)
        {
            //await InvokeAsync(StateHasChanged);
            await varietiesDataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
                                               // await InvokeAsync(StateHasChanged);
        }
        async Task DoGroupAction()
        {
            //if (SelectedBulkAction == BulkAction.none)
            //    return;   ----> button should be disabled 

            await attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");

            int _done = 0;
            int _failed = 0;
            foreach (var item in selectedItemAttributeVarietyLookups)
            {
                if (await attributeVarietyWooLinkedViewRepository.DoGroupActionAsync(item, selectedBulkAction) > 0)
                    _done++;
                else
                    _failed++;
            }
            await attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {_done} items and not applied to {_failed} items.");
            await varietiesDataGrid.Reload();
        }
        async Task ReloadAsync()
        {
            attributeVarietyWooLinkedViewRepository.CurrWooLinkedGridSettings.CurrentPage = 1;
            await varietiesDataGrid.Reload();
        }
        public async Task SetParentAttributeId(Guid newAtttributeID)
        {
            ParentItemAttributeLookupId = newAtttributeID;
            attributeVarietyWooLinkedViewRepository.SetParentAttributeId(newAtttributeID);
            await ReloadAsync();
        }
        #endregion
    }
}
