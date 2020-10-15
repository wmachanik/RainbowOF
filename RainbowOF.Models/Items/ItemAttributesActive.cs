                    using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Models.Items
{
    // Desc                 Type    Comments
    // --------------------+-------+-----------------------------------------
    // ItemAttributeID       Int     Pk with ItemID
    // ItemID                Int     Link to the Item that has this attribute
    // IsUsedForVariableType Bool    Is this used as a variable type 

    class ItemAttributesActive
    {
        public int ItemAttributeID { get; set; }
        public int ItemID { get; set; }
        public bool IsUsedForVariableType { get; set; }
    }
}
