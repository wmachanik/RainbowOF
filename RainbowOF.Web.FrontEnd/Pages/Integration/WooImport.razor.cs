using Microsoft.AspNetCore.Components;
using RainbowOF.Models.System;
using RainbowOF.Tools;
using RainbowOF.Web.FrontEnd.Pages.ChildComponents.Modals;
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
        public bool collapseWooCategoriesImport = true;
        public bool IsSaved = false;
        public bool IsSaving = false;
        public bool IsChanged = false;
        public WooSettings WooSettingsModel { get; set; } = null;
        //for saving
        protected ShowModalMessage ShowSavedStatus { get; set; }
        //for changing
        protected ShowModalMessage ShowChangedStatus { get; set; }
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
            IAppRepository<WooSettings> _WooPrefs = _AppUnitOfWork.Repository<WooSettings>();

            StateHasChanged();

             WooSettingsModel = await _WooPrefs.FindFirstAsync();
            if (WooSettingsModel == null)
            {
                WooSettingsModel = new WooSettings();   // if nothing send back a empty record
            }

            IsSaved = false;
            StateHasChanged();
        }
        public async void HandleValidSubmit()
        {
            if (WooSettingsModel != null)
            {
                ShowSaving();
                IAppRepository<WooSettings> _WooSettingsRepo = _AppUnitOfWork.Repository<WooSettings>();
                // save
                // run this update regardless 
                if (WooSettingsModel.WooSettingsId > 0)
                {
                    // it means that there was a record in the database.
                    IsSaved =  (await _WooSettingsRepo.UpdateAsync(WooSettingsModel)) > 0;
                }
                else
                {
                    // it means that there was a record in the database.
                    IsSaved = (await _WooSettingsRepo.AddAsync(WooSettingsModel)) > 0;
                }
                IsChanged = false;
                if (IsSaved) ShowChangedStatus.UpdateModalMessage("Woo settings have been saved.");
                else ShowChangedStatus.UpdateModalMessage("Error  saving Woo settings. Please check database access.");
                HideSaving();
                ShowChangedStatus.ShowModal();
                StateHasChanged();
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
                //ShowChangedStatus.ShowModal();
            }
            else
            {

            }
        }
    }
}
