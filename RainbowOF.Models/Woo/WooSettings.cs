using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RainbowOF.Models.System
{
    public class WooSettings
    {
        public int WooSettingsID { get; set; }
        [DisplayName("Woo Query URL")]
        [DefaultValue("www.mysite.com")]
        [StringLength(500)]
        public string QueryURL { get; set; }
        [DisplayName("Woo URL is secure")]
        [DefaultValue(true)]
        public bool IsSecureURL { get; set; }
        [DisplayName("Customer Key for Woo integration")]
        [StringLength(250)]
        public string CustomerKey { get; set; }
        [DisplayName("Customer Secret for Woo integration")]
        [StringLength(250)]
        public string CustomerSecret { get; set; }
        [DisplayName("Root API Postfix")]
        [StringLength(100)]
        [DefaultValue("wc-api/v3")]
        public string RootAPIPostFix {get;set;}
        [DisplayName("WCObj Postfix")]
        [StringLength(100)]
        [DefaultValue("wp-json/wc/v3")]
        public string JSONAPIPostFix { get; set; }
        [DefaultValue("true")]
        public bool AreCategoriesImported { get; set; }
        [DefaultValue("true")]
        public bool AreAttributesImported { get; set; }
        [DefaultValue("true")]
        public bool AreVarietiesMapped { get; set; }
        [DefaultValue("true")]
        public bool OnlyInStockItemsImported { get; set; }
        [DefaultValue("false")]
        public bool AreItemQuantatiesImported { get; set; }


    }
}
