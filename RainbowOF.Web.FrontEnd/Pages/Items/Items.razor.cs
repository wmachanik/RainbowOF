using AutoMapper;
using Blazorise;
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
using RainbowOF.Web.FrontEnd.Pages.ChildComponents.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class Items : ComponentBase
    {
        #region // Interface variables Stuff
        //public GridSettings _WooLinkedGridSettings = new GridSettings();
        private ItemView _SelectedItemRow = null;
        public ItemView seletectedItem = null;
        public BulkAction SelectedBulkAction = BulkAction.none;
        public List<ItemView> SelectedItemRows = null;
        #endregion
        #region // Models variables
        public List<ItemView> dataModels = null;
        //public bool GroupButtonEnabled = true;
        private bool _IsLoading = false;
        private bool _ShowItemDetail = false;
        private bool _ShowReplaceItem = false;
        private bool _ShowWooLinked = true;
        private string _Status = "";
        private DataGrid<ItemView> _DataGrid;
        private IItemWooLinkedView _ItemWooLinkedViewRepository;
        #endregion
        #region Injected classes
        [Inject]
        IUnitOfWork appUnitOfWork { get; set; }
        [Inject]
        ApplicationState _AppState { get; set; }
        [Inject]
        public ILoggerManager appLoggerManager { get; set; }
        [Inject]
        public IMapper _Mapper { get; set; }
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
            _ItemWooLinkedViewRepository = new ItemWooLinkedViewRepository(appLoggerManager, appUnitOfWork,/* _WooLinkedGridSettings,*/ _Mapper);
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
            _Status = statusString;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Items - Status changed to: {statusString}");
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
                var response = await _ItemWooLinkedViewRepository.LoadViewItemsPaginatedAsync(currentDataGridParameters);
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
                appLoggerManager.LogError( $"Items - Error running async tasks: {ex.Message}");
                throw;
            }
            //restore the old items that were selected.
            SelectedItemRows = _ItemWooLinkedViewRepository.PopSelectedItems(dataModels);
            StateHasChanged();
        }
        /// <summary>
        /// Reload the Grid after resetting the page
        /// </summary>
        /// <returns>void</returns>
        async Task ReloadAsync()
        {

            _ItemWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage = 1;
            await _DataGrid.Reload();
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
            if (_IsLoading)
                return;
            _IsLoading = true;
            // 
            if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
            {
                DataGridParameters _dataGridParameters = _ItemWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, _ItemWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue);
                if (_ItemWooLinkedViewRepository._WooLinkedGridSettings.PageSize != inputDataGridReadData.PageSize)
                { /// page sized changed so jump back to original page
                    _ItemWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                    _ItemWooLinkedViewRepository._WooLinkedGridSettings.PageSize = inputDataGridReadData.PageSize;
                    //                  await Reload();
                }
                await SetLoadStatusAsync("Checking Woo status & loading Attributes");
                try
                {
                    await SetLoadStatusAsync("Checking Woo status");
                    _ItemWooLinkedViewRepository._WooLinkedGridSettings.WooIsActive = await _ItemWooLinkedViewRepository.WooIsActiveAsync(_AppState);
                    _ShowWooLinked = _ShowWooLinked && _ItemWooLinkedViewRepository._WooLinkedGridSettings.WooIsActive;  // show woo link is selected and woo is active.
                    await SetLoadStatusAsync("Loading Attributes");
                    await LoadItemListAsync(_dataGridParameters);
                }
                catch (Exception ex)
                {
                    appLoggerManager.LogError($"Items - Error running async tasks: {ex.Message}");
                    throw;
                }
                _Status = string.Empty;
            }
            _IsLoading = false;
        }
        /// <summary>
        /// Handle Customer Search On Key UP - essentially adds the key stroke to the filter.
        /// </summary>
        /// <param name="kbEventHandler"></param>
        /// <returns></returns>
        async Task HandleCustomerSearchOnKeyUpAsync(KeyboardEventArgs kbEventHandler)
        {
            var key = (string)kbEventHandler.Key;   // not using this but just in case
                                                    //if (_ItemWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue.Length> 2)
                                                    //{
            await _DataGrid.Reload();
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
                await _ItemWooLinkedViewRepository.AddWooItemAndMapAsync(_SelectedItemRow);
                await _DataGrid.Reload();
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

            await _ItemWooLinkedViewRepository.InsertRowAsync(_newItem);
            await _DataGrid.Reload();
        }
        /// <summary>
        /// When a new item is created initialise the newItem.
        /// </summary>
        /// <param name="newItem">The item to be created (essentially it is passed by ref</param>
        void OnItemNewItemDefaultSetter(ItemView newItem) //Item pNewCatItem)
        {
            newItem = _ItemWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        /// <summary>
        /// Update the item that is passed in.
        /// </summary>
        /// <param name="updatedCatItemView">the ItemView to be updated</param>
        /// <returns>int value> 0 for success or -1 for error </returns>
        async Task<int> UpdateItemAsync(ItemView updatedCatItemView)
        {
            int _result = await _ItemWooLinkedViewRepository.UpdateItemAsync(updatedCatItemView);
            await _DataGrid.Reload();
            return _result;
        }
        /// <summary>
        /// On row updated - update the row (using Update Row Async)
        /// </summary>
        /// <param name="updatedItem"></param>
        /// <returns></returns>
        async Task OnRowUpdatedAsync(SavedRowItem<ItemView, Dictionary<string, object>> updatedItem)
        {
            await _ItemWooLinkedViewRepository.UpdateRowAsync(updatedItem.Item);
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
            _SelectedItemRow = modelItem.Item;
            var deleteItem = modelItem;
            await _ItemWooLinkedViewRepository._WooLinkedGridSettings.DeleteConfirmationWithOption.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemName}?", _SelectedItemRow.HasECommerceAttributeMap);  //,"Delete","Cancel"); - passed in on init
        }
        /// <summary>
        /// Deletion of the woo item is confirmed - so delete the product in Woo.
        /// </summary>
        /// <param name="confirmClick">Did the user confirm they want to delete it.</param>
        /// <returns></returns>
        async Task ConfirmDeleteWooItem_ClickAsync(bool confirmClick)
        {
            // they want to delete the item to Woo 
            await _ItemWooLinkedViewRepository.DeleteWooItemAsync(_SelectedItemRow.ItemId, confirmClick);
            //  regardless of how we got here they wanted to delete the original Attribute so delete it now, but only after Woo delete if they wanted it deleted.
            await _ItemWooLinkedViewRepository.DeleteRowAsync(_SelectedItemRow);
            await _DataGrid.Reload();
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
                    await _ItemWooLinkedViewRepository._WooLinkedGridSettings.DeleteWooItemConfirmation.ShowModalAsync("Are you sure?", $"Delete {_SelectedItemRow.ItemName} from Woo too?", "Delete", "Cancel");
                else
                    await _ItemWooLinkedViewRepository.DeleteRowAsync(_SelectedItemRow);

            }
            await _DataGrid.Reload();
        }
        /// <summary>
        /// When a row is removed handle this
        /// </summary>
        /// <param name="modelItem">The ItemView to that was removed.</param>
        /// <returns></returns>
        public async Task OnRowRemoved(ItemView modelItem)
        {
            await InvokeAsync(StateHasChanged);
            await _DataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
        }

        Dictionary<OrderBys, string> _listOfOrderBys = null;
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
            await _ItemWooLinkedViewRepository._WooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");
            int done = 0;
            int failed = 0;
            foreach (var item in SelectedItemRows)
            {
                if (await _ItemWooLinkedViewRepository.DoGroupActionAsync(item, SelectedBulkAction)> 0)
                    done++;
                else
                    failed++;
            }
            await _ItemWooLinkedViewRepository._WooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {done} items and not applied to {failed} items.");
            await _DataGrid.Reload();
        }
        #endregion
        //private NewItemAttributeVarietyComponent NewAttributeVariety;
    }
}
