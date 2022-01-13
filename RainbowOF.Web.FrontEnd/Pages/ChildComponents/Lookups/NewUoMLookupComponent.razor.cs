using Blazorise;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Lookups
{
    public partial class NewUoMLookupComponent : ComponentBase
    {
        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Parameter]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public EventCallback<bool> UoMAddedEvent { get; set; }

        private Modal NewUoMModalRef;

        public ItemUoMLookup _NewItemUoM = new();
        private Dictionary<Guid, string> _ListOfUoMSymbols = null;

        protected override async Task OnInitializedAsync()
        {
            //SelectedUoMId = ((SourceUoMId ?? Guid.Empty) == Guid.Empty) ? Guid.Empty : (Guid)SourceUoMId;  // store in a local var to keep state until modal closed. If not Select list changes to original value
            if (_ListOfUoMSymbols == null)
                _ListOfUoMSymbols =  _AppUnitOfWork.GetListOfUoMSymbols();
            await InvokeAsync(StateHasChanged);
        }

        public async Task ShowModalAsync()
        {
            _NewItemUoM.UoMName = "each";
            _NewItemUoM.UoMSymbol = "@";
            _NewItemUoM.BaseUoMId = null;
            _NewItemUoM.BaseConversationFactor = 1;
            _NewItemUoM.RoundTo = 2;
            await NewUoMModalRef.Show();
        }

        private async Task HideModal(bool IsSaveClicked)
        {
            if (IsSaveClicked)
            {
                IAppRepository<ItemUoMLookup> appRepository = _AppUnitOfWork.Repository<ItemUoMLookup>();
                if (appRepository != null)
                {
                    var _result = await appRepository.AddAsync(_NewItemUoM);
                    if (_result == null)
                        PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding new Unit of Measure: {_NewItemUoM.UoMName}");
                    else
                        PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Unit of Measure: {_NewItemUoM.UoMName} added.");
                }
            }
            await NewUoMModalRef.Hide();
            await UoMAddedEvent.InvokeAsync(IsSaveClicked);   // tell the parent if we saved or not -> We could change SavedClicked if there was an error.
        }

    }
}
