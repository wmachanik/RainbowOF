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
        bool IsBusy = false;
        List<ItemCategoryLookup> ModelItemCategoryLookups;

        ItemCategoryLookup SeletectedItemCategoryLookup;

        [Inject]
        IUnitOfWork appUnitOfWork { get; set; }
        [Parameter]
        public Guid? SourceParentCategoryId { get; set; }
        [Parameter]
        public bool CanUseAsync { get; set; } = true;  // issues with detail mean that if this is a detail grid we disable Async

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            ModelItemCategoryLookups = await GetItemCategoryLookupsAsync(SourceParentCategoryId, CanUseAsync);
//            await InvokeAsync(StateHasChanged);
        }

        async Task<List<ItemCategoryLookup>> GetItemCategoryLookupsAsync(Guid? sourceParentId, bool IsAsyncCall = true)
        {
            if (IsBusy) return null;
            IsBusy = true;
            List<ItemCategoryLookup> _gotItemCategoryLookups   = null;

            if (IsAsyncCall)
                _gotItemCategoryLookups = (await appUnitOfWork.itemCategoryLookupRepository.GetByAsync(icl => icl.ParentCategoryId == sourceParentId)).ToList();
            else
                _gotItemCategoryLookups = appUnitOfWork.itemCategoryLookupRepository.GetBy(icl => icl.ParentCategoryId == sourceParentId).ToList();
            IsBusy = false;
            return _gotItemCategoryLookups;
        }
    }
}
