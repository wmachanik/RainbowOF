﻿using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.System;
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
    public partial class ItemAttributes : ComponentBase
    {
        // Interface Stuff
        public GridSettings _gridSettings = new GridSettings();
        public ItemAttributeLookupView SelectedItemAttributeLookup = null;
        public BulkAction SelectedBulkAction = BulkAction.none;
        // variables / Models
        public List<ItemAttributeLookupView> dataModels = null;
        public ItemAttributeLookupView seletectedItem = null;
        //public const string disabledStr = "N";
        //public const string enabledStr = "Y";

        public bool GroupButtonEnabled = true;
        private bool IsLoading = false;
        private string _Status = "";
        DataGrid<ItemAttributeLookupView> _DataGrid;

        // All there workings are here
        IAttributeWooLinkedView _AttributeWooLinkedViewRepository;

        public List<ItemAttributeLookupView> SelectedItemAttributeLookups;
        [Inject]
        IAppUnitOfWork _appUnitOfWork { get; set; }
        [Inject]
        ApplicationState _appState { get; set; }
        [Inject]
        public ILoggerManager _logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _AttributeWooLinkedViewRepository = new AttributeWooLinkedViewRepository(_logger, _appUnitOfWork, _gridSettings);
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
            await HandleReadDataAsync(new DataGridReadDataEventArgs<ItemAttributeLookupView>(_gridSettings.CurrentPage, _gridSettings.PageSize, null, System.Threading.CancellationToken.None));
        }

        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemAttributeLookupView> inputDataGridReadData)
        {
           if (IsLoading)
                return;

            IsLoading = true;
            // 

            if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
            {
                DataGridParameters _dataGridParameters = _AttributeWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, _gridSettings.CustomFilterValue);
                if (_gridSettings.PageSize != inputDataGridReadData.PageSize)
                { /// page sized changed so jump back to original page
                    _gridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                    _gridSettings.PageSize = inputDataGridReadData.PageSize;
                    //                  await Reload();
                }
                await SetLoadStatus("Checking Woo status & loading Attributes");
                try
                {
                    await SetLoadStatus("Checking Woo status");
                    _gridSettings.WooIsActive = await _AttributeWooLinkedViewRepository.WooIsActive(_appState);
                    await SetLoadStatus("Loading Attributes");
                    await LoadItemAttributeLookupList(_dataGridParameters); 
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

        private async Task LoadItemAttributeLookupList(DataGridParameters currentDataGridParameters) 
        {
            // store old select items list
            try
            {
                var response = await _AttributeWooLinkedViewRepository.LoadViewItemsPaginatedAsync(currentDataGridParameters);  
                if (dataModels == null)
                    dataModels = new List<ItemAttributeLookupView>(response);  // not sure why we have to use a holding variable just following the demo code
                else
                {
                    dataModels.Clear();
                    dataModels.AddRange(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            //restore the old items that were selected.
            SelectedItemAttributeLookups = _AttributeWooLinkedViewRepository.PopSelectedItems(dataModels);
            StateHasChanged();
        }

        //private async Task<List<ItemAttributeLookup>> GetAllItemAttributeLookups()
        //{
        //    IAppRepository<ItemAttributeLookup> _ItemAttributeLookupRepository = _appUnitOfWork.Repository<ItemAttributeLookup>();
        //    List<ItemAttributeLookup> _ItemAttributeLookups = (await _ItemAttributeLookupRepository.GetAllEagerAsync(ial => ial.ItemAttributeVarietyLookups))
        //        .OrderBy(ial => ial.OrderBy)
        //        .ToList();

        //    return _ItemAttributeLookups;
        //}

        async Task HandleCustomerSearchOnKeyUp(KeyboardEventArgs kbEventHandler)
        {
            var key = (string)kbEventHandler.Key;   // not using this but just in case
                                                    //if (_gridSettings.CustomFilterValue.Length > 2)
                                                    //{
            await _DataGrid.Reload();
            //}
        }
        async Task OnRowInserting(SavedRowItem<ItemAttributeLookupView, Dictionary<string, object>> pInsertedItem)
        {
            var newItem = pInsertedItem.Item;

            await _AttributeWooLinkedViewRepository.InsertRowAsync(newItem);
            await _DataGrid.Reload();
        }
        void OnItemAttributeLookupNewItemDefaultSetter(ItemAttributeLookupView newItem) //ItemAttributeLookup pNewCatItem)
        {
            newItem = _AttributeWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        async Task<int> UpdateItemAttributeLookup(ItemAttributeLookupView UpdatedCatItemView)
        {
            int _result = await _AttributeWooLinkedViewRepository.UpdateItemAsync(UpdatedCatItemView);
            await _DataGrid.Reload();
            return _result;
        }
        async Task OnRowUpdating(SavedRowItem<ItemAttributeLookupView, Dictionary<string, object>> pUpdatedItem)
        {
            await _AttributeWooLinkedViewRepository.UpdateRowAsync(pUpdatedItem.Item);
            await _DataGrid.Reload();
        }
        void OnRowRemoving(CancellableRowChange<ItemAttributeLookupView> modelItem)
        {
            // set the Selected Item Attribute for use later
            SelectedItemAttributeLookup = modelItem.Item;
            var deleteItem = modelItem;
            _gridSettings.DeleteConfirmation.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.AttributeName}?", SelectedItemAttributeLookup.HasWooAttributeMap);  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmAddWooItem_Click(bool confirm)
        {
            if (confirm)
            {
                // they want to add the item to Woo 
                await _AttributeWooLinkedViewRepository.AddWooItemAndMapAsync(SelectedItemAttributeLookup);
                await _DataGrid.Reload();
            }
        }
        async Task ConfirmDeleteWooItem_Click(bool confirm)
        {
            // they want to delete the item to Woo 
            await _AttributeWooLinkedViewRepository.DeleteWooItemAsync(SelectedItemAttributeLookup.ItemAttributeLookupId, confirm);
            //  regardless of how we got here they wanted to delete the original Attribute so delete it now, but only after Woo delete if they wanted it deleted.
            await _AttributeWooLinkedViewRepository.DeleteRowAsync(SelectedItemAttributeLookup);
            await _DataGrid.Reload();
        }
        //protected async Task
        async Task ConfirmDelete_Click(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooAttribute and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    _gridSettings.DeleteWooItemConfirmation.ShowModal("Are you sure?", $"Delete {SelectedItemAttributeLookup.AttributeName} from Woo too?", "Delete", "Cancel");
                else
                    await _AttributeWooLinkedViewRepository.DeleteRowAsync(SelectedItemAttributeLookup);

            }
            await _DataGrid.Reload();
        }
        public async Task OnRowRemoved(ItemAttributeLookupView modelItem)
        {
            await InvokeAsync(StateHasChanged);
            await _DataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
        }

        Dictionary<OrderBys, string> _ListOfOrderBys = null;
        public Dictionary<OrderBys, string> GetListOfOrderBys()
        {
            if (_ListOfOrderBys == null)
            {
                _ListOfOrderBys = new Dictionary<OrderBys, string>();
                foreach (OrderBys obs in Enum.GetValues(typeof(OrderBys)))
                {
                    _ListOfOrderBys.Add(obs, obs.ToString());
                }
            }
            return _ListOfOrderBys;
        }
        async Task DoGroupAction()
        {
            //if (SelectedBulkAction == BulkAction.none)
            //    return;   ----> button should be disabled 

            _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");

            int done = 0;
            int failed = 0;
            foreach (var item in SelectedItemAttributeLookups)
            {
                if (await _AttributeWooLinkedViewRepository.DoGroupActionAsync(item, SelectedBulkAction) > 0)
                    done++;
                else
                    failed++;
            }
            _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {done} items and not applied to {failed} items.");
            await _DataGrid.Reload();
        }
        async Task Reload()
        {
            _gridSettings.CurrentPage = 1;
            await _DataGrid.Reload();
        }

        //        #region VarietyGridStuff
        // ------------------------------------
        // All the Attribute Variety stuff
        // ------------------------------------
        private NewItemAttributeVarietyComponent NewAttributeVariety;

//            Modal NewAttributeVarietieModalRef;
        //        Deprecated
        // ------------------------------------
        //bool OnCustomFilter(ItemAttributeLookupView model)
        //{
        //    if (string.IsNullOrEmpty(_gridSettings.CustomFilterValue))
        //        return true;
        //    return
        //        model.AttributeName?.Contains(_gridSettings.CustomFilterValue, StringComparison.OrdinalIgnoreCase) == true
        //        || model.ParentAttribute?.AttributeName.Contains(_gridSettings.CustomFilterValue, StringComparison.OrdinalIgnoreCase) == true;
        //}
        //ItemAttributeLookup GetItemAttributeLookupItemFromView(ItemAttributeLookupView pItem)
        //{
        //    ItemAttributeLookup newItemAttributeLookup = new ItemAttributeLookup
        //    {
        //        ItemAttributeLookupId = pItem.ItemAttributeLookupId,
        //        AttributeName = pItem.AttributeName,
        //        OrderBy = pItem.OrderBy
        //        Notes = pItem.Notes,
        //    };

        //    return newItemAttributeLookup;
        //}

    }
}

