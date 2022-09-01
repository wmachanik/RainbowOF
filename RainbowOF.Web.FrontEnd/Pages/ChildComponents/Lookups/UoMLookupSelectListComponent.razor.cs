using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Lookups
{
    public partial class UoMLookupSelectListComponent : ComponentBase
    {
        #region Injected and parameter values
        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        [Parameter]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public Guid? SourceUoMId { get; set; }
        [Parameter]
        public EventCallback<Guid> UoMIdChangedEvent { get; set; }
        #endregion
        #region Private and local variables
        //        private bool IsUoMTableChecked = false;
        private NewUoMLookupComponent newUoMComponentRef { get; set; }
        private Guid selectedUoMId { get; set; }
        private Dictionary<Guid, string> _listOfUoMSymbols = null;
        #endregion
        #region Initialisation
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            selectedUoMId = ((SourceUoMId ?? Guid.Empty) == Guid.Empty) ? Guid.Empty : (Guid)SourceUoMId;  // store in a local var to keep state until modal closed. If not Select list changes to original value
            if (_listOfUoMSymbols == null)
                _listOfUoMSymbols = AppUnitOfWork.GetListOfUoMSymbols();
            await InvokeAsync(StateHasChanged);

        }
        #endregion
        #region Back end code
        //private async Task<Dictionary<Guid, string>> LoadUoMsFromData()
        //{

        //    Dictionary<Guid, string> _listOfUoMSymbols = new Dictionary<Guid, string>();
        //    IAppRepository<ItemUoM> _UoMRepository = AppUnitOfWork.Repository<ItemUoM>();

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
            if (_listOfUoMSymbols != null)
                _listOfUoMSymbols.Clear();
            _listOfUoMSymbols = AppUnitOfWork.GetListOfUoMSymbols(true);
            await InvokeAsync(StateHasChanged);
        }
        protected async Task OnUoMIdChanged(Guid newUoMId)
        {
            // if they want to add a new one then do so, otherwise change it and send the value back to the parent component   
            selectedUoMId = newUoMId;
            await UoMIdChangedEvent.InvokeAsync(newUoMId);
        }
        #endregion
    }
}
