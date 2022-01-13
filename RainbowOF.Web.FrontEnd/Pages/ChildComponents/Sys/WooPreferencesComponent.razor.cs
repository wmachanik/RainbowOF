using Microsoft.AspNetCore.Components;
using RainbowOF.Models.System;
using RainbowOF.Tools;
using RainbowOF.Components.Modals;
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
        protected ShowModalMessage ShowModalStatus { get; set; }
        public async Task CheckWooStatusAsync()
        {
            Waiting = true;
            StateHasChanged();
            WooAPISettings _WooAPISettings = new WooAPISettings(WooSettingsModel);
            //{
            //    ConsumerKey = WooSettingsModel.ConsumerKey,
            //    ConsumerSecret = WooSettingsModel.ConsumerSecret,
            //    QueryURL = WooSettingsModel.QueryURL,
            //    IsSecureURL = WooSettingsModel.IsSecureURL,
            //    JSONAPIPostFix = WooSettingsModel.JSONAPIPostFix,
            //    RootAPIPostFix = WooSettingsModel.RootAPIPostFix
            //};

            WooProduct _WooProducts = new WooProduct(_WooAPISettings, Logger);

            //int _count = await Task.Run(() => _WooProducts.GetProductCount());    //CheckProductLink())
            //WooStatus = ((_count > 0) ? $"Success - product count: {_count}" : "Failed");

            bool _success = await Task.Run(() => _WooProducts.CheckProductLinkAsync());   // GetProductCount());    //
            WooStatus = (_success ? $"Success" : "Failed");

            Waiting = false;
            StateHasChanged();
            ShowModalStatus.UpdateModalMessage($"Woo API call status: {Environment.NewLine}{Environment.NewLine}  {WooStatus}");
            await ShowModalStatus.ShowModalAsync();
        }

        protected void StatusClosed_Click()
        {
            StateHasChanged();
        }
    }
}

