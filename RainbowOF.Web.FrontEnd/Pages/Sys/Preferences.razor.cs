using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.FrontEnd.Models;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Sys
{
    public partial class Preferences : ComponentBase
    {
        public SysPrefsModel SysPrefsModel { get; set; } = null;
        public bool IsSaved = false;
        public bool IsSaving = false;
        public bool collapseSysVisible = true;
        public bool collapseWooVisible = true;
        //bool collapse3Visible = false;
        public PopUpAndLogNotification PopSavedStatus { get; set; }
        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        [Inject]
        public ILoggerManager AppLoggerManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            StateHasChanged();
            await LoadSysPrefs();
        }
        private async Task LoadSysPrefs()
        {
            StateHasChanged();
            await Task.Run(() => SysPrefsModel = AppUnitOfWork.SysPrefsRepository.GetSysPrefs());
            IsSaved = false;
            StateHasChanged();
        }
        public async Task HandleValidSubmit()
        {
            if (SysPrefsModel != null)
            {
                ShowSaving();
                // save
                bool _Saved = await AppUnitOfWork.SysPrefsRepository.UpdateSysPreferencesAsync(SysPrefsModel);

                if (!_Saved)
                    await PopSavedStatus.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Error saving preferences!");
                else
                    await PopSavedStatus.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Preferences saved.");

                HideSaving();
                //await ShowModalAsync();
                StateHasChanged();
            }
        }
        public void HandleInvalidSubmit()
        {
            IsSaved = false;
        }
        public void ShowSaving()
        {
            IsSaving = true;
            StateHasChanged();
        }
        public void HideSaving()
        {
            IsSaving = false;
            StateHasChanged();
        }
        protected void StatusClosed_Click()
        {
            StateHasChanged();
        }

        //public void ShowSecret()
        //{
        //    if (this.TxtType == "password")
        //    {
        //        this.TxtType = "text";
        //    }
        //    else
        //    {
        //        this.TxtType = "password";
        //    }
        //}
        //protected bool IsShowModal { get; set; }

        ////[Parameter]
        //public string ModalTitle { get; set; } = "Status Message";
        ////[Parameter]
        //public string ModalMessage { get; set; } = "Something Happened";
        ////[Parameter]
        //public EventCallback CloseModel { get; set; }

        //public void Show()
        //{
        //    IsShowModal = true;
        //    StateHasChanged();
        //}

        //public void UpdateModalMessage(string pMessage)
        //{
        //    ModalMessage = pMessage;
        //}

        //protected async Task OnCloseModal()
        //{
        //    IsShowModal = false;
        //    StateHasChanged();
        //    await CloseModel.InvokeAsync(null);
        //}

    }
}
