using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Models.Woo
{
    //Desc                       Type    Comments
    // ------------------------+-------+-----------------------------------------
    //ItemAttributeVarietyID     Int     Pk with WooProductAttribTermID
    //WooProductAttributeTermID  Int     Links to woo attribute

    public class WooProductAttributeTermMapping
    {
        public int ItemAttributeVarietyID { get; set; }
        public int WooProductAttributeTermID { get; set; }
    }
}
