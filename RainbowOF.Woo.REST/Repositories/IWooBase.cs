using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using WooCommerceNET;

namespace RainbowOF.Woo.REST.Repositories
{
    public interface IWooBase
    {
        WooAPISettings WooAPISettings { get; set; }
        ILoggerManager Logger { get; set; }
        RestAPI GetJSONRestAPI { get; }
        RestAPI GetRootRestAPI { get; }
        bool IsActive();

    }
}
