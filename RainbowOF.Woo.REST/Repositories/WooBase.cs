using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using System;
using WooCommerceNET;

namespace RainbowOF.Woo.REST.Repositories
{
    public class WooBase : IWooBase
    {

        public WooBase(WooAPISettings wooAPISettings, ILoggerManager logger)
        {
            //while (wooAPISettings.QueryURL.EndsWith("/"))
            //    wooAPISettings.QueryURL.Remove(wooAPISettings.QueryURL.Length - 1);

            _wooAPISettings = wooAPISettings;
            //_RestAPI = new RestAPI(wooAPISettings.FullSourceURL + "/wp-json/wc/v3/",
            //    wooAPISettings.ConsumerKey,
            //    wooAPISettings.ConsumerSecret, !wooAPISettings.IsSecureURL);

            this.Logger = logger;
        }

        private WooAPISettings _wooAPISettings;
        public WooAPISettings WooAPISettings
        {
            get { return _wooAPISettings; }
            set { _wooAPISettings = value; }
        }
        private ILoggerManager localAppLoggerManager { get; set; }
        public ILoggerManager Logger
        {
            get { return localAppLoggerManager; }
            set { localAppLoggerManager = value; }
        }

        public RestAPI GetJSONRestAPI
        {
            get
            {
                return new RestAPI(_wooAPISettings.FullSourceURL + _wooAPISettings.JSONAPIPostFix,
               _wooAPISettings.ConsumerKey,
               _wooAPISettings.ConsumerSecret,
               !_wooAPISettings.IsSecureURL);
            }
        }
        public RestAPI GetRootRestAPI
        {
            get
            {
                return new RestAPI(_wooAPISettings.FullSourceURL + _wooAPISettings.RootAPIPostFix,
               _wooAPISettings.ConsumerKey,
               _wooAPISettings.ConsumerSecret,
               !_wooAPISettings.IsSecureURL);
            }
        }

        public bool IsActive()
        {
            RestAPI _RestAPI = GetJSONRestAPI;
            Type _type = _RestAPI.GetType();
            return _type != null;
        }
    }
}
