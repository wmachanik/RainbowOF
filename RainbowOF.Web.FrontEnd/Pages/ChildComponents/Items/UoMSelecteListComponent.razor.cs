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
        private Dictionary<Guid, string> ListOfUoMSymbols = null;


        protected override async Task OnInitializedAsync()
        {
            SelectedUoMId = ((SourceUoMId ?? Guid.Empty) == Guid.Empty) ? Guid.Empty : (Guid)SourceUoMId;  // store in a local var to keep state until modal closed. If not Select list changes to original value
            if (ListOfUoMSymbols == null)
                ListOfUoMSymbols = await Task.Run(() => _appUnitOfWork.GetListOfUoMSymbols());
        }
        //private async Task<Dictionary<Guid, string>> LoadUoMsFromData()
        //{

        //    Dictionary<Guid, string> _listOfUoMSymbols = new Dictionary<Guid, string>();
        //    IAppRepository<ItemUoM> _UoMRepository = _appUnitOfWork.Repository<ItemUoM>();

        //    List<ItemUoM> _itemUoMs = (await _UoMRepository.GetAllAsync()).ToList();
        //    if (_itemUoMs != null)
        //    {
        //        foreach (var item in _itemUoMs)
        //        {
        //            _listOfUoMSymbols.Add(item.ItemUoMId, item.UoMSymbol);
        //        }
        //    }
        //    IsUoMTableChecked = true;   // tell the component to show the add button
        //    return _listOfUoMSymbols;
        //}
        //public async Task<Dictionary<Guid, string>> GetListOfUoMs()
        //{
        //    //if we have not tried this before, and if the table is not blank.
        //    if ((!IsUoMTableChecked))   // && (_ListOfUoMSymbols == null))
        //    {
        //        if (ListOfUoMSymbols != null) ListOfUoMSymbols.Clear();
        //        ListOfUoMSymbols = await LoadUoMsFromData();
        //    }
        //    return ListOfUoMSymbols;
        //}
        private async Task ReloadUoMList()
        {
            if (ListOfUoMSymbols != null) ListOfUoMSymbols.Clear();
            var _result = await Task.Run(() => _appUnitOfWork.GetListOfUoMSymbols(true));
            ListOfUoMSymbols = _result;
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
