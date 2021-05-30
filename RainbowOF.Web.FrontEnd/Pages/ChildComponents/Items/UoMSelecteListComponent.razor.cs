using Blazorise;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Items;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Items
{
    public partial class UoMSelecteListComponent : ComponentBase
    {
        [Inject]
        IAppUnitOfWork _appUnitOfWork { get; set; }
        [Parameter]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public Guid? SourceUoMId { get; set; }
        [Parameter]
        public EventCallback<Guid> UoMIdChanged { get; set; }

        private bool IsUoMTableChecked = false;
        private NewUoMComponent NewUoMComponentRef;
        private Guid SelectedUoMId;
        Dictionary<Guid, string> _ListOfUoMs = null;

        protected override async Task OnInitializedAsync()
        {
            SelectedUoMId = ((SourceUoMId ?? Guid.Empty) == Guid.Empty) ? Guid.Empty : (Guid)SourceUoMId;  // store in a local var to keep state until modal closed. If not Select list changes to original value
            _ListOfUoMs = await GetListOfUoMs();
        }
        private async Task<Dictionary<Guid, string>> LoadUoMsFromData()
        {
            Dictionary<Guid, string>  _listOfUoMs = new Dictionary<Guid, string>();
            IAppRepository<ItemUoM> _UoMRepository = _appUnitOfWork.Repository<ItemUoM>();
            List<ItemUoM> _listOfitemUoMs = (await _UoMRepository.GetAllAsync()).ToList();
            if (_listOfitemUoMs != null)
            {
                foreach (var item in _listOfitemUoMs)
                {
                    _listOfUoMs.Add(item.ItemUoMId, item.UoMSymbol);
                }
                IsUoMTableChecked = true;   // tell the component to show the add button
            }
            return _listOfUoMs;
        }

        public async Task<Dictionary<Guid, string>> GetListOfUoMs()
        {
            //if we have not tried this before, and if the table is not blank.
            if ((!IsUoMTableChecked) && (_ListOfUoMs == null))
            {
                _ListOfUoMs = await LoadUoMsFromData();
            }
            return _ListOfUoMs;
        }
        private async Task ReloadUoMList()
        {
            _ListOfUoMs.Clear();
            _ListOfUoMs = await LoadUoMsFromData();
            StateHasChanged();
        }
        protected async Task OnUoMIdChanged(Guid newUoMId)
        {
            // if they want to add a new one then do so, otherwise change it and send the value back to the parent component   
            SelectedUoMId = newUoMId;
            await UoMIdChanged.InvokeAsync(newUoMId);
        }

    }
}
