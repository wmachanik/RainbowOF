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
        IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Parameter]
        public Guid? SourceParentCategoryId { get; set; }
        [Parameter]
        public bool CanUseAsync { get; set; } = true;  // issues with detail mean that if this is a detail grid we disable Async

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            ModelItemCategoryLookups = await GetItemCategoryLookupsAsync(SourceParentCategoryId, CanUseAsync);
//            await InvokeAsync(StateHasChanged);
        }

        async Task<List<ItemCategoryLookup>> GetItemCategoryLookupsAsync(Guid? sourceParentId, bool IsAsyncCall = true)
        {
            if (IsBusy) return null;
            IsBusy = true;
            var repo = _AppUnitOfWork.itemCategoryLookupRepository();

            List<ItemCategoryLookup> _gotItemCategoryLookups   = null;

            if (IsAsyncCall)
                _gotItemCategoryLookups = (await repo.GetByAsync(icl => icl.ParentCategoryId == sourceParentId)).ToList();
            else
                _gotItemCategoryLookups = repo.GetBy(icl => icl.ParentCategoryId == sourceParentId).ToList();
            //await InvokeAsync(StateHasChanged);

            IsBusy = false;
            return _gotItemCategoryLookups;
        }
    }
}
