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
        // Interface Stuff
        public GridSettings _GridSettings = new GridSettings();
        public ItemView SelectedItemRow = null;
        public BulkAction SelectedBulkAction = BulkAction.none;
        // variables / Models
        public List<ItemView> dataModels = null;
        public ItemView seletectedItem = null;

        public bool GroupButtonEnabled = true;
        private bool IsLoading = false;
        private bool ShowItemDetail = false;
        private bool ShowReplaceItem = false;
        private bool ShowWooLinked = true;
        private string _Status = "";
        DataGrid<ItemView> _DataGrid;

        // All there workings are here
        IItemWooLinkedView _ItemWooLinkedViewRepository;

        public List<ItemView> SelectedItemRows;
        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        ApplicationState _AppState { get; set; }
        [Inject]
        public ILoggerManager _Logger { get; set; }
        [Inject]
        public IMapper _Mapper { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _ItemWooLinkedViewRepository = new ItemWooLinkedViewRepository(_Logger, _AppUnitOfWork, _GridSettings, _Mapper);
            //await LoadData();
            await InvokeAsync(StateHasChanged);
        }
        private async Task SetLoadStatus(string statusString)
        {
            _Status = statusString;
            await InvokeAsync(StateHasChanged);
        }
        public async Task LoadData()
        {
            await HandleReadDataAsync(new DataGridReadDataEventArgs<ItemView>(_GridSettings.CurrentPage, _GridSettings.PageSize, null, System.Threading.CancellationToken.None));
        }

        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemView> inputDataGridReadData)
        {
            if (IsLoading)
                return;

            IsLoading = true;
            // 

            if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
            {
                DataGridParameters _dataGridParameters = _ItemWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, _GridSettings.CustomFilterValue);
                if (_GridSettings.PageSize != inputDataGridReadData.PageSize)
                { /// page sized changed so jump back to original page
                    _GridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                    _GridSettings.PageSize = inputDataGridReadData.PageSize;
                    //                  await Reload();
                }
                await SetLoadStatus("Checking Woo status & loading Attributes");
                try
                {
                    await SetLoadStatus("Checking Woo status");
                    _GridSettings.WooIsActive = await _ItemWooLinkedViewRepository.WooIsActiveAsync(_AppState);
                    ShowWooLinked = ShowWooLinked && _GridSettings.WooIsActive;  // show woo link is selected and woo is active.
                    await SetLoadStatus("Loading Attributes");
                    await LoadItemList(_dataGridParameters);
                }
                catch (Exception ex)
                {
                    _Logger.LogError($"Error running async tasks: {ex.Message}");
                    throw;
                }
                _Status = string.Empty;
            }
            IsLoading = false;
        }

        private async Task LoadItemList(DataGridParameters currentDataGridParameters)
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
                _Logger.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            //restore the old items that were selected.
            SelectedItemRows = _ItemWooLinkedViewRepository.PopSelectedItems(dataModels);
            StateHasChanged();
        }
        async Task HandleCustomerSearchOnKeyUp(KeyboardEventArgs kbEventHandler)
        {
            var key = (string)kbEventHandler.Key;   // not using this but just in case
                                                    //if (_gridSettings.CustomFilterValue.Length > 2)
                                                    //{
            await _DataGrid.Reload();
            //}
        }
        async Task OnRowInserting(SavedRowItem<ItemView, Dictionary<string, object>> insertedItem)
        {
            var _newItem = insertedItem.Item;

            await _ItemWooLinkedViewRepository.InsertRowAsync(_newItem);
            await _DataGrid.Reload();
        }
        void OnItemNewItemDefaultSetter(ItemView newItem) //Item pNewCatItem)
        {
            newItem = _ItemWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        async Task<int> UpdateItem(ItemView UpdatedCatItemView)
        {
            int _result = await _ItemWooLinkedViewRepository.UpdateItemAsync(UpdatedCatItemView);
            await _DataGrid.Reload();
            return _result;
        }
        async Task OnRowUpdating(SavedRowItem<ItemView, Dictionary<string, object>> updatedItem)
        {
            await _ItemWooLinkedViewRepository.UpdateRowAsync(updatedItem.Item);
            await _DataGrid.Reload();
        }
        void OnRowRemoving(CancellableRowChange<ItemView> modelItem)
        {
            // set the Selected Item Attribute for use later
            SelectedItemRow = modelItem.Item;
            var deleteItem = modelItem;
            _GridSettings.DeleteConfirmation.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemName}?", SelectedItemRow.HasECommerceAttributeMap);  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmAddWooItem_Click(bool confirmClick)
        {
            if (confirmClick)
            {
                // they want to add the item to Woo 
                await _ItemWooLinkedViewRepository.AddWooItemAndMapAsync(SelectedItemRow);
                await _DataGrid.Reload();
            }
        }
        async Task ConfirmDeleteWooItem_Click(bool confirmClick)
        {
            // they want to delete the item to Woo 
            await _ItemWooLinkedViewRepository.DeleteWooItemAsync(SelectedItemRow.ItemId, confirmClick);
            //  regardless of how we got here they wanted to delete the original Attribute so delete it now, but only after Woo delete if they wanted it deleted.
            await _ItemWooLinkedViewRepository.DeleteRowAsync(SelectedItemRow);
            await _DataGrid.Reload();
        }
        //protected async Task
        async Task ConfirmDelete_Click(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooAttribute and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    _GridSettings.DeleteWooItemConfirmation.ShowModal("Are you sure?", $"Delete {SelectedItemRow.ItemName} from Woo too?", "Delete", "Cancel");
                else
                    await _ItemWooLinkedViewRepository.DeleteRowAsync(SelectedItemRow);

            }
            await _DataGrid.Reload();
        }
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
        async Task DoGroupAction()
        {
            _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");
            int done = 0;
            int failed = 0;
            foreach (var item in SelectedItemRows)
            {
                if (await _ItemWooLinkedViewRepository.DoGroupActionAsync(item, SelectedBulkAction) > 0)
                    done++;
                else
                    failed++;
            }
            _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {done} items and not applied to {failed} items.");
            await _DataGrid.Reload();
        }
        async Task Reload()
        {
            _GridSettings.CurrentPage = 1;
            await _DataGrid.Reload();
        }

        //private NewItemAttributeVarietyComponent NewAttributeVariety;
    }
}
