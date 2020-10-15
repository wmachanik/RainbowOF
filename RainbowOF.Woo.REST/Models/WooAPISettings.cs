using RainbowOF.Models.System;
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
                return ((IsSecureURL) ? "https://" : "http://") + QueryURL ;
            }
        }

    }
}
