using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;

namespace RainbowOF.Woo.REST.Repositories
{
    public class WooBase : IWooBase
    {
        private WooAPISettings _WooAPISettings;
        private ILoggerManager appLoggerManager;

        public WooBase(WooAPISettings wooAPISettings, ILoggerManager logger)
        {
            //while (wooAPISettings.QueryURL.EndsWith("/"))
            //    wooAPISettings.QueryURL.Remove(wooAPISettings.QueryURL.Length - 1);

            _WooAPISettings = wooAPISettings;
            //_RestAPI = new RestAPI(wooAPISettings.FullSourceURL + "/wp-json/wc/v3/",
            //    wooAPISettings.ConsumerKey,
            //    wooAPISettings.ConsumerSecret, !wooAPISettings.IsSecureURL);

            this.appLoggerManager = logger;
        }

        public WooAPISettings WooAPISettings
        {
            get { return _WooAPISettings; }
            set { _WooAPISettings = value; }
        }

        public ILoggerManager Logger
        {
            get { return appLoggerManager; }
            set { appLoggerManager = value; }
        }

        public RestAPI GetJSONRestAPI
        {
            get
            {
                return new RestAPI(_WooAPISettings.FullSourceURL + _WooAPISettings.JSONAPIPostFix,
               _WooAPISettings.ConsumerKey,
               _WooAPISettings.ConsumerSecret,
               !_WooAPISettings.IsSecureURL);
            }
        }
        public RestAPI GetRootRestAPI
        {
            get
            {
                return new RestAPI(_WooAPISettings.FullSourceURL + _WooAPISettings.RootAPIPostFix,
               _WooAPISettings.ConsumerKey,
               _WooAPISettings.ConsumerSecret,
               !_WooAPISettings.IsSecureURL);
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
