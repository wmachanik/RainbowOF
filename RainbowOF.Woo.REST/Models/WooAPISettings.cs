﻿using RainbowOF.Models.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Woo.REST.Models
{
    public class WooAPISettings : WooSettings
    {
        public string FullSourceURL
        {
            get
            {
                if (!QueryURL.EndsWith("/")) QueryURL += "/";
                return ((IsSecureURL) ? "https://" : "http://") + QueryURL;
            }
        }

        public WooAPISettings()
        {

        }

        public WooAPISettings(WooSettings mapWooSettings)
        {
            MapAPISettings(mapWooSettings);
        }

        public void MapAPISettings(WooSettings mapWooSettings)
        {
            ConsumerKey = mapWooSettings.ConsumerKey;
            ConsumerSecret = mapWooSettings.ConsumerSecret;
            QueryURL = mapWooSettings.QueryURL;
            IsSecureURL = mapWooSettings.IsSecureURL;
            JSONAPIPostFix = mapWooSettings.JSONAPIPostFix;
            RootAPIPostFix = mapWooSettings.RootAPIPostFix;
        }

    }
}
