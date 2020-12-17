using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Models.Woo
{
    public class WooProductAttributeMapping
    {
        /// ItemAttributeId	Int	Pk with WooProductAttribId
        /// WooProductAttributeId Int Links to woo attribute
        public int WooProductAttributeMappingId { get; set; }
        public int WooProductAttributeId { get; set; }

    }
}
