using AutoMapper;
using Blazorise;
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
using RainbowOF.Web.FrontEnd.Pages.ChildComponents.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemAttributes : ComponentBase
    {
        // Interface Stuff
        public GridSettings _GridSettings = new GridSettings();
        public ItemAttributeLookupView SelectedItemAttributeLookup = null;
        public BulkAction SelectedBulkAction = BulkAction.none;
        // variables / Models
        public List<ItemAttributeLookupView> dataModels = null;
        public ItemAttributeLookupView seletectedItem = null;

        public bool GroupButtonEnabled = true;
        private bool IsLoading = false;
        private string _Status = "";
        DataGrid<ItemAttributeLookupView> _DataGrid;

        // All there workings are here
        ItemWooLinkedView _AttributeWooLinkedViewRepository;

        public List<ItemAttributeLookupView> SelectedItemAttributeLookups;
        [Inject]
        public IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        public ApplicationState _AppState { get; set; }
        [Inject]
        public ILoggerManager _Logger { get; set; }
        [Inject]
        public IMapper _Mapper { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _AttributeWooLinkedViewRepository = new AttributeWooLinkedViewRepository(_Logger, _AppUnitOfWork, _GridSettings, _Mapper);
            //await LoadData();
            await InvokeAsync(StateHasChanged);
        }
        private async Task SetLoadStatus(string statusString)
        {
            _Status = statusString;
            await InvokeAsync(StateHasChanged);
        }
        //public async Task LoadData()
        //{
        //    //await HandleReadDataAsync(new DataGridReadDataEventArgs<ItemAttributeLookupView>(_GridSettings.CurrentPage, _GridSettings.PageSize, null, System.Threading.CancellationToken.None));
        //}

        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemAttributeLookupView> inputDataGridReadData)
        {
           if (IsLoading)
                return;
            IsLoading = true;
            // 
            try
            {
                if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
                {
                    DataGridParameters _dataGridParameters = _AttributeWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, _GridSettings.CustomFilterValue);
                    if (_GridSettings.PageSize != inputDataGridReadData.PageSize)
                    { /// page sized changed so jump back to original page
                        _GridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                        _GridSettings.PageSize = inputDataGridReadData.PageSize;
                        //                  await Reload();
                    }
                    await SetLoadStatus("Checking Woo status & loading Attributes");
                    await SetLoadStatus("Checking Woo status");
                    _GridSettings.WooIsActive = await _AttributeWooLinkedViewRepository.WooIsActiveAsync(_AppState);
                    await SetLoadStatus("Loading Attributes");
                    await LoadItemAttributeLookupList(_dataGridParameters);
                    _Status = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadItemAttributeLookupList(DataGridParameters currentDataGridParameters) 
        {
            // store old select items list
            try
            {
                var _response = await _AttributeWooLinkedViewRepository.LoadViewItemsPaginatedAsync(currentDataGridParameters);  
                if (dataModels == null)
                    dataModels = new List<ItemAttributeLookupView>(_response);  // not sure why we have to use a holding variable just following the demo code
                else
                {
                    dataModels.Clear();
                    dataModels.AddRange(_response);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError($"Error running async tasks: {ex.Message}");
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

        async Task HandleCustomerSearchOnKeyUpAsync(KeyboardEventArgs kbEventHandler)
        {
            var key = (string)kbEventHandler.Key;   // not using this but just in case
                                                    //if (_gridSettings.CustomFilterValue.Length > 2)
                                                    //{
            await _DataGrid.Reload();
            //}
        }
        async Task OnRowInsertingAsync(SavedRowItem<ItemAttributeLookupView, Dictionary<string, object>> insertedItem)
        {
            var newItem = insertedItem.Item;

            await _AttributeWooLinkedViewRepository.InsertRowAsync(newItem);
            await _DataGrid.Reload();
        }
        void OnItemAttributeLookupNewItemDefaultSetter(ItemAttributeLookupView newItem) //ItemAttributeLookup pNewCatItem)
        {
            newItem = _AttributeWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        async Task<int> UpdateItemAttributeLookupAsync(ItemAttributeLookupView updatedCatItemView)
        {
            int _result = await _AttributeWooLinkedViewRepository.UpdateItemAsync(updatedCatItemView);
            await _DataGrid.Reload();
            return _result;
        }
        async Task OnRowUpdatingAsync(SavedRowItem<ItemAttributeLookupView, Dictionary<string, object>> updatedItem)
        {
            await _AttributeWooLinkedViewRepository.UpdateRowAsync(updatedItem.Item);
            await _DataGrid.Reload();
        }
        void OnRowRemoving(CancellableRowChange<ItemAttributeLookupView> modelItem)
        {
            // set the Selected Item Attribute for use later
            SelectedItemAttributeLookup = modelItem.Item;
            var deleteItem = modelItem;
            _GridSettings.DeleteConfirmation.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.AttributeName}?", SelectedItemAttributeLookup.HasECommerceAttributeMap);  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmAddWooItem_ClickAsync(bool confirmClicked)
        {
            if (confirmClicked)
            {
                // they want to add the item to Woo 
                await _AttributeWooLinkedViewRepository.AddWooItemAndMapAsync(SelectedItemAttributeLookup);
                await _DataGrid.Reload();
            }
        }
        async Task ConfirmDeleteWooItem_ClickAsync(bool confirmClicked)
        {
            IsLoading = true;
            // they want to delete the item to Woo 
            await _AttributeWooLinkedViewRepository.DeleteWooItemAsync(SelectedItemAttributeLookup.ItemAttributeLookupId, confirmClicked);
            //  regardless of how we got here they wanted to delete the original Attribute so delete it now, but only after Woo delete if they wanted it deleted.
            await _AttributeWooLinkedViewRepository.DeleteRowAsync(SelectedItemAttributeLookup);
            IsLoading = false;
            await _DataGrid.Reload();
        }
        //protected async Task
        async Task ConfirmDelete_ClickAsync(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            IsLoading = true;
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooAttribute and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    _GridSettings.DeleteWooItemConfirmation.ShowModal("Are you sure?", $"Delete {SelectedItemAttributeLookup.AttributeName} from Woo too?", "Delete", "Cancel");
                else
                    await _AttributeWooLinkedViewRepository.DeleteRowAsync(SelectedItemAttributeLookup);

            }
            IsLoading = false;
            await _DataGrid.Reload();
        }
        public async Task OnRowRemovedAsync(ItemAttributeLookupView modelItem)
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
        async Task DoGroupActionAsync()
        {
            //if (SelectedBulkAction == BulkAction.none)
            //    return;   ----> button should be disabled 

            _GridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");

            int done = 0;
            int failed = 0;
            foreach (var item in SelectedItemAttributeLookups)
            {
                if (await _AttributeWooLinkedViewRepository.DoGroupActionAsync(item, SelectedBulkAction) > 0)
                    done++;
                else
                    failed++;
            }
            _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {done} items and not applied to {failed} items.");
            await _DataGrid.Reload();
        }
        async Task ReloadAsync()
        {
            _GridSettings.CurrentPage = 1;
            await _DataGrid.Reload();
        }

        //        #region VarietyGridStuff
        // ------------------------------------
        // All the Attribute Variety stuff
        // ------------------------------------
        private NewItemAttributeVarietyLookupComponent NewAttributeVariety;

//            Modal NewAttributeVarietyModalRef;
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

