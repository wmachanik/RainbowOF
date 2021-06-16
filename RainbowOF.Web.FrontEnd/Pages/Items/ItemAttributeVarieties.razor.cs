using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.ViewModels.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemAttributeVarieties : ComponentBase
    {
        public Guid AttributeParentId = Guid.Empty;
        public ItemAttributeVarietiesComponent _VarietiesComponent;

        public Dictionary<Guid, string> _ItemAttributes = null;
        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IAppRepository<ItemAttributeLookup> _appRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();

            var _itemAttributeVarieties = await _appRepository.GetAllAsync();

            _ItemAttributes = new Dictionary<Guid, string>();
            foreach (var item in _itemAttributeVarieties)
            {
                _ItemAttributes.Add(item.ItemAttributeLookupId, item.AttributeName);
            }

            //await LoadData();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnSelectedAttributeChange(Guid newAtttributeID)
        {
            AttributeParentId = newAtttributeID;
            if (_VarietiesComponent != null) // then it has not been rendered
                await _VarietiesComponent.SetParentAttributeId(newAtttributeID);
        }
    }
}
