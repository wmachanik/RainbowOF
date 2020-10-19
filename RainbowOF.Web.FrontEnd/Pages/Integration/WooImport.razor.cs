using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.System;
using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using RanbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImport : ComponentBase
    {
        public bool collapseWooVisible = true;
        public bool IsSaved = false;
        public bool IsChanged = false;
        public WooSettings WooSettingsModel;
        protected ShowModal ShowSavedStatus { get; set; }
        protected ShowModal ShowChangedStatus { get; set; }
        [Inject]
        public IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        public ILoggerManager _logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            StateHasChanged();
            await LoadWooPrefs();
            //return base.OnInitializedAsync();
        }

        private async Task LoadWooPrefs()
        {
            IAppRepository<WooAPISettings> _WooPrefs = _AppUnitOfWork.Repository<WooAPISettings>();
            StateHasChanged();
            await Task.Run(() => WooSettingsModel = _WooPrefs.GetAll().FirstOrDefault());
            IsSaved = false;
            StateHasChanged();
        }
        public async void HandleValidSubmit()
        {
            if (WooSettingsModel != null)
            {
                /// ShowSaving();
                IAppRepository<WooSettings> _WooSettings = _AppUnitOfWork.Repository<WooSettings>();
                // save
                await _WooSettings.UpdateAsync(WooSettingsModel);
                //                HideSaving();
                IsSaved = true;
                IsChanged = false;
                ShowSavedStatus.Show();
                // StateHasChanged();
            }
        }


        public void HandleInvalidSubmit()
        {
            IsSaved = false;
        }

        protected void StatusClosed_Click()
        {
            StateHasChanged();
        }


        public void StartWooImport()
        {
            // start the process of the WooImport
            // check if saved.
            if (IsChanged)
            {
                ShowChangedStatus.Show();
            }
            else
            {

            }
        }
    }
}
