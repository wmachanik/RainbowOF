using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class Items : ComponentBase
    {
        // Interface Stuff
        public bool IsSortable = true;
        public bool IsFilterable = true;
        public bool DoShowPager = true;
        public int PageSize = 20;
        public string customFilterValue;
        public int IndexSelectedCategory;
        //public string SelectedItemTabName = "categories";

        public Item SelectedItem = null;

        // variables / Models
        public List<Item> ItemList;
        public List<ItemCategoryLookup> ListOfItemCategories;

        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadItemList();
        }

        private async Task LoadItemList()
        {
            StateHasChanged();
            /// get all the items and using those get all the categories in of items
            IAppRepository<ItemCategoryLookup> _ItemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            ListOfItemCategories = (await _ItemCategoryLookupRepository.GetAllAsync())
                .OrderBy(icl => icl.ParentCategoryId)
                .ThenBy(icl => icl.CategoryName).ToList();
            if (ListOfItemCategories != null)
            {
                IndexSelectedCategory = 0;
            }
            ItemList = await GetItemsByCategoryIndex(0);
            if (ItemList != null)
                SelectedItem = ItemList[0];
            StateHasChanged();
        }
        bool OnCustomFilter(Item model)
        {
            if (string.IsNullOrEmpty(customFilterValue))
                return true;

            return
                model.ItemName?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.ItemAbbreviatedName?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.SKU?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true;
        }

        private async Task<List<Item>> GetItemsByCategoryIndex(int CategoryIndex)
        {
            IItemRepository _ItemRepository = _AppUnitOfWork.itemRepository();
            if (CategoryIndex == 0)
                return (await _ItemRepository.GetAllAsync()).ToList();
            else
            {
                // return only the children
                var _CatId = ListOfItemCategories[CategoryIndex - 1].ItemCategoryLookupId;

                return (
                    await _ItemRepository.GetByAsync(i => i.ItemCategories
                                                                .All(ic => i.ItemCategories.Select(ic => ic.ItemCategoryLookupId).Contains(_CatId))
                                                     )
                    ).ToList();

                //var _AllItems = await _ItemRepository.GetAllAsync();

                //return _AllItems.Where(i => i.PrimaryItemCategoryLookupId == _ItemCat.ItemCategoryLookupId).ToList();
            }
        }

        public async Task OnCatagoeryChanged(ChangeEventArgs e)
        {
            string _SelectedIndexStr = (string)e.Value;
            int _SelectedIndex = int.Parse(_SelectedIndexStr);   // positiion in the list
            IndexSelectedCategory = _SelectedIndex; //  DatesInLog.FindIndex(0, dt => dt.Date == DatesInLog[_SelectedDate]);
            ItemList = await GetItemsByCategoryIndex(_SelectedIndex);
            StateHasChanged();
        }


        public List<ItemCategory> GetAllItemsCategories(Guid pItemId)
        {
            IAppRepository<ItemCategory> _ItemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();

            return (_ItemCategoryRepository.GetBy(ic => ic.ItemId == pItemId)).ToList();
        }
        public List<ItemAttribute> GetAllItemsAttributes(Guid pItemId)
        {
            IAppRepository<ItemAttribute> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttribute>();

            return (_ItemAttributeRepository.GetBy(ia => ia.ItemId == pItemId)).ToList();
        }
        public List<ItemAttributeVariety> GetAllItemsAttributeVariety(Guid pItemId)
        {
            IAppRepository<ItemAttributeVariety> _ItemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVariety>();

            return (_ItemAttributeVarietyRepository.GetBy(iv => iv.ItemId == pItemId)).ToList();
        }

        //public void OnSelectedItemTabChanged(string tabSeleted)
        //{
        //    SelectedItemTabName = tabSeleted;
        //}

        //public bool HasItemDetail()
        //{
        //    return (SelectedItem.ItemCategories?.Count > 0)
        //        || (SelectedItem.ItemAttributes?.Count > 0)
        //        || (SelectedItem.ItemAttributeVarieties?.Count > 0);
        //}

        public void OnRowInserted(SavedRowItem<Item, Dictionary<string, object>> i)
        {
            var newItem = i.Item;

            //employee.Id = dataModels?.Max( x => x.Id ) + 1 ?? 1;

            //dataModels.Add( employee );
        }

        public void OnRowUpdated(SavedRowItem<Item, Dictionary<string, object>> i)
        {
            var updatedItem = i.Item;

            //employee.FirstName = (string)e.Values["FirstName"];
            //employee.LastName = (string)e.Values["LastName"];
            //employee.EMail = (string)e.Values["EMail"];
            //employee.City = (string)e.Values["City"];
            //employee.Zip = (string)e.Values["Zip"];
            //employee.DateOfBirth = (DateTime?)e.Values["DateOfBirth"];
            //employee.Childrens = (int?)e.Values["Childrens"];
            //employee.Gender = (string)e.Values["Gender"];
            //employee.Salary = (decimal)e.Values["Salary"];
        }

        public void OnRowRemoved(Item modelItem)
        {
            var deleteItem = modelItem;
            //if ( dataModels.Contains( model ) )
            //{
            //    dataModels.Remove( model );
            //}
        }

    }
}
