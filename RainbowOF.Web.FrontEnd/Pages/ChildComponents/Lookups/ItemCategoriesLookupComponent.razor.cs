using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Lookups
{
    public partial class ItemCategoriesLookupComponent : ComponentBase
    {
        private bool isBusy { get; set; } = false;
        private List<ItemCategoryLookup> modelItemCategoryLookups { get; set; }
        private ItemCategoryLookup seletectedItemCategoryLookup { get; set; }

        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        [Parameter]
        public Guid? SourceParentCategoryId { get; set; }
        [Parameter]
        public bool CanUseAsync { get; set; } = true;  // issues with detail mean that if this is a detail grid we disable Async

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            modelItemCategoryLookups = await GetItemCategoryLookupsAsync(SourceParentCategoryId, CanUseAsync);
            //            await InvokeAsync(StateHasChanged);
        }

        async Task<List<ItemCategoryLookup>> GetItemCategoryLookupsAsync(Guid? sourceParentId, bool IsAsyncCall = true)
        {
            if (isBusy) return null;
            isBusy = true;
            List<ItemCategoryLookup> _gotItemCategoryLookups = null;

            if (IsAsyncCall)
                _gotItemCategoryLookups = (await AppUnitOfWork.ItemCategoryLookupRepository.GetByAsync(icl => icl.ParentCategoryId == sourceParentId)).ToList();
            else
                _gotItemCategoryLookups = AppUnitOfWork.ItemCategoryLookupRepository.GetBy(icl => icl.ParentCategoryId == sourceParentId).ToList();
            isBusy = false;
            return _gotItemCategoryLookups;
        }
    }
}
