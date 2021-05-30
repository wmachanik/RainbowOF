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
    public partial class NewUoMComponent : ComponentBase
    {
        [Inject]
        IAppUnitOfWork _appUnitOfWork { get; set; }
        [Parameter]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public EventCallback<bool> UoMAdded { get; set; }

        private Modal NewUoMModalRef;

        public ItemUoM _NewItemUoM = new ItemUoM();

        public void ShowModal() // (ModalSize modalSize, int? maxHeight = null, bool centered = false)
        {
            _NewItemUoM.UoMName = "each";
            _NewItemUoM.UoMSymbol = "@";
            _NewItemUoM.BaseUoMId = null;
            _NewItemUoM.BaseConversationFactor = 1;
            _NewItemUoM.RoundTo = 2;
            NewUoMModalRef.Show();
        }

        private async Task HideModal(bool SaveClicked)
        {
            if (SaveClicked)
            {
                IAppRepository<ItemUoM> appRepository = _appUnitOfWork.Repository<ItemUoM>();
                if (appRepository != null)
                {
                    int _result = await appRepository.AddAsync(_NewItemUoM);
                    if (_result == AppUnitOfWork.CONST_WASERROR)
                        PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding new Unit of Measure: {_NewItemUoM.UoMName}");
                    else
                        PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Unit of Measure: {_NewItemUoM.UoMName} added.");
                }
            }
            NewUoMModalRef.Hide();
            await UoMAdded.InvokeAsync(SaveClicked);   // tell the parent if we saved or not -> We could change SavedClicked if there was an error.
        }
    }
}
