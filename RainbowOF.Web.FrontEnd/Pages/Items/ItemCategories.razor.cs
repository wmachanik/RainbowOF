﻿using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Lookups;
using RainbowOF.Tools;
using RainbowOF.Tools.Services;
using RainbowOF.ViewModels.Common;
using RainbowOF.ViewModels.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemCategories : ComponentBase
    {
        // Interface Stuff
        public GridSettings _gridSettings = new GridSettings();

        //public int PageSize = 15;
        //public string customFilterValue;
        //public bool IsNarrow = true;
        //public bool IsBordered = true;
        //public bool IsFilterable = false;
        //protected ConfirmModal DeleteConfirmation { get; set; }
        //public RainbowOF.Components.Modals.PopUpAndLogNotification PopUpRef { get; set; }
        public ItemCategoryLookupView SelectedItemCategoryLookup = null;
        public BulkAction SelectedBulkAction = BulkAction.none;
        // variables / Models
        public List<ItemCategoryLookupView> dataModels;
        public const string disabledStr = "N";
        public const string enabledStr = "Y";

        //public bool AreAllChecked = false;
        //public Blazorise.IconName CheckIcon = Blazorise.IconName.CheckSquare;
        public bool GroupButtonEnabled = true;
        private bool IsLoading = false;
        private string _Status = "";
        DataGrid<ItemCategoryLookupView> _DataGrid;

        // All there workings are here
        ICategoryWooLinkedView _categoryWooLinkedViewRepository;

        public List<ItemCategoryLookupView> SelectedItemCategoryLookups;
        [Inject]
        IAppUnitOfWork _appUnitOfWork { get; set; }
        [Inject]
        ApplicationState _appState { get; set; }
        [Inject]
        public ILoggerManager _logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _categoryWooLinkedViewRepository = new CategoryWooLinkedViewRepository(_logger, _appUnitOfWork, _gridSettings);
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
            await HandleReadDataAsync(new DataGridReadDataEventArgs<ItemCategoryLookupView>(_gridSettings.CurrentPage, _gridSettings.PageSize, null, System.Threading.CancellationToken.None));
        }

        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemCategoryLookupView> inputDataGridReadData)
        {
            if (IsLoading)
                return;

            IsLoading = true;
            // 

            if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
            {
                DataGridParameters _dataGridParameters = _categoryWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, _gridSettings.CustomFilterValue);
                if (_gridSettings.PageSize != inputDataGridReadData.PageSize)
                { /// page sized changed so jump back to original page
                   _gridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                    _gridSettings.PageSize = inputDataGridReadData.PageSize;
 //                  await Reload();
                }
                await SetLoadStatus("Checking Woo status & loading categories");
                try
                {
                    await SetLoadStatus("Checking Woo status");
                    _gridSettings.WooIsActive = await _categoryWooLinkedViewRepository.WooIsActive(_appState);
                    await SetLoadStatus("Loading categories");
                    await LoadItemCategoryLookupList(_dataGridParameters); // inputDataGridReadData.Page - 1, inputDataGridReadData.PageSize, inputDataGridReadData.Columns);


                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error running async tasks: {ex.Message}");
                    throw;
                }

                #region AttemptAtMultTaskLoading
                //var _WooIsActiveTask = _categoryWooLinkedViewRepository.WooIsActive(_appState);
                //var _LoadGridDataTask = LoadItemCategoryLookupList(_dataGridParameters);
                //await Task.WhenAll(_WooIsActiveTask, _LoadGridDataTask);

                //var _loadTasks = new List<Task> { _WooIsActiveTask, _LoadGridDataTask };
                //while (_loadTasks.Count > 0)
                //{
                //    Task finishedTask = await Task.WhenAny(_loadTasks);
                //    if (finishedTask == _WooIsActiveTask)
                //    {
                //        await SetLoadStatus("Woo status checked loading categories");
                //    }
                //    else if (finishedTask == _LoadGridDataTask)
                //    {
                //        await SetLoadStatus("Categories loaded checkingWoo status");
                //    }
                //    _loadTasks.Remove(finishedTask);
                //}
                //await SetLoadStatus("Woo status set and categories loaded");
                //_gridSettings.WooIsActive = _WooIsActiveTask.Result;
                //await SetLoadStatus("Checking Woo status");
                //_gridSettings.WooIsActive = await _categoryWooLinkedViewRepository.WooIsActive(_appState);
                //await SetLoadStatus("Loading categories");
                //await LoadItemCategoryLookupList(_dataGridParameters); // inputDataGridReadData.Page - 1, inputDataGridReadData.PageSize, inputDataGridReadData.Columns);
                #endregion 
                _Status = string.Empty;
            }
            IsLoading = false;
        }

        private async Task LoadItemCategoryLookupList(DataGridParameters currentDataGridParameters) //int startPage, int currentPageSize, IEnumerable<DataGridColumnInfo> currentDataGridColumnInfos)
        {
            // store old select items list
            try
            {
                var response = await _categoryWooLinkedViewRepository.LoadViewItemsPaginatedAsync(currentDataGridParameters);  // LoadAllViewItemsAsync(); // (startPage, currentPageSize);
                if (dataModels == null)
                    dataModels = new List<ItemCategoryLookupView>(response);  // not sure why we have to use a holding variable
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
            SelectedItemCategoryLookups = _categoryWooLinkedViewRepository.PopSelectedItems(dataModels);
            StateHasChanged();
        }

        private async Task<List<ItemCategoryLookup>> GetAllItemCategoryLookups()
        {
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _appUnitOfWork.Repository<ItemCategoryLookup>();
            List<ItemCategoryLookup> _ItemCategoryLookups = (await _ItemCategoryLookupRepository.GetAllEagerAsync(icl => icl.ParentCategory))
                .OrderBy(icl => icl.ParentCategoryId)
                .ThenBy(icl => icl.CategoryName).ToList();

            return _ItemCategoryLookups;
        }

        async Task HandleCustomerSearchOnKeyUp(KeyboardEventArgs kbEventHandler)
        {
            var key = (string)kbEventHandler.Key;   // not using this but just in case
                                                    //if (_gridSettings.CustomFilterValue.Length > 2)
                                                    //{
            await _DataGrid.Reload();
            //}
        }
        bool OnCustomFilter(ItemCategoryLookupView model)
        {
            if (string.IsNullOrEmpty(_gridSettings.CustomFilterValue))
                return true;
            return
                model.CategoryName?.Contains(_gridSettings.CustomFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.ParentCategory?.CategoryName.Contains(_gridSettings.CustomFilterValue, StringComparison.OrdinalIgnoreCase) == true;
        }
        //ItemCategoryLookup GetItemCategoryLookupItemFromView(ItemCategoryLookupView pItem)
        //{
        //    ItemCategoryLookup newItemCategoryLookup = new ItemCategoryLookup
        //    {
        //        ItemCategoryLookupId = pItem.ItemCategoryLookupId,
        //        CategoryName = pItem.CategoryName,
        //        UsedForPrediction = pItem.UsedForPrediction,
        //        ParentCategoryId = (pItem.ParentCategoryId == Guid.Empty) ? null : pItem.ParentCategoryId,
        //        Notes = pItem.Notes,
        //    };

        //    return newItemCategoryLookup;
        //}
        async Task OnRowInserting(SavedRowItem<ItemCategoryLookupView, Dictionary<string, object>> pInsertedItem)
        {
            var newItem = pInsertedItem.Item;

            await _categoryWooLinkedViewRepository.InsertRowAsync(newItem);
            await _DataGrid.Reload();
        }
        void OnItemCategoryLookupNewItemDefaultSetter(ItemCategoryLookupView newItem) //ItemCategoryLookup pNewCatItem)
        {
            newItem = _categoryWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        async Task<int> UpdateItemCategoryLookup(ItemCategoryLookupView UpdatedCatItemView)
        {
            int _result = await _categoryWooLinkedViewRepository.UpdateItemAsync(UpdatedCatItemView);
            await _DataGrid.Reload();
            return _result;
        }
        async Task OnRowUpdating(SavedRowItem<ItemCategoryLookupView, Dictionary<string, object>> pUpdatedItem)
        {
            await _categoryWooLinkedViewRepository.UpdateRowAsync(pUpdatedItem.Item);
            await _DataGrid.Reload();

        }
        void OnRowRemoving(CancellableRowChange<ItemCategoryLookupView> modelItem)
        {
            // set the Selected Item Category for use later
            SelectedItemCategoryLookup = modelItem.Item;
            var deleteItem = modelItem;
            _gridSettings.DeleteConfirmation.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.CategoryName}?", SelectedItemCategoryLookup.HasWooCategoryMap);  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmAddWooItem_Click(bool confirm)
        {
            if (confirm)
            {
                // they want to add the item to Woo 
                await _categoryWooLinkedViewRepository.AddWooItemAndMapAsync(SelectedItemCategoryLookup);
                await _DataGrid.Reload();
            }
        }
        async Task ConfirmDeleteWooItem_Click(bool confirm)
        {
            // they want to delete the item to Woo 
            await _categoryWooLinkedViewRepository.DeleteWooItemAsync(SelectedItemCategoryLookup.ItemCategoryLookupId, confirm);
            //  regardless of how we got here they wanted to delete the original category so delete it now, but only after Woo delete if they wanted it deleted.
            await _categoryWooLinkedViewRepository.DeleteRowAsync(SelectedItemCategoryLookup);
            await _DataGrid.Reload();
        }
        //protected async Task
        async Task ConfirmDelete_Click(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooCategory and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    _gridSettings.DeleteWooItemConfirmation.ShowModal("Are you sure?", $"Delete {SelectedItemCategoryLookup.CategoryName} from Woo too?", "Delete", "Cancel");
                else
                    await _categoryWooLinkedViewRepository.DeleteRowAsync(SelectedItemCategoryLookup);

            }
            await _DataGrid.Reload();
        }
        public async Task OnRowRemoved(ItemCategoryLookupView modelItem)
        {
            await InvokeAsync(StateHasChanged);
            await _DataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
            //            var deleteItem = modelItem;
            //if ( dataModels.Contains( model ) )
            //{
            //    dataModels.Remove( model );
            //}
        }

        public Dictionary<Guid, string> GetListOfParentCategories()
        {
            Dictionary<Guid, string> _ListOfParents = new Dictionary<Guid, string>();
            foreach (var model in dataModels)
            {
                if (model.ParentCategoryId == null)
                {
                    _ListOfParents.Add(model.ItemCategoryLookupId, model.CategoryName);
                }
            }
            return _ListOfParents;
        }

        //void SelectAllRows()
        //{
        //    if (AreAllChecked)
        //        SelectedItemCategoryLookups.Clear();
        //    else
        //    {
        //        foreach (var item in modelItemCategoryLookupViews)
        //        {
        //            if (!SelectedItemCategoryLookups.Exists(ic => ic.ItemCategoryLookupId == item.ItemCategoryLookupId))
        //                SelectedItemCategoryLookups.Add(item);
        //        }
        //    }
        //    CheckIcon = AreAllChecked ? Blazorise.IconName.CheckSquare : Blazorise.IconName.Square;
        //    AreAllChecked = !AreAllChecked;
        //    StateHasChanged();
        //}
        //async Task<int> DoActionOnItem(ItemCategoryLookupView pItem)
        //{
        //    if (SelectedBulkAction == BulkAction.AllowWooSync)
        //        pItem.CanUpdateWooMap = true;
        //    else if (SelectedBulkAction == BulkAction.DisallowWooSync)
        //        pItem.CanUpdateWooMap = false;
        //    return await UpdateWooCategoryMap(pItem);
        //}
        async Task DoGroupAction()
        {
            //if (SelectedBulkAction == BulkAction.none)
            //    return;   ----> button should be disabled 

            _gridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");

            int done = 0;
            int failed = 0;
            foreach (var item in SelectedItemCategoryLookups)
            {
                if (await _categoryWooLinkedViewRepository.DoGroupActionAsync(item, SelectedBulkAction) > 0)
                    done++;
                else
                    failed++;
            }
            _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {done} items and not applied to {failed} items.");
            ///SelectedItemCategoryLookups.Clear();  // need to do this since we are reloading
            await _DataGrid.Reload();
            // await LoadItemCategoryLookupList();   // reload the list so the latest item is displayed
        }

        async Task Reload()
        {
            _gridSettings.CurrentPage = 1;
            //return 
            //await LoadItemCategoryLookupList(); // ((e.Page - 1), e.PageSize);
            await _DataGrid.Reload();
        }
    }
}
