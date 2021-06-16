using Blazorise;
using Microsoft.AspNetCore.Components;
using RainbowOF.FrontEnd.Models;
using RainbowOF.Tools;
using RainbowOF.Components.Modals;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.System;
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
        protected ShowModalMessage ShowSavedStatus { get; set; }
        [Inject]
        private IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        private ILoggerManager _Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            StateHasChanged();
            await LoadSysPrefs();
            //return base.OnInitializedAsync();
        }
/*
 *      public string ModalTitle = "Saving Status";
        public string ModalMessage = "System Preferences Saved";

        private Modal modalRef;

        private void ShowModal()
        {
            modalRef.Show();
        }

        private void HideModal()
        {
            modalRef.Hide();
        }
*/
        private async Task LoadSysPrefs()
        {
            ISysPrefsRepository _SysPref = _AppUnitOfWork.sysPrefsRepository();
            StateHasChanged();
            await Task.Run(() => SysPrefsModel = _SysPref.GetSysPrefs());
            IsSaved = false;
            StateHasChanged();
        }
        public async void HandleValidSubmit()
        {
            if (SysPrefsModel != null)
            {
                ShowSaving();
                ISysPrefsRepository _SysPref = _AppUnitOfWork.sysPrefsRepository();
                // save
                bool _Saved = await _SysPref.UpdateSysPreferencesAsync(SysPrefsModel);

                if (!_Saved)
                    ShowSavedStatus.UpdateModalMessage("Error saving preferences");
                else
                    ShowSavedStatus.ShowModal();

                HideSaving();
                //ShowModal();
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
