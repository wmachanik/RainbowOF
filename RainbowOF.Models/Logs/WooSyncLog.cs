using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RainbowOF.Models.Logs
{
    /// <summary>
    /// WooSyncLog Tablet
    /// Desc	            Type	    Comments
    /// WooSyncLogId        Int         PK
    /// WooSyncDateTime     DateTime    The Date and time the sync was done
    /// Section             Enum        Which section: Coupons, Customers, Order, OrderRefunds, Product, ProductAttributes, ProductAttributeTerms, ProductCategories, ProductTags, WebHooks, Taxes or System
    /// SectionID           Int         Id if queried otherwise “0”.
    /// Result              Enum        Success, Error, Timeout
    /// Parameters          String      What was sent
    /// Notes               String      Any notes

    /// </summary>
    public class WooSyncLog
    {
        public int WooSyncLogId { get; set; }
        [DisplayName("Date and time the sync was done")]
        public DateTime WooSyncDateTime { get; set; }
/// 
/// //
/// 
/// seCTION id - IS IT USED?
/// 

        public WooSections Section { get; set; }
        [DisplayName("Id if queried otherwise “0”.")]
        public int SectionID { get; set; }
        public WooResults Result { get; set; }
        public string Parameters { get; set; }
        public string Notes { get; set; }

    }
}
