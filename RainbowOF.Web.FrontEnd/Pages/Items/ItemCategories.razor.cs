using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Lookups;
using RainbowOF.Components.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RainbowOF.ViewModels.Common;
using RainbowOF.Repositories.Lookups;

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
        public List<ItemCategoryLookupView> modelItemCategoryLookupViews;
        public bool AreAllChecked = false;
        public Blazorise.IconName CheckIcon = Blazorise.IconName.CheckSquare;
        public bool GroupButtonEnabled = true;
        // All there workings are here
        ICategoryWooLinkedView _categoryWooLinkedViewRepository;

        public List<ItemCategoryLookupView> SelectedItemCategoryLookups;
        [Inject]
        IAppUnitOfWork _appUnitOfWork { get; set; }
        [Parameter]
        public ILoggerManager _logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _categoryWooLinkedViewRepository = new CategoryWooLinkedViewRepository(_logger, _appUnitOfWork, _gridSettings);
            await LoadItemCategoryLookupList();
        }

        private async Task LoadItemCategoryLookupList()
        {
            StateHasChanged();

            // store old select items list
            _categoryWooLinkedViewRepository.PushSelectedItems(SelectedItemCategoryLookups);
            // retrieve database items
            modelItemCategoryLookupViews = await _categoryWooLinkedViewRepository.LoadAllViewItems();
            //restore the old items that were selected.
            SelectedItemCategoryLookups = _categoryWooLinkedViewRepository.PopSelectedItems(modelItemCategoryLookupViews);
            StateHasChanged();
        }

            /*
            List<ItemCategoryLookupView> oldSelectedItems = null;
            if (SelectedItemCategoryLookups != null)
            {
                oldSelectedItems = new List<ItemCategoryLookupView>(SelectedItemCategoryLookups);
                SelectedItemCategoryLookups.Clear(); // the refresh does this
            }
                        /// get all the items and using those get all the categories in of items
                        List<ItemCategoryLookup> itemCategoryLookups = await GetAllItemCategoryLookups();

                        modelItemCategoryLookupViews = new List<ItemCategoryLookupView>();
                        WooCategoryMap wooCategoryMap;
                        // Map Items to Woo CategoryMap
                        foreach (var itemCat in itemCategoryLookups)
                        {
                            //  map all the items across to the view then allocate extra woo stuff if exists.
                            wooCategoryMap = await GetWooCategoryMappedAsync(itemCat.ItemCategoryLookupId);

                            modelItemCategoryLookupViews.Add(new ItemCategoryLookupView
                            {
                                ItemCategoryLookupId = itemCat.ItemCategoryLookupId,
                                CategoryName = itemCat.CategoryName,
                                UsedForPrediction = itemCat.UsedForPrediction,
                                ParentCategoryId = itemCat.ParentCategoryId,
                                ParentCategory = itemCat.ParentCategory,
                                Notes = itemCat.Notes,
                                RowVersion = itemCat.RowVersion,

                                CanUpdateWooMap = (wooCategoryMap == null) ? null : wooCategoryMap.CanUpdate
                            });
                        }
                        //ShowPager = (modelItemCategoryLookupViews.Count > PageSize);
            if (oldSelectedItems != null)
                foreach (var item in oldSelectedItems)
                {
                    var _oldSelectdItem = modelItemCategoryLookupViews.Where(icl => icl.ItemCategoryLookupId == item.ItemCategoryLookupId).FirstOrDefault();
                    if (_oldSelectdItem != null)  // if it was deleted this will be the case
                        SelectedItemCategoryLookups.Add(_oldSelectdItem);
                }
            */
        //}

        //public void OnSelectePageChanged(ChangeEventArgs e)
        //{
        //    //(e.Value shoe be string)
        //    PageSize = Convert.ToInt32(e.Value);
        //    ShowPager = (modelItemCategoryLookupViews != null) && (modelItemCategoryLookupViews.Count > PageSize);

        //}
        /*
    private async Task<WooCategoryMap> GetWooCategoryMappedAsync(Guid WooCategoryMapId)
    {
        IAppRepository<WooCategoryMap> wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

        return await wooCategoryMapRepository.FindFirstAsync(wcm => wcm.ItemCategoryLookupId == WooCategoryMapId);
    }
        */

        private async Task<List<ItemCategoryLookup>> GetAllItemCategoryLookups()
        {
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _appUnitOfWork.Repository<ItemCategoryLookup>();
            List<ItemCategoryLookup> _ItemCategoryLookups = (await _ItemCategoryLookupRepository.GetAllEagerAsync(icl => icl.ParentCategory))
                .OrderBy(icl => icl.ParentCategoryId)
                .ThenBy(icl => icl.CategoryName).ToList();

            return _ItemCategoryLookups;
        }

        bool OnCustomFilter(ItemCategoryLookupView model)
        {
            if (string.IsNullOrEmpty(_gridSettings.customFilterValue))
                return true;

            return
                model.CategoryName?.Contains(_gridSettings.customFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.ParentCategory?.CategoryName.Contains(_gridSettings.customFilterValue, StringComparison.OrdinalIgnoreCase) == true;
        }
        ItemCategoryLookup GetItemCategoreLookupItemFromView(ItemCategoryLookupView pItem)
        {
            ItemCategoryLookup newItemCategoryLookup = new ItemCategoryLookup
            {
                ItemCategoryLookupId = pItem.ItemCategoryLookupId,
                CategoryName = pItem.CategoryName,
                UsedForPrediction = pItem.UsedForPrediction,
                ParentCategoryId = (pItem.ParentCategoryId == Guid.Empty) ? null : pItem.ParentCategoryId,
                Notes = pItem.Notes,
            };

            return newItemCategoryLookup;
        }
        async Task OnRowInserting(SavedRowItem<ItemCategoryLookupView, Dictionary<string, object>> pInsertedItem)
        {
            var newItem = pInsertedItem.Item;

            await _categoryWooLinkedViewRepository.InsertRowAsync(newItem);
        }

        /* //  IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
        //// first check we do not already have a category like this.
        //if (await _ItemCategoryLookupRepository.FindFirstAsync(icl => icl.CategoryName == newItem.CategoryName) == null)
        //{
        //    int _recsAdded = await _ItemCategoryLookupRepository.AddAsync(GetItemCategoreLookupItemFromView(newItem));
        //    if (_recsAdded != AppUnitOfWork.CONST_WASERROR)
        //        _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newItem.CategoryName} - {_AppUnitOfWork.GetErrorMessage()}", "Category Added");
        //    else
        //        _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newItem.CategoryName} - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Category");
        //}
        //else
        //    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newItem.CategoryName} already exists, so could not be added.");
        //await LoadItemCategoryLookupList();   // reload the list so the latest item is displayed
        */
        void OnItemCategoryLookupNewItemDefaultSetter(ItemCategoryLookupView newItem) //ItemCategoryLookup pNewCatItem)
        {
            _categoryWooLinkedViewRepository.NewItemDefaultSetter(newItem);
        }
        //if (pNewCatItem == null)
        //    pNewCatItem = new ItemCategoryLookup();

        //pNewCatItem.CategoryName = "Cat name (must be unique)";
        //pNewCatItem.Notes = $"Added {DateTime.Now.Date}";
        //pNewCatItem.ParentCategoryId = Guid.Empty;
        //pNewCatItem.UsedForPrediction = true;
        async Task<int> UpdateItemCategoryLookup(ItemCategoryLookup pUpdatedCatItem)
        {
            return await _categoryWooLinkedViewRepository.UpdateItemAsync(pUpdatedCatItem);
        }
        //int _recsUpdted = 0;

        //IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
        //// first check it exists - it could have been deleted 
        //ItemCategoryLookup pUpdatedLookup = await _ItemCategoryLookupRepository.GetByIdAsync(pUpdatedCatItem.ItemCategoryLookupId);

        //if (pUpdatedLookup == null)
        //{
        //    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Category: {pUpdatedCatItem.CategoryName} is no longer found, was it deleted?");
        //    return AppUnitOfWork.CONST_WASERROR;
        //}
        //else
        //{
        //    pUpdatedLookup.CategoryName = pUpdatedCatItem.CategoryName;
        //    pUpdatedLookup.ParentCategoryId = (pUpdatedCatItem.ParentCategoryId == Guid.Empty) ? null : pUpdatedCatItem.ParentCategoryId;
        //    pUpdatedLookup.UsedForPrediction = pUpdatedCatItem.UsedForPrediction;
        //    pUpdatedLookup.Notes = pUpdatedCatItem.Notes;
        //    _recsUpdted = await _ItemCategoryLookupRepository.UpdateAsync(pUpdatedLookup);
        //    if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
        //        _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{pUpdatedCatItem.CategoryName} - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Category");
        //}
        //return _recsUpdted;
        //}

        //async Task<int> UpdateWooCategoryMap(ItemCategoryLookupView pUpdatedItem)
        //{
        //    return await _categoryWooLinkedView.UpdateWooMappingAsync(pUpdatedItem);

        //    //int _recsUpdated = 0;
        //    //WooCategoryMap updateWooCategoryMap = await GetWooCategoryMappedAsync(pUpdatedItem.ItemCategoryLookupId);
        //    //if (updateWooCategoryMap != null)
        //    //{
        //    //    if (updateWooCategoryMap.CanUpdate == pUpdatedItem.CanUpdateWooMap)
        //    //    {
        //    //        // not necessary to display message.
        //    //        //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo Category Map for category: {pUpdatedItem.CategoryName} has not changed, so was not updated?");
        //    //    }
        //    //    else
        //    //    {
        //    //        updateWooCategoryMap.CanUpdate = (bool)pUpdatedItem.CanUpdateWooMap;
        //    //        IAppRepository<WooCategoryMap> wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();
        //    //        _recsUpdated = await wooCategoryMapRepository.UpdateAsync(updateWooCategoryMap);
        //    //        _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category: {pUpdatedItem.CategoryName} was updated.");
        //    //    }
        //    //}
        //    //else
        //    //    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Category Map for category: {pUpdatedItem.CategoryName} is no longer found, was it deleted?");

        //    //return _recsUpdated;
        //}
        //private bool IsDuplicate(ItemCategoryLookupView pItem)
        //{
        //    //// check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
        //    //var _exists = modelItemCategoryLookupViews.FindAll(ml => ml.CategoryName == pItem.CategoryName);
        //    //return ((_exists != null) && (_exists.Count > 1));
        //}
        //private bool IsValid(ItemCategoryLookupView pItem)
        //{
        //    // check that there is a loop back on PaerentId
        //    return (pItem.ParentCategoryId != pItem.ItemCategoryLookupId);
        //}
        async Task OnRowUpdating(SavedRowItem<ItemCategoryLookupView, Dictionary<string, object>> pUpdatedItem)
        {
            await _categoryWooLinkedViewRepository.UpdateRowAsync(pUpdatedItem.Item);
        }
            //ItemCategoryLookupView updatedItem = pUpdatedItem.Item;
            //if (IsDuplicate(updatedItem))
            //    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Category Name: {updatedItem.CategoryName} - already exists, cannot be updated", "Exists already");
            //else
            //{
            //    if (IsValid(updatedItem))
            //    {
            //        ItemCategoryLookup updatedItemCategoryLookup = GetItemCategoreLookupItemFromView(updatedItem);
            //        // update and check for errors 
            //        if (await UpdateItemCategoryLookup(updatedItemCategoryLookup) != AppUnitOfWork.CONST_WASERROR)
            //        {
            //            if ((updatedItem.HasWooCategoryMap) && (await UpdateWooCategoryMap(updatedItem) == AppUnitOfWork.CONST_WASERROR))
            //                _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"WooCategory map for Item: {updatedItem.CategoryName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating");
            //            //else
            //            //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category: {updatedItem.CategoryName} was updated.");
            //        }
            //    }
            //    else
            //    {
            //        string pMessage = $"Category Item {updatedItem.CategoryName} cannot be parent and child.";
            //        _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, pMessage, "Error updating");
            //    }
            //}
            //await LoadItemCategoryLookupList();   // reload the list so the latest item is displayed
 //       }
        void OnRowRemoving(CancellableRowChange<ItemCategoryLookupView> modelItem)
        {
            // set the Selected Item Category for use later
            SelectedItemCategoryLookup = modelItem.Item;
            var deleteItem = modelItem;
            _gridSettings.DeleteConfirmation.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.CategoryName}?", SelectedItemCategoryLookup.HasWooCategoryMap );  //,"Delete","Cancel"); - passed in on init
        }
        //
        async Task ConfirmAddWooItem_Click(bool confirm)
        {
            if (confirm)
            {
                await _categoryWooLinkedViewRepository.AddWooItemAsync(SelectedItemCategoryLookup);
            }
        }
        async Task ConfirmDeleteWooItem_Click(bool confirm)
        {
            if (confirm)
            {
                await _categoryWooLinkedViewRepository.DeleteWooItemAsync(SelectedItemCategoryLookup.ItemCategoryLookupId);
            }
        }
        //protected async Task
        async Task ConfirmDelete_Click(ConfirmModalWithOption.ConfirmResults confirmationOption)
        {
            if ((confirmationOption == ConfirmModalWithOption.ConfirmResults.confirm) || (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption))
            {
                await _categoryWooLinkedViewRepository.DeleteRowAsync(SelectedItemCategoryLookup);
                if (confirmationOption == ConfirmModalWithOption.ConfirmResults.confirmWithOption)
                    _gridSettings.DeleteWooItemConfirmation.ShowModal("Are you sure?", $"Do you want to delete {SelectedItemCategoryLookup.CategoryName} from Woo too?", "Delete from Woo", "Cancel");

                //IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

                //var _recsDelete = await _ItemCategoryLookupRepository.DeleteByIdAsync(SelectedItemCategoryLookup.ItemCategoryLookupId);     //DeleteByAsync(icl => icl.ItemCategoryLookupId == SelectedItemCategoryLookup.ItemCategoryLookupId);

                //if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                //    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Category: {SelectedItemCategoryLookup.CategoryName} is no longer found, was it deleted?");
                //else
                //    _gridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Category: {SelectedItemCategoryLookup.CategoryName} was it deleted?");
            }
        }
        public async Task OnRowRemoved(ItemCategoryLookupView modelItem)
        {
            await LoadItemCategoryLookupList();   // reload the list so the latest item is displayed
//            var deleteItem = modelItem;
            //if ( dataModels.Contains( model ) )
            //{
            //    dataModels.Remove( model );
            //}
        }

        public Dictionary<Guid, string> GetListOfParentCategories()
        {
            Dictionary<Guid, string> _ListOfParents = new Dictionary<Guid, string>();
            foreach (var model in modelItemCategoryLookupViews)
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
            await LoadItemCategoryLookupList();   // reload the list so the latest item is displayed
        }
    }
}

