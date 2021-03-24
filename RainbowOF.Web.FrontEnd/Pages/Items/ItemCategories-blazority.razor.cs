using Blazority;
using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Lookups;
using RainbowOF.Web.FrontEnd.Pages.ChildComponents.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemCategories : ComponentBase
    {
        // Interface Stuff
        public int PageSize = 20;
        public string customFilterValue;
        public bool ShowPager = true;
        //public string SelectedItemTabName = "categories";

        public ItemCategoryLookup SelectedItemCategoryLookup = null;
        protected ConfirmModal DeleteConfirmation { get; set; }
        public ShowModalMessage ShowModalStatus { get; set; }
        // variables / Models
        public List<ItemCategoryLookupView> modelItemCategoryLookupViews;
        public bool AreAllChecked = false;
        public Blazorise.IconName CheckIcon = Blazorise.IconName.CheckSquare;
        public bool GroupButtonEnabled = true;
        //public List<SelectListValue> ListOfParents = new List<SelectListValue>();

        //private List<ItemCategoryLookup> ItemCategoryLookups;
        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Parameter]
        public ILoggerManager Logger { get; set; }

        public HashSet<ItemCategoryLookupView> SelectedItemCatagories { get; set; } = new HashSet<ItemCategoryLookupView>();
        /*
         class SelectListValue
                {
                    private Guid? _id { get; set; }
                    public string value { get; set; }
                    public Guid Id { get { return (_id == null) ? Guid.Empty : (Guid)_id; } set { _id = value; } }
                    public string Option { get; set; }
                }
          */

        protected override async Task OnInitializedAsync()
        {
            await LoadItemCategoryLookupList();
        }

        private async Task LoadItemCategoryLookupList()
        {
            StateHasChanged();
            /// get all the items and using those get all the catagories in of items
            List<ItemCategoryLookup> itemCategoryLookups = await GetAllItemCategoryLookups();

            modelItemCategoryLookupViews = new List<ItemCategoryLookupView>();
            WooCategoryMap wooCategoryMap;
            foreach (var itemCat in itemCategoryLookups)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                wooCategoryMap = await GetWooCategoryMappedAsync(itemCat.ItemCategoryLookupId);

                modelItemCategoryLookupViews.Add(new ItemCategoryLookupView
                {
                    ItemCategoryLookupId = itemCat.ItemCategoryLookupId,
                    CategoryName = itemCat.CategoryName,
                    ParentCategoryId = itemCat.ParentCategoryId,
                    ParentCategory = itemCat.ParentCategory,
                    Notes = itemCat.Notes,
                    RowVersion = itemCat.RowVersion,
                    IsChecked = false,

                    CanUpdateWooMap = (wooCategoryMap == null) ? null : wooCategoryMap.CanUpdate
                }); 
            }
            ShowPager = (modelItemCategoryLookupViews.Count > PageSize);
            StateHasChanged();
        }

        public void OnSelectePageChanged(ChangeEventArgs e)
        {
            //(e.Value shoe be string)
            PageSize = Convert.ToInt32(e.Value);
            ShowPager = (modelItemCategoryLookupViews != null) && (modelItemCategoryLookupViews.Count > PageSize);

        }
        private async Task<WooCategoryMap> GetWooCategoryMappedAsync(Guid WooCategoryMap)
        {
            IAppRepository<WooCategoryMap> wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            return await wooCategoryMapRepository.FindFirstAsync(wcm => wcm.ItemCategoryLookupId == WooCategoryMap);
        }

        private async Task<List<ItemCategoryLookup>> GetAllItemCategoryLookups()
        {
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            List<ItemCategoryLookup> _ItemCategoryLookups = (await _ItemCategoryLookupRepository.GetAllEagerAsync(icl => icl.ParentCategory))
                .OrderBy(icl => icl.ParentCategoryId)
                .ThenBy(icl => icl.CategoryName).ToList();

            return _ItemCategoryLookups;
        }

        bool OnCustomFilter(ItemCategoryLookupView model)
        {
            if (string.IsNullOrEmpty(customFilterValue))
                return true;

            return
                model.CategoryName?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.ParentCategory?.CategoryName.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true;
        }
        async Task OnRowInserted(SavedRowItem<ItemCategoryLookupView, Dictionary<string, object>> pInsertedItem)
        {
            var newItem = pInsertedItem.Item;
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

            int _recsAdded = await _ItemCategoryLookupRepository.AddAsync(new ItemCategoryLookup
            {
                CategoryName = newItem.CategoryName,
                ParentCategoryId = (newItem.ParentCategoryId == Guid.Empty) ? null : newItem.ParentCategoryId,
                Notes = newItem.Notes
            });
            if (_recsAdded == AppUnitOfWork.CONST_WASERROR)
                ShowModalStatus.UpdateModalMessage($"Error adding Category: {newItem.CategoryName} - {_AppUnitOfWork.GetErrorMessage()}");
        }
        void OnItemCategoryLookupNewItemDefaultSetter(ItemCategoryLookup pNewCatItem)
        {
            if (pNewCatItem == null)
                pNewCatItem = new ItemCategoryLookup();

            pNewCatItem.CategoryName = "UniqueCatName";
            pNewCatItem.Notes = $"Added {DateTime.Now.Date}";
            pNewCatItem.ParentCategoryId = Guid.Empty;
        }
        async Task<int> UpdateItemCategoryLookup(ItemCategoryLookup pUpdatedCatItem)
        {
            int _recsUpdted = 0;
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            // first check it exists - it coudl have been dleted 
            ItemCategoryLookup pUpdatedLookup = await _ItemCategoryLookupRepository.GetByIdAsync(pUpdatedCatItem.ItemCategoryLookupId);

            if (pUpdatedLookup == null)
            {
                ShowModalStatus.UpdateModalMessage($"Category: {pUpdatedCatItem.CategoryName} is no longer found, was it deleted?");
                return AppUnitOfWork.CONST_WASERROR;
            }
            else
            {
                pUpdatedLookup.CategoryName = pUpdatedCatItem.CategoryName;
                pUpdatedLookup.ParentCategoryId = (pUpdatedCatItem.ParentCategoryId == Guid.Empty) ? null : pUpdatedCatItem.ParentCategoryId;
                pUpdatedLookup.Notes = pUpdatedCatItem.Notes;
                _recsUpdted = await _ItemCategoryLookupRepository.UpdateAsync(pUpdatedLookup);
                if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
                    ShowModalStatus.UpdateModalMessage($"Error updating Category: {pUpdatedCatItem.CategoryName} - {_AppUnitOfWork.GetErrorMessage()}");
            }
            return _recsUpdted;
        }

        async Task<int> UpdateWooCategoryMap(ItemCategoryLookupView pUpdatedItem)
        {
            int _recsUpdated = 0;

            WooCategoryMap updateWooCategoryMap = await GetWooCategoryMappedAsync(pUpdatedItem.ItemCategoryLookupId);
            if (updateWooCategoryMap.CanUpdate != pUpdatedItem.CanUpdateWooMap)
            {
                updateWooCategoryMap.CanUpdate = (bool)pUpdatedItem.CanUpdateWooMap;
                IAppRepository<WooCategoryMap> wooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();
                _recsUpdated = await wooCategoryMapRepository.UpdateAsync(updateWooCategoryMap);
            }
            else
                ShowModalStatus.UpdateModalMessage($"Woo Category Map for category: {pUpdatedItem.CategoryName} is no longer found, was it deleted?");

            return _recsUpdated;
        }
        async Task OnRowUpdated(SavedRowItem<ItemCategoryLookupView, Dictionary<string, object>> pUpdatedItem)
        {
            ItemCategoryLookupView updatedItem = pUpdatedItem.Item;

            ItemCategoryLookup updtedItemCategoryLookup = new ItemCategoryLookup
            {
                ItemCategoryLookupId = updatedItem.ItemCategoryLookupId,
                CategoryName = updatedItem.CategoryName,
                ParentCategoryId = updatedItem.ParentCategoryId,
                Notes = updatedItem.Notes,
            };

            if (await UpdateItemCategoryLookup(updtedItemCategoryLookup) != AppUnitOfWork.CONST_WASERROR)
            {
                if (await UpdateWooCategoryMap(updatedItem) == AppUnitOfWork.CONST_WASERROR)
                    ShowModalStatus.UpdateModalMessage($"Error updating WooCategory map for Item: {updatedItem.CategoryName} - {_AppUnitOfWork.GetErrorMessage()}");
                else
                    ShowModalStatus.UpdateModalMessage($"Category: {updatedItem.CategoryName} was updated.");
            }
            ShowModalStatus.ShowModal();
            await LoadItemCategoryLookupList();   // reload the list so the latest item is displayed
        }
        void OnRowRemoving(CancellableRowChange<ItemCategoryLookupView> modelItem)
        {
            // set the Seleceted Item Category for use later
            SelectedItemCategoryLookup = modelItem.Item;
            var deleteItem = modelItem;
            DeleteConfirmation.SetTitleAndMessage("Delete confirmation", $"Are you sure you want to delete {deleteItem.Item.CategoryName}?");
            DeleteConfirmation.ShowModal();
        }

        //protected async Task
        async Task ConfirmDelete_Click(bool deleteConfirmed)
        {
            if (deleteConfirmed)
            {
                IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();

                var _recsDelete = await _ItemCategoryLookupRepository.DeleteByIdAsync(SelectedItemCategoryLookup.ItemCategoryLookupId);     //DeleteByAsync(icl => icl.ItemCategoryLookupId == SelectedItemCategoryLookup.ItemCategoryLookupId);

                if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                    ShowModalStatus.UpdateModalMessage($"Category: {SelectedItemCategoryLookup.CategoryName} is no longer found, was it deleted?");
                else
                    ShowModalStatus.UpdateModalMessage($"Category: {SelectedItemCategoryLookup.CategoryName} was it deleted?");
                ShowModalStatus.ShowModal();
                await LoadItemCategoryLookupList();   // reload the list so the latest item is displayed
            }
        }
        public void OnRowRemoved(ItemCategoryLookupView modelItem)
        {
            var deleteItem = modelItem;
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

        void SelectAllRows()
        {
            foreach (var item in modelItemCategoryLookupViews)
            {
                item.IsChecked = !AreAllChecked;
            }
            CheckIcon = AreAllChecked ? Blazorise.IconName.CheckSquare : Blazorise.IconName.Square;
            AreAllChecked = !AreAllChecked;
            SetGroupButtonStatus(); // null);
            StateHasChanged();
        }

        EventCallback SetGroupButtonStatus() //bool pIsChecked, Guid? pId = null)   //(bool? pIsChecked)
        {
            var selectedItems = modelItemCategoryLookupViews.Where(m => m.IsChecked).ToList();
            GroupButtonEnabled = (selectedItems != null);

            if (GroupButtonEnabled)
            {
                CheckIcon = (modelItemCategoryLookupViews.Count == selectedItems.Count) ? Blazorise.IconName.CheckSquare : Blazorise.IconName.Square;
                GroupButtonEnabled = (selectedItems.Count > 0);
            }

            return EventCallback.Empty;
        }
        void DoActionOnItem(ItemCategoryLookupView pItem)
        {
            pItem.IsChecked = false;  // should rather do the thing
        }
        void DoGroupAction()
        {
            var selectedItems = modelItemCategoryLookupViews.Where(m => m.IsChecked);

            foreach (var item in selectedItems)
            {
                DoActionOnItem(item);
            }
        }

    }
}

