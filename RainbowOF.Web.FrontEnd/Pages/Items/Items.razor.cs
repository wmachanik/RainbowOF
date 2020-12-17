using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Items;
using RanbowOF.Repositories.Common;
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
        public string customFilterValue;

        // variables / Models
        public List<Item> ItemList;

        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadItemList();
        }

        private async Task LoadItemList()
        {
            IAppRepository<Item> _ItemList = _AppUnitOfWork.Repository<Item>();
            StateHasChanged();
            // need to rather add paging this thing is gonna get arge
            await Task.Run(() => ItemList = _ItemList.GetAll().ToList());
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

    }
}
