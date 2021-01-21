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
    public interface IWooBase
    {
        WooAPISettings WooAPISettings { get; set; }
        ILoggerManager Logger { get; set; }
        RestAPI GetJSONRestAPI { get; }
        RestAPI GetRootRestAPI { get; }

    }
}
