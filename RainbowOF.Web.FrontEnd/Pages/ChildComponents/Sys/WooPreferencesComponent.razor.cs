using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.System;
using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Sys
{
    public partial class WooPreferencesComponent : ComponentBase
    {
        [Parameter]
        public WooSettings WooSettingsModel { get; set; }
        [Parameter]
        public ILoggerManager Logger { get; set; }

        public bool ShowKey = false;
        public bool ShowSecret = false;
        public string WooStatus = "none";
        public bool Waiting = false;
        protected ShowModal ShowModalStatus { get; set; }
        public async void CheckWooStatus()
        {
            Waiting = true;
            StateHasChanged();
            WooAPISettings _WooAPISettings = new WooAPISettings
            {
                CustomerKey = WooSettingsModel.CustomerKey,
                CustomerSecret = WooSettingsModel.CustomerSecret,
                QueryURL = WooSettingsModel.QueryURL,
                IsSecureURL = WooSettingsModel.IsSecureURL,
                JSONAPIPostFix = WooSettingsModel.JSONAPIPostFix,
                RootAPIPostFix = WooSettingsModel.RootAPIPostFix
            };

            WooProducts _WooProducts = new WooProducts(_WooAPISettings, Logger);
            int _count = await Task.Run(() => _WooProducts.GetProductCount());    //.CheckProductLink());

            WooStatus = ((_count > 0) ? $"Success - product count: {_count}" : "Failed");
            Waiting = false;
            StateHasChanged();
            ShowModalStatus.UpdateModalMessage($"Woo API call status: {Environment.NewLine}{Environment.NewLine}  {WooStatus}");
            ShowModalStatus.Show();
        }

        protected void StatusClosed_Click()
        {
            StateHasChanged();
        }
    }
}

