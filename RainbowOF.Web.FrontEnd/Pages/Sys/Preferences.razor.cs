using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.FrontEnd.Models;
using RainbowOF.Tools;
using RanbowOF.Repositories.Common;
using RanbowOF.Repositories.System;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Sys
{
    public partial class Preferences : ComponentBase
    {
        public SysPrefsModel SysPrefsModel { get; set; } = null;
        public bool Saved = false;
        public bool Saving = false;
        public bool collapseSysVisible = true;
        public bool collapseWooVisible = true;
        //bool collapse3Visible = false;
        protected ShowModal ShowSavedStatus { get; set; }

        [Inject]
        public IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        public ILoggerManager _logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            StateHasChanged();
            await LoadSysPrefs();
            //return base.OnInitializedAsync();
        }

        private async Task LoadSysPrefs()
        {
            ISysPrefsRepository _SysPref = _AppUnitOfWork.sysPrefsRepository();
            StateHasChanged();
            await Task.Run(() => SysPrefsModel = _SysPref.GetSysPrefs());
            Saved = false;
            StateHasChanged();
        }
        public async void HandleValidSubmit()
        {
            if (SysPrefsModel != null)
            {
                ShowSaving();
                ISysPrefsRepository _SysPref = _AppUnitOfWork.sysPrefsRepository();
                // save
                await _SysPref.UpdateSysPreferencesAsync(SysPrefsModel);

                HideSaving();
                ShowSavedStatus.Show();
                // StateHasChanged();
            }
        }


        public void HandleInvalidSubmit()
        {
            Saved = false;
        }

        public void ShowSaving()
        {
            Saving = true;
            StateHasChanged();
        }
        public void HideSaving()
        {
            Saving = false;
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
    }
}
