using Microsoft.AspNetCore.Components;
using RainbowOF.Models.System;
using RainbowOF.Tools;
using RainbowOF.Components.Modals;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RainbowOF.Tools.Services;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImport : ComponentBase
    {
        public bool collapseWooVisible = true;
        public bool collapseWooCategoriesImport = true;
        public bool IsSaved = false;
        public bool IsSaving = false;
        public bool IsChanged = false;
        public WooSettings modelWooSettings { get; set; } = null;
        ////for saving
        //protected ShowModalMessage ShowSavedStatus { get; set; }
        ////for changing
        //protected ShowModalMessage ShowChangedStatus { get; set; }
        //protected PopUpAndLogNotification PopSavedStatus { get; set; }
        protected PopUpAndLogNotification PopChangedStatus { get; set; }

        [Inject]
        private IUnitOfWork appUnitOfWork { get; set; }
        [Inject]
        private ILoggerManager appLoggerManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            //   PopChangedStatus.ShowNotification(PopUpAndLogNotification.NotificationType.Info, "loading preferences");
            await LoadWooPrefs();
        }

        private async Task LoadWooPrefs()
        {
            IRepository<WooSettings> _wooPrefs = appUnitOfWork.Repository<WooSettings>();

            //            StateHasChanged();

            modelWooSettings =  _wooPrefs.FindFirst(); // await _wooPrefs.FindFirstAsync();
            if (modelWooSettings == null)
            {
                modelWooSettings = new WooSettings();   // if nothing send back a empty record
            }

            IsSaved = false;
            await InvokeAsync(StateHasChanged);
            // StateHasChanged();
        }
        public async Task HandleValidSubmitAsync()
        {
            if (modelWooSettings != null)
            {
                ShowSaving();
                IRepository<WooSettings> _WooSettingsRepo = appUnitOfWork.Repository<WooSettings>();
                // save
                // run this update regardless 
                if (modelWooSettings.WooSettingsId> 0)
                {
                    // it means that there was a record in the database.
                    IsSaved =  (await _WooSettingsRepo.UpdateAsync(modelWooSettings))> 0;
                }
                else
                {
                    // it means that there was a record in the database.
                    IsSaved = (await _WooSettingsRepo.AddAsync(modelWooSettings)) != null;
                }
                IsChanged = false;
                if (IsSaved) await PopChangedStatus.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success,"Woo settings have been saved.");
                else await PopChangedStatus.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Error saving Woo settings. Please check database access.");
                HideSaving();
                //await ShowChangedStatus.ShowModalAsync();
                //StateHasChanged();
            }
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
                //ShowChangedStatus.ShowModalAsync();
            }
            else
            {

            }
        }
    }
}
