using AutoMapper;
using Blazorise.DataGrid;
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
    public partial class ItemCategoriesLookup : ComponentBase
    {
        // Interface Stuff
        //public GridSettings _WooLinkedGridSettings = new();

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
        private ICategoryWooLinkedView _CategoryWooLinkedViewRepository;

        public List<ItemCategoryLookupView> SelectedItemCategoryLookups;
        [Inject]
        private IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        private ApplicationState _AppState { get; set; }
        [Inject]
        public ILoggerManager _Logger { get; set; }
        [Inject]
        public IMapper _Mapper { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _CategoryWooLinkedViewRepository = new CategoryWooLinkedViewRepository(_Logger, _AppUnitOfWork, /*_WooLinkedGridSettings,*/ _Mapper);
            //await LoadData();
            await InvokeAsync(StateHasChanged);
        }
        private async Task SetLoadStatusAsync(string statusString)
        {
            _Status = statusString;
            await InvokeAsync(StateHasChanged);
        }
        //public async Task LoadDataAsync()
        //{
        //    await HandleReadDataAsync(new DataGridReadDataEventArgs<ItemCategoryLookupView>(_CategoryWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage, _CategoryWooLinkedViewRepository._WooLinkedGridSettings.PageSize, null, System.Threading.CancellationToken.None));
        //}
        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemCategoryLookupView> inputDataGridReadData)
        {
            if (IsLoading)
                return;

            IsLoading = true;
            // 

            if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
            {
                DataGridParameters _dataGridParameters = _CategoryWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, _CategoryWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue);
                if (_CategoryWooLinkedViewRepository._WooLinkedGridSettings.PageSize != inputDataGridReadData.PageSize)
                { /// page sized changed so jump back to original page
                    _CategoryWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                    _CategoryWooLinkedViewRepository._WooLinkedGridSettings.PageSize = inputDataGridReadData.PageSize;
                    //                  await Reload();
                }
                await SetLoadStatusAsync("Checking Woo status & loading categories");
                try
                {
                    await SetLoadStatusAsync("Checking Woo status");
                    _CategoryWooLinkedViewRepository._WooLinkedGridSettings.WooIsActive = await _CategoryWooLinkedViewRepository.WooIsActiveAsync(_AppState);
                    await SetLoadStatusAsync("Loading categories");
                    await LoadItemCategoryLookupListAsync(_dataGridParameters); // inputDataGridReadData.Page - 1, inputDataGridReadData.PageSize, inputDataGridReadData.Columns);
                }
                catch (Exception ex)
                {
                    _Logger.LogError($"Error running async tasks: {ex.Message}");
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
                //_CategoryWooLinkedViewRepository._WooLinkedGridSettings.WooIsActive = _WooIsActiveTask.Result;
                //await SetLoadStatus("Checking Woo status");
                //_CategoryWooLinkedViewRepository._WooLinkedGridSettings.WooIsActive = await _categoryWooLinkedViewRepository.WooIsActive(_appState);
                //await SetLoadStatus("Loading categories");
                //await LoadItemCategoryLookupList(_dataGridParameters); // inputDataGridReadData.Page - 1, inputDataGridReadData.PageSize, inputDataGridReadData.Columns);
                #endregion 
                _Status = string.Empty;
            }
            IsLoading = false;
        }

        private async Task LoadItemCategoryLookupListAsync(DataGridParameters currentDataGridParameters) //int startPage, int currentPageSize, IEnumerable<DataGridColumnInfo> currentDataGridColumnInfos)
        {
            // store old select items list
            try
            {
                var _response = await _CategoryWooLinkedViewRepository.LoadViewItemsPaginatedAsync(currentDataGridParameters);  // LoadAllViewItemsAsync(); // (startPage, currentPageSize);
                if (dataModels == null)
                    dataModels = new List<ItemCategoryLookupView>(_response);  // not sure why we have to use a holding variable
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
            SelectedItemCategoryLookups = _CategoryWooLinkedViewRepository.PopSelectedItems(dataModels);
            StateHasChanged();
        }

        //private async Task<List<ItemCategoryLookup>> GetAllItemCategoryLookups()
        //{
        //    IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
        //    List<ItemCategoryLookup> _ItemCategoryLookups = (await _ItemCategoryLookupRepository.GetAllEagerAsync(icl => icl.ParentCategory))
        //        .OrderBy(icl => icl.ParentCategoryId)
        //        .ThenBy(icl => icl.CategoryName).ToList();

        //    return _ItemCategoryLookups;
        //}

        async Task HandleCustomerSearchOnKeyUp(KeyboardEventArgs kbEventHandler)
        {
            var key = (string)kbEventHandler.Key;   // not using this but just in case
                                                    //if (_CategoryWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue.Length > 2)
                                                    //{
            await _DataGrid.Reload();
            //}
        }
        bool OnCustomFilter(ItemCategoryLookupView model)
        {
            if (string.IsNullOrEmpty(_CategoryWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue))
                return true;
            return
                model.CategoryName?.Contains(_CategoryWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.ParentCategory?.CategoryName.Contains(_CategoryWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue, StringComparison.OrdinalIgnoreCase) == true;
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

            await _CategoryWooLinkedViewRepository.InsertRowAsync(newItem);
            await _DataGrid.Reload();
        }
        void OnItemCategoryLookupNewItemDefaultSetter(ItemCategoryLookupView newItem) //ItemCategoryLookup pNewCatItem)
        {
            newItem = _CategoryWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        async Task<int> UpdateItemCategoryLookup(ItemCategoryLookupView updatedCatItemView)
        {
            int _result = await _CategoryWooLinkedViewRepository.UpdateItemAsync(updatedCatItemView);
            await _DataGrid.Reload();
            return _result;
        }
        async Task OnRowUpdating(SavedRowItem<ItemCategoryLookupView, Dictionary<string, object>> updatedCatViewItem)
        {
            await _CategoryWooLinkedViewRepository.UpdateRowAsync(updatedCatViewItem.Item);
            await _DataGrid.Reload();

        }
        void OnRowRemoving(CancellableRowChange<ItemCategoryLookupView> modelItem)
        {
            // set the Selected Item Category for use later
            SelectedItemCategoryLookup = modelItem.Item;
            var deleteItem = modelItem;
            _CategoryWooLinkedViewRepository._WooLinkedGridSettings.DeleteConfirmationWithOption.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.CategoryName}?", SelectedItemCategoryLookup.HasECommerceCategoryMap);  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmAddWooItem_Click(bool confirm)
        {
            if (confirm)
            {
                // they want to add the item to Woo 
                await _CategoryWooLinkedViewRepository.AddWooItemAndMapAsync(SelectedItemCategoryLookup);
                await _DataGrid.Reload();
            }
        }
        async Task ConfirmDeleteWooItem_Click(bool confirm)
        {
            // they want to delete the item to Woo 
            await _CategoryWooLinkedViewRepository.DeleteWooItemAsync(SelectedItemCategoryLookup.ItemCategoryLookupId, confirm);
            //  regardless of how we got here they wanted to delete the original category so delete it now, but only after Woo delete if they wanted it deleted.
            await _CategoryWooLinkedViewRepository.DeleteRowAsync(SelectedItemCategoryLookup);
            await _DataGrid.Reload();
        }
        //protected async Task
        async Task ConfirmDelete_Click(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooCategory and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    _CategoryWooLinkedViewRepository._WooLinkedGridSettings.DeleteWooItemConfirmation.ShowModal("Are you sure?", $"Delete {SelectedItemCategoryLookup.CategoryName} from Woo too?", "Delete", "Cancel");
                else
                    await _CategoryWooLinkedViewRepository.DeleteRowAsync(SelectedItemCategoryLookup);

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
        Dictionary<Guid, string> _ListOfParents = null;
        public Dictionary<Guid, string> GetListOfParentCategories()
        {
            if (_ListOfParents == null)
            {
                _ListOfParents = new Dictionary<Guid, string>();
                IItemCategoryLookupRepository _itemCategoryLookupRepository = _AppUnitOfWork.itemCategoryLookupRepository();
                List<ItemCategoryLookup> _itemCategoryLookups = _itemCategoryLookupRepository.GetAll()
                    .OrderBy(icl => icl.FullCategoryName)
                    .ToList();
                foreach (var model in _itemCategoryLookups)
                {
                    //if (model.ParentCategoryId == null)
                    //{
                    _ListOfParents.Add(model.ItemCategoryLookupId, model.FullCategoryName);
                    //}
                }
            }
            return _ListOfParents;
        }


        async Task DoGroupAction()
        {
            //if (SelectedBulkAction == BulkAction.none)
            //    return;   ----> button should be disabled 

            _CategoryWooLinkedViewRepository._WooLinkedGridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");

            int _done = 0;
            int _failed = 0;
            foreach (var item in SelectedItemCategoryLookups)
            {
                if (await _CategoryWooLinkedViewRepository.DoGroupActionAsync(item, SelectedBulkAction) > 0)
                    _done++;
                else
                    _failed++;
            }
            _CategoryWooLinkedViewRepository._WooLinkedGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {_done} items and not applied to {_failed} items.");
            ///SelectedItemCategoryLookups.Clear();  // need to do this since we are reloading
            await _DataGrid.Reload();
            // await LoadItemCategoryLookupList();   // reload the list so the latest item is displayed
        }

        async Task ReloadAsync()
        {
            _CategoryWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage = 1;
            //return 
            //await LoadItemCategoryLookupList(); // ((e.Page - 1), e.PageSize);
            await _DataGrid.Reload();
        }
    }
}

