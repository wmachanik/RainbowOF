using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Models.Woo
{
    public class WooProductAttributeMapping
    {
        /// ItemAttributeID	Int	Pk with WooProductAttribID
        /// WooProductAttributeID Int Links to woo attribute
        public int ItemAttributeID { get; set; }
        public int WooProductAttributeID { get; set; }

    }
}
