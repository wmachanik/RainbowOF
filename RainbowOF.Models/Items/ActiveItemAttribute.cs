                    using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Models.Items
{
    // Desc                 Type    Comments
    // --------------------+-------+-----------------------------------------
    // ItemAttributeId       Int     Pk with ItemId
    // ItemId                Int     Link to the Item that has this attribute
    // IsUsedForVariableType Bool    Is this used as a variable type 

    public class ActiveItemAttribute
    {
        public int ActiveItemAttributeId { get; set; }
        public int ItemId { get; set; }
        public bool IsUsedForVariableType { get; set; }
    }
}
