using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RainbowOF.Models.System
{
    public class WooSettings
    {
        public int WooSettingsId { get; set; }
        [DisplayName("Woo Query URL")]
        [DefaultValue("www.mysite.com")]
        [StringLength(500)]
        public string QueryURL { get; set; }
        [DisplayName("Woo URL is secu`re")]
        [DefaultValue(true)]
        public bool IsSecureURL { get; set; } = true;
        [DisplayName("Consumer Key for Woo integration")]
        [StringLength(250)]
        public string ConsumerKey { get; set; }
        [DisplayName("Consumer Secret for Woo integration")]
        [StringLength(250)]
        public string ConsumerSecret { get; set; }
        [DisplayName("Root API Postfix")]
        [StringLength(100)]
        [DefaultValue("wc-api/v3")]
        public string RootAPIPostFix {get;set; } = "wc-api/v3";
        [DisplayName("WCObj Postfix")]
        [StringLength(100)]
        [DefaultValue("wp-json/wc/v3")]
        public string JSONAPIPostFix { get; set; } = "wp-json/wc/v3";
        [DefaultValue("true")]
        public bool AreCategoriesImported { get; set; } = true;  // are categories imported
        [DefaultValue("true")]
        public bool AreAttributesImported { get; set; } = true;  // are we importing attributes
        [DefaultValue("true")]
        public bool AreVarietiesMapped { get; set; } = true;   // are we mapping the attributes
        [DefaultValue("true")]
        public bool OnlyInStockItemsImported { get; set; } = true;   // are only items that are marked as in stock imported?
        [DefaultValue("true")]
        public bool AreAffiliateProdcutsImported { get; set; } = false;    // are affiliated products imported
        // default settings

    }
}
