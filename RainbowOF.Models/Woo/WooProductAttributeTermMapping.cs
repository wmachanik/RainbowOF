using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Models.Woo
{
    //Desc                       Type    Comments
    // ------------------------+-------+-----------------------------------------
    //ItemAttributeVarietyId     Int     Pk with WooProductAttribTermId
    //WooProductAttributeTermId  Int     Links to woo attribute

    public class WooProductAttributeTermMapping
    {
        public int ItemAttributeVarietyId { get; set; }
        public int WooProductAttributeTermId { get; set; }
    }
}
