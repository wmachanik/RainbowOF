using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.System;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImport : ComponentBase
    {
        private bool collapseWooVisible { get; set; } = true;
        private bool collapseWooCategoriesImport { get; set; } = true;
        private bool isSaved { get; set; } = false;
        private bool isSaving { get; set; } = false;
        private bool isChanged { get; set; } = false;
        private WooSettings modelWooSettings { get; set; } = null;
        ////for saving
        //protected ShowModalMessage ShowSavedStatus { get; set; }
        ////for changing
        //protected ShowModalMessage ShowChangedStatus { get; set; }
        //protected PopUpAndLogNotification PopSavedStatus { get; set; }
        public PopUpAndLogNotification PopChangedStatus { get; set; }
        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        [Inject]
        public ILoggerManager AppLoggerManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            //   PopChangedStatus.ShowNotification(PopUpAndLogNotification.NotificationType.Info, "loading preferences");
            await LoadWooPrefs();
        }

        private async Task LoadWooPrefs()
        {
            IRepository<WooSettings> _wooPrefs = AppUnitOfWork.Repository<WooSettings>();
            //            StateHasChanged();
            modelWooSettings = _wooPrefs.FindFirst(); // await _wooPrefs.FindFirstAsync();
            if (modelWooSettings == null)
            {
                modelWooSettings = new WooSettings();   // if nothing send back a empty record
            }
            isSaved = false;
            await InvokeAsync(StateHasChanged);
            // StateHasChanged();
        }
        public async Task HandleValidSubmitAsync()
        {
            if (modelWooSettings != null)
            {
                ShowSaving();
                IRepository<WooSettings> _WooSettingsRepo = AppUnitOfWork.Repository<WooSettings>();
                // save
                // run this update regardless 
                if (modelWooSettings.WooSettingsId > 0)
                {
                    // it means that there was a record in the database.
                    isSaved = (await _WooSettingsRepo.UpdateAsync(modelWooSettings)) > 0;
                }
                else
                {
                    // it means that there was a record in the database.
                    isSaved = (await _WooSettingsRepo.AddAsync(modelWooSettings)) != null;
                }
                isChanged = false;
                if (isSaved) await PopChangedStatus.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, "Woo settings have been saved.");
                else await PopChangedStatus.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Error saving Woo settings. Please check database access.");
                HideSaving();
                //await ShowChangedStatus.ShowModalAsync();
                //StateHasChanged();
            }
        }
        public void ShowSaving()
        {
            isSaving = true;
            StateHasChanged();
        }
        public void HideSaving()
        {
            isSaving = false;
            StateHasChanged();
        }
        public void HandleInvalidSubmit()
        {
            isSaved = false;
        }
        protected void StatusClosed_Click()
        {
            StateHasChanged();
        }
        public void StartWooImport()
        {
            // start the process of the WooImport
            // check if saved.
            if (isChanged)
            {
                //ShowChangedStatus.ShowModalAsync();
            }
            else
            {

            }
        }
    }
}
