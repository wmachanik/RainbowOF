using Blazorise;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Lookups
{
    public partial class NewUoMLookupComponent : ComponentBase
    {
        #region Injected and parameter variables
        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        [Parameter]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public EventCallback<bool> UoMAddedEvent { get; set; }
        #endregion
        #region Private variables
        private Modal newUoMModalRef { get; set; }
        private ItemUoMLookup newItemUoM { get; set; } = new();
        private Dictionary<Guid, string> _listOfUoMSymbols = null;
        #endregion
        #region Initialisation
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            //SelectedUoMId = ((SourceUoMId ?? Guid.Empty) == Guid.Empty) ? Guid.Empty : (Guid)SourceUoMId;  // store in a local var to keep state until modal closed. If not Select list changes to original value
            if (_listOfUoMSymbols == null)
                _listOfUoMSymbols = AppUnitOfWork.GetListOfUoMSymbols();
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Component back end methods
        public async Task ShowModalAsync()
        {
            newItemUoM.UoMName = "each";
            newItemUoM.UoMSymbol = "@";
            newItemUoM.BaseUoMId = null;
            newItemUoM.BaseConversationFactor = 1;
            newItemUoM.RoundTo = 2;
            await newUoMModalRef.Show();
        }
        private async Task HideModal(bool IsSaveClicked)
        {
            if (IsSaveClicked)
            {
                IRepository<ItemUoMLookup> appRepository = AppUnitOfWork.Repository<ItemUoMLookup>();
                if (appRepository != null)
                {
                    var _result = await appRepository.AddAsync(newItemUoM);
                    if (_result == null)
                        await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error adding new Unit of Measure: {newItemUoM.UoMName}");
                    else
                        await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Unit of Measure: {newItemUoM.UoMName} added.");
                }
            }
            await newUoMModalRef.Hide();
            await UoMAddedEvent.InvokeAsync(IsSaveClicked);   // tell the parent if we saved or not -> We could change SavedClicked if there was an error.
        }
        #endregion

    }
}
