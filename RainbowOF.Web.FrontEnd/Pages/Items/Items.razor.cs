using AutoMapper;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RainbowOF.Components.Modals;
using RainbowOF.Models.System;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Items;
using RainbowOF.Tools;
using RainbowOF.Tools.Services;
using RainbowOF.ViewModels.Common;
using RainbowOF.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class Items : ComponentBase
    {
        #region Injected classes
        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        [Inject]
        public ApplicationState AppState { get; set; }
        [Inject]
        public ILoggerManager AppLoggerManager { get; set; }
        [Inject]
        public IMapper AppMapper { get; set; }
        #endregion
        #region Interface variables Stuff
        //public GridSettings _WooLinkedGridSettings = new GridSettings();
        private ItemView selectedItemRow { get; set; } = null;
        private ItemView seletectedItem { get; set; } = null;
        private BulkAction selectedBulkAction { get; set; } = BulkAction.none;
        private List<ItemView> selectedItemRows { get; set; } = null;
        #endregion
        #region // Models variables
        private List<ItemView> dataModels { get; set; } = null;
        //public bool GroupButtonEnabled = true;
        private bool isLoading { get; set; } = false;
        private bool showItemDetail { get; set; } = false;
        private bool showReplaceItem { get; set; } = false;
        private bool showWooLinked { get; set; } = true;
        private string status { get; set; } = "";
        private DataGrid<ItemView> dataGrid { get; set; }
        private IItemWooLinkedView itemWooLinkedViewRepository { get; set; } = null;
        #endregion
        #region Support methods
        // All there workings are here
        /// <summary>
        /// Initialises Item Woo Linked View Repository, and updates the page
        /// </summary>
        /// <returns>void</returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            itemWooLinkedViewRepository = new ItemWooLinkedViewRepository(AppLoggerManager, AppUnitOfWork,/* _WooLinkedGridSettings,*/ AppMapper);
            //await LoadData();
            await InvokeAsync(StateHasChanged);
        }
        /// <summary>
        /// Sets the Status string and updates the DOM with that string. Logs the change in status
        /// </summary>
        /// <param name="statusString"></param>
        /// <returns>void</returns>
        private async Task SetLoadStatusAsync(string statusString)
        {
            status = statusString;
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Items - Status changed to: {statusString}");
            await InvokeAsync(StateHasChanged);
        }
        /// <summary>
        /// Load the actual Items + View additional values into the data model using the current data grid parameters. Also saves the current selected lists and restores it.
        /// </summary>
        /// <param name="currentDataGridParameters">Grid parameters</param>
        /// <returns>void</returns>
        private async Task LoadItemListAsync(DataGridParameters currentDataGridParameters)
        {
            // store old select items list
            try
            {
                var response = await itemWooLinkedViewRepository.LoadViewItemsPaginatedAsync(currentDataGridParameters);
                if (dataModels == null)
                    dataModels = new List<ItemView>(response);  // not sure why we have to use a holding variable just following the demo code
                else
                {
                    dataModels.Clear();
                    dataModels.AddRange(response);
                }
            }
            catch (Exception ex)
            {
                AppLoggerManager.LogError($"Items - Error running async tasks: {ex.Message}");
                throw;
            }
            //restore the old items that were selected.
            selectedItemRows = itemWooLinkedViewRepository.PopSelectedItems(dataModels);
            StateHasChanged();
        }
        /// <summary>
        /// Reload the Grid after resetting the page
        /// </summary>
        /// <returns>void</returns>
        async Task ReloadAsync()
        {

            itemWooLinkedViewRepository.CurrWooLinkedGridSettings.CurrentPage = 1;
            await dataGrid.Reload();
        }
        #endregion
        #region Interface methods
        /// <summary>
        /// Handle to loading of the Item data using paging, supporting searching and sorting. Load Item List.
        /// Logic: Store the current selected items then retrieve details form grid then restore the selected items.
        /// </summary>
        /// <param name="inputDataGridReadData">Paging, sorting and filtering info</param>
        /// <returns></returns>
        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemView> inputDataGridReadData)
        {
            if (isLoading)
                return;
            isLoading = true;
            // 
            if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
            {
                DataGridParameters _dataGridParameters = itemWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, itemWooLinkedViewRepository.CurrWooLinkedGridSettings.CustomFilterValue);
                if (itemWooLinkedViewRepository.CurrWooLinkedGridSettings.PageSize != inputDataGridReadData.PageSize)
                { /// page sized changed so jump back to original page
                    itemWooLinkedViewRepository.CurrWooLinkedGridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                    itemWooLinkedViewRepository.CurrWooLinkedGridSettings.PageSize = inputDataGridReadData.PageSize;
                    //                  await Reload();
                }
                await SetLoadStatusAsync("Checking Woo status & loading Attributes");
                try
                {
                    await SetLoadStatusAsync("Checking Woo status");
                    itemWooLinkedViewRepository.CurrWooLinkedGridSettings.WooIsActive = await itemWooLinkedViewRepository.WooIsActiveAsync(AppState);
                    showWooLinked = showWooLinked && itemWooLinkedViewRepository.CurrWooLinkedGridSettings.WooIsActive;  // show woo link is selected and woo is active.
                    await SetLoadStatusAsync("Loading Attributes");
                    await LoadItemListAsync(_dataGridParameters);
                }
                catch (Exception ex)
                {
                    AppLoggerManager.LogError($"Items - Error running async tasks: {ex.Message}");
                    throw;
                }
                status = string.Empty;
            }
            isLoading = false;
        }
        /// <summary>
        /// Handle Customer Search On Key UP - essentially adds the key stroke to the filter.
        /// </summary>
        /// <param name="kbEventHandler"></param>
        /// <returns></returns>
        async Task HandleCustomerSearchOnKeyUpAsync(KeyboardEventArgs kbEventHandler)
        {
            _ = (string)kbEventHandler.Key;   // not using this but just in case
                                              //if (_ItemWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue.Length> 2)
                                              //{
            await dataGrid.Reload();
            //}
        }
        /// <summary>
        /// Handle the user confirmation asking to add the Woo Mapping
        /// </summary>
        /// <param name="confirmClick">Did the user click confirm</param>
        /// <returns></returns>
        async Task ConfirmAddWooItem_ClickAsync(bool clickConfirmed)
        {
            if (clickConfirmed)
            {
                // they want to add the item to Woo 
                await itemWooLinkedViewRepository.AddWooItemAndMapAsync(selectedItemRow);
                await dataGrid.Reload();
            }
            // Should we mark that this item was not linked?
        }
        /// <summary>
        /// When a row is inserted save that row and reload the grid
        /// </summary>
        /// <param name="insertedItem"></param>
        /// <returns>void</returns>
        async Task OnRowInsertingAsync(SavedRowItem<ItemView, Dictionary<string, object>> insertedItem)
        {
            var _newItem = insertedItem.Item;

            await itemWooLinkedViewRepository.InsertRowAsync(_newItem);
            await dataGrid.Reload();
        }
        /// <summary>
        /// When a new item is created initialise the newItem.
        /// </summary>
        /// <param name="newItem">The item to be created (essentially it is passed by ref</param>
        void OnItemNewItemDefaultSetter(ItemView newItem) //Item pNewCatItem)
        {
            //newItem =
            itemWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        /// <summary>
        /// Update the item that is passed in.
        /// </summary>
        /// <param name="updatedCatItemView">the ItemView to be updated</param>
        /// <returns>int value> 0 for success or -1 for error </returns>
        //async Task<int> UpdateItemAsync(ItemView updatedCatItemView)
        //{
        //    int _result = await _ItemWooLinkedViewRepository.UpdateItemAsync(updatedCatItemView);
        //    await _DataGrid.Reload();
        //    return _result;
        //}
        /// <summary>
        /// On row updated - update the row (using Update Row Async)
        /// </summary>
        /// <param name="updatedItem"></param>
        /// <returns></returns>
        async Task OnRowUpdatedAsync(SavedRowItem<ItemView, Dictionary<string, object>> updatedItem)
        {
            await itemWooLinkedViewRepository.UpdateRowAsync(updatedItem.Item);
            //->>> show we do error checking?
            //await _DataGrid.Reload();
        }
        /// <summary>
        /// On row removing async - Launch confirmation dialog, so an item can be deleted if the user wants
        /// </summary>
        /// <param name="modelItem">the model item to be delete, if confirmed</param>
        async Task OnRowRemovingAsync(CancellableRowChange<ItemView> modelItem)
        {
            // set the Selected Item Attribute for use later
            selectedItemRow = modelItem.Item;
            var deleteItem = modelItem;
            await itemWooLinkedViewRepository.CurrWooLinkedGridSettings.DeleteConfirmationWithOption.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemName}?", selectedItemRow.HasECommerceAttributeMap);  //,"Delete","Cancel"); - passed in on init
        }
        /// <summary>
        /// Deletion of the woo item is confirmed - so delete the product in Woo.
        /// </summary>
        /// <param name="confirmClick">Did the user confirm they want to delete it.</param>
        /// <returns></returns>
        async Task ConfirmDeleteWooItem_ClickAsync(bool confirmClick)
        {
            // they want to delete the item to Woo 
            await itemWooLinkedViewRepository.DeleteWooItemAsync(selectedItemRow.ItemId, confirmClick);
            //  regardless of how we got here they wanted to delete the original Attribute so delete it now, but only after Woo delete if they wanted it deleted.
            await itemWooLinkedViewRepository.DeleteRowAsync(selectedItemRow);
            await dataGrid.Reload();
        }
        /// <summary>
        /// Deletion of the item for system - and also if confirmed in Woo
        /// </summary>
        /// <param name="confirmationOption">Confirmation option passed by grid</param>
        /// <returns></returns>
        async Task ConfirmDeleteItem_Click(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooAttribute and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    await itemWooLinkedViewRepository.CurrWooLinkedGridSettings.DeleteWooItemConfirmation.ShowModalAsync("Are you sure?", $"Delete {selectedItemRow.ItemName} from Woo too?", "Delete", "Cancel");
                else
                    await itemWooLinkedViewRepository.DeleteRowAsync(selectedItemRow);

            }
            await dataGrid.Reload();
        }
        /// <summary>
        /// When a row is removed handle this
        /// </summary>
        /// <param name="modelItem">The ItemView to that was removed.</param>
        /// <returns></returns>
        public async Task OnRowRemoved(ItemView modelItem)
        {
            await InvokeAsync(StateHasChanged);
            await dataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
        }

        private Dictionary<OrderBys, string> _listOfOrderBys = null;
        public Dictionary<OrderBys, string> GetListOfOrderBys()
        {
            if (_listOfOrderBys == null)
            {
                _listOfOrderBys = new Dictionary<OrderBys, string>();
                foreach (OrderBys obs in Enum.GetValues(typeof(OrderBys)))
                {
                    _listOfOrderBys.Add(obs, obs.ToString());
                }
            }
            return _listOfOrderBys;
        }
        /// <summary>
        /// Perform the group action as selected.
        /// </summary>
        /// <returns>void</returns>
        async Task DoGroupAction()
        {
            await itemWooLinkedViewRepository.CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");
            int done = 0;
            int failed = 0;
            foreach (var item in selectedItemRows)
            {
                if (await itemWooLinkedViewRepository.DoGroupActionAsync(item, selectedBulkAction) > 0)
                    done++;
                else
                    failed++;
            }
            await itemWooLinkedViewRepository.CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {done} items and not applied to {failed} items.");
            await dataGrid.Reload();
        }
        #endregion
        //private NewItemAttributeVarietyComponent NewAttributeVariety;
    }
}
