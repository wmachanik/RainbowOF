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
        //public GridSettings _WooLinkedGridSettings = new();
        //public int PageSize = 15;
        //public string customFilterValue;
        //public bool IsNarrow = true;
        //public bool IsBordered = true;
        //public bool IsFilterable = false;
        //protected ConfirmModal DeleteConfirmation { get; set; }
        //public RainbowOF.Components.Modals.PopUpAndLogNotification PopUpRef { get; set; }
        public ItemCategoryLookupView SelectedItemCategoryLookup { get; set; } = null;
        public BulkAction SelectedBulkAction { get; set; } = BulkAction.none;
        // variables / Models
        private List<ItemCategoryLookupView> dataModels { get; set; }
        public const string CONST_DISABLEDSTR = "N";
        public const string CONST_ENABLEDSTR = "Y";
        //public bool AreAllChecked = false;
        //public Blazorise.IconName CheckIcon = Blazorise.IconName.CheckSquare;
        //private bool groupButtonEnabled = true;
        private bool isLoading { get; set; } = false;
        private string currStatus { get; set; } = "";
        private DataGrid<ItemCategoryLookupView> dataGrid { get; set; }
        // All there workings are here
        private ICategoryWooLinkedView categoryWooLinkedViewRepository { get; set; }
        private List<ItemCategoryLookupView> selectedItemCategoryLookups { get; set; }
        #endregion
        #region Initialisation
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            categoryWooLinkedViewRepository = new CategoryWooLinkedViewRepository(AppLoggerManager, AppUnitOfWork, /*_WooLinkedGridSettings,*/ AppMapper);
            //await LoadData();
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Back end code
        private async Task SetLoadStatusAsync(string statusString)
        {
            currStatus = statusString;
            await InvokeAsync(StateHasChanged);
        }
        //public async Task LoadDataAsync()
        //{
        //    await HandleReadDataAsync(new DataGridReadDataEventArgs<ItemCategoryLookupView>(_CategoryWooLinkedViewRepository._WooLinkedGridSettings.CurrentPage, _CategoryWooLinkedViewRepository._WooLinkedGridSettings.PageSize, null, System.Threading.CancellationToken.None));
        //}
        public async Task HandleReadDataAsync(DataGridReadDataEventArgs<ItemCategoryLookupView> inputDataGridReadData)
        {
            if (isLoading)
                return;

            isLoading = true;
            // 

            if (!inputDataGridReadData.CancellationToken.IsCancellationRequested)
            {
                DataGridParameters _dataGridParameters = categoryWooLinkedViewRepository.GetDataGridCurrent(inputDataGridReadData, categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.CustomFilterValue);
                if (categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.PageSize != inputDataGridReadData.PageSize)
                { /// page sized changed so jump back to original page
                    categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.CurrentPage = _dataGridParameters.CurrentPage = 1;  // force this
                    categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.PageSize = inputDataGridReadData.PageSize;
                    //                  await Reload();
                }
                await SetLoadStatusAsync("Checking Woo status & loading categories");
                try
                {
                    await SetLoadStatusAsync("Checking Woo status");
                    categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.WooIsActive = await categoryWooLinkedViewRepository.WooIsActiveAsync(AppState);
                    await SetLoadStatusAsync("Loading categories");
                    await LoadItemCategoryLookupListAsync(_dataGridParameters); // inputDataGridReadData.Page - 1, inputDataGridReadData.PageSize, inputDataGridReadData.Columns);
                }
                catch (Exception ex)
                {
                    AppLoggerManager.LogError($"Error running async tasks: {ex.Message}");
                    throw;
                }

                #region AttemptAtMultTaskLoading
                //var localWooIsActiveTask = _categoryWooLinkedViewRepository.WooIsActive(_AppState);
                //var _LoadGridDataTask = LoadItemCategoryLookupList(_dataGridParameters);
                //await Task.WhenAll(localWooIsActiveTask, _LoadGridDataTask);

                //var _loadTasks = new List<Task> { localWooIsActiveTask, _LoadGridDataTask };
                //while (_loadTasks.Count> 0)
                //{
                //    Task finishedTask = await Task.WhenAny(_loadTasks);
                //    if (finishedTask == localWooIsActiveTask)
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
                //_CategoryWooLinkedViewRepository._WooLinkedGridSettings.WooIsActive = localWooIsActiveTask.Result;
                //await SetLoadStatus("Checking Woo status");
                //_CategoryWooLinkedViewRepository._WooLinkedGridSettings.WooIsActive = await _categoryWooLinkedViewRepository.WooIsActive(_AppState);
                //await SetLoadStatus("Loading categories");
                //await LoadItemCategoryLookupList(_dataGridParameters); // inputDataGridReadData.Page - 1, inputDataGridReadData.PageSize, inputDataGridReadData.Columns);
                #endregion 
                currStatus = string.Empty;
            }
            isLoading = false;
        }

        private async Task LoadItemCategoryLookupListAsync(DataGridParameters currentDataGridParameters) //int startPage, int currentPageSize, IEnumerable<DataGridColumnInfo> currentDataGridColumnInfos)
        {
            // store old select items list
            try
            {
                var _response = await categoryWooLinkedViewRepository.LoadViewItemsPaginatedAsync(currentDataGridParameters);  // LoadAllViewItemsAsync(); // (startPage, currentPageSize);
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
                AppLoggerManager.LogError($"Error running async tasks: {ex.Message}");
                throw;
            }
            //restore the old items that were selected.
            selectedItemCategoryLookups = categoryWooLinkedViewRepository.PopSelectedItems(dataModels);
            StateHasChanged();
        }

        //private async Task<List<ItemCategoryLookup>> GetAllItemCategoryLookups()
        //{
        //    IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = AppUnitOfWork.Repository<ItemCategoryLookup>();
        //    List<ItemCategoryLookup> _ItemCategoryLookups = (await _ItemCategoryLookupRepository.GetAllEagerAsync(icl => icl.ParentCategory))
        //        .OrderBy(icl => icl.ParentCategoryId)
        //        .ThenBy(icl => icl.CategoryName).ToList();

        //    return _ItemCategoryLookups;
        //}

        async Task HandleCustomerSearchOnKeyUp(KeyboardEventArgs kbEventHandler)
        {
            _ = (string)kbEventHandler.Key;   // not using this but just in case
                                              //if (_CategoryWooLinkedViewRepository._WooLinkedGridSettings.CustomFilterValue.Length> 2)
                                              //{
            await dataGrid.Reload();
            //}
        }
        bool OnCustomFilter(ItemCategoryLookupView model)
        {
            if (string.IsNullOrEmpty(categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.CustomFilterValue))
                return true;
            return
                model.CategoryName?.Contains(categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.CustomFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.ParentCategory?.CategoryName.Contains(categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.CustomFilterValue, StringComparison.OrdinalIgnoreCase) == true;
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

            await categoryWooLinkedViewRepository.InsertRowAsync(newItem);
            await dataGrid.Reload();
        }
        void OnItemCategoryLookupNewItemDefaultSetter(ItemCategoryLookupView newItem) //ItemCategoryLookup pNewCatItem)
        {
            //newItem =
            categoryWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        //async Task<int> UpdateItemCategoryLookupAsync(ItemCategoryLookupView updatedCatItemView)
        //{
        //    int _result = await _CategoryWooLinkedViewRepository.UpdateItemAsync(updatedCatItemView);
        //    await _DataGrid.Reload();
        //    return _result;
        //}
        async Task OnRowUpdatedAsync(SavedRowItem<ItemCategoryLookupView, Dictionary<string, object>> updatedCatViewItem)
        {
            await categoryWooLinkedViewRepository.UpdateRowAsync(updatedCatViewItem.Item);
            await dataGrid.Reload();

        }
        async Task OnRowRemovingAsync(CancellableRowChange<ItemCategoryLookupView> modelItem)
        {
            // set the Selected Item Category for use later
            SelectedItemCategoryLookup = modelItem.Item;
            var deleteItem = modelItem;
            await categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.DeleteConfirmationWithOption.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.CategoryName}?", SelectedItemCategoryLookup.HasECommerceCategoryMap);  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmAddWooItem_ClickAsync(bool confirm)
        {
            if (confirm)
            {
                // they want to add the item to Woo 
                await categoryWooLinkedViewRepository.AddWooItemAndMapAsync(SelectedItemCategoryLookup);
                await dataGrid.Reload();
            }
        }
        async Task ConfirmDeleteWooItem_ClickAsync(bool confirm)
        {
            // they want to delete the item to Woo 
            await categoryWooLinkedViewRepository.DeleteWooItemAsync(SelectedItemCategoryLookup.ItemCategoryLookupId, confirm);
            //  regardless of how we got here they wanted to delete the original category so delete it now, but only after Woo delete if they wanted it deleted.
            await categoryWooLinkedViewRepository.DeleteRowAsync(SelectedItemCategoryLookup);
            await dataGrid.Reload();
        }
        //protected async Task
        async Task ConfirmDelete_ClickAsync(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                // if there is a WooCategory and we have to delete it, then delete that first.
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    await categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.DeleteWooItemConfirmation.ShowModalAsync("Are you sure?", $"Delete {SelectedItemCategoryLookup.CategoryName} from Woo too?", "Delete", "Cancel");
                else
                    await categoryWooLinkedViewRepository.DeleteRowAsync(SelectedItemCategoryLookup);
            }
            await dataGrid.Reload();
        }
        public async Task OnRowRemovedAsync(ItemCategoryLookupView modelItem)
        {
            await InvokeAsync(StateHasChanged);
            await dataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
            //            var deleteItem = modelItem;
            //if ( dataModels.Contains( model ) )
            //{
            //    dataModels.Remove( model );
            //}
        }
        private Dictionary<Guid, string> _listOfParents = null;
        public Dictionary<Guid, string> GetListOfParentCategories()
        {
            if (_listOfParents == null)
            {
                _listOfParents = new Dictionary<Guid, string>();
                List<ItemCategoryLookup> _itemCategoryLookups = AppUnitOfWork.ItemCategoryLookupRepository.GetAll()
                    .OrderBy(icl => icl.FullCategoryName)
                    .ToList();
                foreach (var model in _itemCategoryLookups)
                {
                    //if (model.ParentCategoryId == null)
                    //{
                    _listOfParents.Add(model.ItemCategoryLookupId, model.FullCategoryName);
                    //}
                }
            }
            return _listOfParents;
        }
        async Task DoGroupActionAsync()
        {
            //if (SelectedBulkAction == BulkAction.none)
            //    return;   ----> button should be disabled 
            await categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");

            int _done = 0;
            int _failed = 0;
            foreach (var item in selectedItemCategoryLookups)
            {
                if (await categoryWooLinkedViewRepository.DoGroupActionAsync(item, SelectedBulkAction) > 0)
                    _done++;
                else
                    _failed++;
            }
            await categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {_done} items and not applied to {_failed} items.");
            ///SelectedItemCategoryLookups.Clear();  // need to do this since we are reloading
            await dataGrid.Reload();
            // await LoadItemCategoryLookupList();   // reload the list so the latest item is displayed
        }
        async Task ReloadAsync()
        {
            categoryWooLinkedViewRepository.CurrWooLinkedGridSettings.CurrentPage = 1;
            //return 
            //await LoadItemCategoryLookupList(); // ((e.Page - 1), e.PageSize);
            await dataGrid.Reload();
        }
        #endregion
    }
}

