using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using RanbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public class ImportBase : ComponentBase
    {
        public bool collapseWooVisible = true;
        public bool Saved = false;
        public WooAPISettings WooAPIModel;
        protected ShowModal ShowSavedStatus { get; set; }

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
            await Task.Run(() => WooAPIModel = _WooPrefs.GetAll().FirstOrDefault());
            Saved = false;
            StateHasChanged();
        }
        public async void HandleValidSubmit()
        {
            if (WooAPIModel != null)
            {
                /// ShowSaving();
                IAppRepository<WooAPISettings> _WooPrefs = _AppUnitOfWork.Repository<WooAPISettings>();
                // save
                await _WooPrefs.UpdateAsync(WooAPIModel);
                //                HideSaving();
                Saved = true;
                ShowSavedStatus.Show();
                // StateHasChanged();
            }
        }


        public void HandleInvalidSubmit()
        {
            Saved = false;
        }

        protected void StatusClosed_Click()
        {
            StateHasChanged();
        }

    }
}
