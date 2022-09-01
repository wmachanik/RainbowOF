using AutoMapper;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RainbowOF.Components.Modals;
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
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemAttributes : ComponentBase
    {
        #region Injected and parameter values
        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        [Inject]
        public ApplicationState AppState { get; set; }
        [Inject]
        public ILoggerManager AppLoggerManager { get; set; }
        [Inject]
        public IMapper AppMapper { get; set; }
        #endregion
        #region Private and local variables
        // Interface Stuff
        //public GridSettings _WooLinkedGridSettings = new GridSettings();
        private ItemAttributeLookupView selectedItemAttributeLookup { get; set; } = null;
        private BulkAction selectedBulkAction { get; set; } = BulkAction.none;
        // variables / Models
        private List<ItemAttributeLookupView> dataModels { get; set; } = null;
        private ItemAttributeLookupView seletectedItem { get; set; } = null;
        private NewItemAttributeVarietyLookupComponent newAttributeVariety { get; set; }
        //private bool groupButtonEnabled = true;
        private bool isLoading { get; set; } = false;
        private string currStatus { get; set; } = "";
        private DataGrid<ItemAttributeLookupView> dataGrid { get; set; }
        // All there workings are here
        private IItemWooLinkedView attributeWooLinkedViewRepository { get; set; }
        private List<ItemAttributeLookupView> selectedItemAttributeLookups { get; set; }
        #endregion
        #region Initialisation
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            attributeWooLinkedViewRepository = new AttributeWooLinkedViewRepository(AppLoggerManager, AppUnitOfWork, /*_WooLinkedGridSettings, */AppMapper);
            //await LoadData();
            await InvokeAsync(StateHasChanged);
        }
        private async Task SetLoadStatus(string statusString)
        {
            currStatus = statusString;
            await InvokeAsync(StateHasChanged);
        }
        //public async Task LoadData()
        //{
        //    //await HandleReadDataAsync(new DataGridReadDataEventArgs<ItemAttributeLookupView>( _AttributeWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage,  _AttributeWooLinkedViewRepository._WooLinkedGridSettings.PageSize, null, System.Threading.CancellationToken.None));
        //}
        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemAttributeLookupView> inputDataGridReadData)
        {
            if (isLoading)
                return;
            isLoading = true;
            // 
            try
            {
                if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
                {
                    DataGridParameters _dataGridParameters = attributeWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, attributeWooLinkedViewRepository.CurrWooLinkedGridSettings.CustomFilterValue);
                    if (attributeWooLinkedViewRepository.CurrWooLinkedGridSettings.PageSize != inputDataGridReadData.PageSize)
                    { /// page sized changed so jump back to original page
                        attributeWooLinkedViewRepository.CurrWooLinkedGridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                        attributeWooLinkedViewRepository.CurrWooLinkedGridSettings.PageSize = inputDataGridReadData.PageSize;
                        //                  await Reload();
                    }
                    await SetLoadStatus("Checking Woo status & loading Attributes");
                    await SetLoadStatus("Checking Woo status");
                    attributeWooLinkedViewRepository.CurrWooLinkedGridSettings.WooIsActive = await attributeWooLinkedViewRepository.WooIsActiveAsync(AppState);
                    await SetLoadStatus("Loading Attributes");
                    await LoadItemAttributeLookupList(_dataGridParameters);
                    currStatus = string.Empty;
                }
            }
            catch (Exception ex)
            {
                AppLoggerManager.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task LoadItemAttributeLookupList(DataGridParameters currentDataGridParameters)
        {
            // store old select items list
            try
            {
                var _response = await attributeWooLinkedViewRepository.LoadViewItemsPaginatedAsync(currentDataGridParameters);
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
                AppLoggerManager.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            //restore the old items that were selected.
            selectedItemAttributeLookups = attributeWooLinkedViewRepository.PopSelectedItems(dataModels);
            StateHasChanged();
        }

        //private async Task<List<ItemAttributeLookup>> GetAllItemAttributeLookups()
        //{
        //    IAppRepository<ItemAttributeLookup> _ItemAttributeLookupRepository = AppUnitOfWork.Repository<ItemAttributeLookup>();
        //    List<ItemAttributeLookup> _ItemAttributeLookups = (await _ItemAttributeLookupRepository.GetAllEagerAsync(ial => ial.ItemAttributeVarietyLookups))
        //        .OrderBy(ial => ial.OrderBy)
        //        .ToList();

        //    return _ItemAttributeLookups;
        //}

        async Task HandleCustomerSearchOnKeyUpAsync(KeyboardEventArgs kbEventHandler)
        {
            _ = (string)kbEventHandler.Key;   // not using this but just in case
                                              //if ( _AttributeWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue.Length> 2)
                                              //{
            await dataGrid.Reload();
            //}
        }
        async Task OnRowInsertingAsync(SavedRowItem<ItemAttributeLookupView, Dictionary<string, object>> insertedItem)
        {
            var newItem = insertedItem.Item;

            await attributeWooLinkedViewRepository.InsertRowAsync(newItem);
            await dataGrid.Reload();
        }
        void OnItemAttributeLookupNewItemDefaultSetter(ItemAttributeLookupView newItem) //ItemAttributeLookup pNewCatItem)
        {
            //newItem =
            attributeWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        //async Task<int> UpdateItemAttributeLookupAsync(ItemAttributeLookupView updatedCatItemView)
        //{
        //    int _result = await _AttributeWooLinkedViewRepository.UpdateItemAsync(updatedCatItemView);
        //    await _DataGrid.Reload();
        //    return _result;
        //}
        async Task OnRowUpdatedAsync(SavedRowItem<ItemAttributeLookupView, Dictionary<string, object>> updatedItem)
        {
            await attributeWooLinkedViewRepository.UpdateRowAsync(updatedItem.Item);
            await dataGrid.Reload();
        }
        async Task OnRowRemovingAsync(CancellableRowChange<ItemAttributeLookupView> modelItem)
        {
            // set the Selected Item Attribute for use later
            selectedItemAttributeLookup = modelItem.Item;
            var deleteItem = modelItem;
            await attributeWooLinkedViewRepository.CurrWooLinkedGridSettings.DeleteConfirmationWithOption.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.AttributeName}?", selectedItemAttributeLookup.HasECommerceAttributeMap);  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmAddWooItem_ClickAsync(bool confirmClicked)
        {
            if (confirmClicked)
            {
                // they want to add the item to Woo 
                await attributeWooLinkedViewRepository.AddWooItemAndMapAsync(selectedItemAttributeLookup);
                await dataGrid.Reload();
            }
        }
        async Task ConfirmDeleteWooItem_ClickAsync(bool confirmClicked)
        {
            isLoading = true;
            // they want to delete the item to Woo 
            await attributeWooLinkedViewRepository.DeleteWooItemAsync(selectedItemAttributeLookup.ItemAttributeLookupId, confirmClicked);
            //  regardless of how we got here they wanted to delete the original Attribute so delete it now, but only after Woo delete if they wanted it deleted.
            await attributeWooLinkedViewRepository.DeleteRowAsync(selectedItemAttributeLookup);
            isLoading = false;
            await dataGrid.Reload();
        }
        //protected async Task
        async Task ConfirmDelete_ClickAsync(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            isLoading = true;
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooAttribute and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    await attributeWooLinkedViewRepository.CurrWooLinkedGridSettings.DeleteWooItemConfirmation.ShowModalAsync("Are you sure?", $"Delete {selectedItemAttributeLookup.AttributeName} from Woo too?", "Delete", "Cancel");
                else
                    await attributeWooLinkedViewRepository.DeleteRowAsync(selectedItemAttributeLookup);

            }
            isLoading = false;
            await dataGrid.Reload();
        }
        public async Task OnRowRemovedAsync(ItemAttributeLookupView modelItem)
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
        async Task DoGroupActionAsync()
        {
            //if (SelectedBulkAction == BulkAction.none)
            //    return;   ----> button should be disabled 
            await attributeWooLinkedViewRepository.CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");
            int done = 0;
            int failed = 0;
            foreach (var item in selectedItemAttributeLookups)
            {
                if (await attributeWooLinkedViewRepository.DoGroupActionAsync(item, selectedBulkAction) > 0)
                    done++;
                else
                    failed++;
            }
            await attributeWooLinkedViewRepository.CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {done} items and not applied to {failed} items.");
            await dataGrid.Reload();
        }
        async Task ReloadAsync()
        {
            attributeWooLinkedViewRepository.CurrWooLinkedGridSettings.CurrentPage = 1;
            await dataGrid.Reload();
        }
        #endregion
        //        #region VarietyGridStuff
        // ------------------------------------
        // All the Attribute Variety stuff
        // ------------------------------------

        //            Modal NewAttributeVarietyModalRef;
        //        Deprecated
        // ------------------------------------
        //bool OnCustomFilter(ItemAttributeLookupView model)
        //{
        //    if (string.IsNullOrEmpty( _AttributeWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue))
        //        return true;
        //    return
        //        model.AttributeName?.Contains( _AttributeWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue, StringComparison.OrdinalIgnoreCase) == true
        //        || model.ParentAttribute?.AttributeName.Contains( _AttributeWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue, StringComparison.OrdinalIgnoreCase) == true;
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

