using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RainbowOF.Models.Woo
{
    /// <summary>
    /// Used to store the link between our Item and the Woo Product
    /// Field                   Type    Comments
    /// WooProductMapId         Guid    DatabaseGeneratedOption.Identity
    /// ItemAttributeId	        Int	    FK to ItemAttrib
    /// WooProductAttributeId   Int     Links to woo attribute
    /// CanUpdate               bool    Set true by default – false if we do not want the Product Variant to be updated
    /// </summary>
    public class WooProductMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid WooProductMapId { get; set; }
        public Guid ItemId { get; set; }
        public int WooProductId { get; set; }   // 8.4 updates this to ulong, had an issues with this change, so rolled back to 08.8.3
        [DefaultValue(true)]
        [DisplayName("Can Update?")]
        public bool CanUpdate { get; set; }

    }
}
