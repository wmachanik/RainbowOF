using RainbowOF.Models.System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RainbowOF.Models.Items
{
    //  Desc Type    Comments
    //  ItemAttributeID Int Pk
    //  AttributeName String(100) Required, indexed
    //  OrderBy Enum? (custom, name, num and id)	Should this be an enum?
    //  Notes String
    public class ItemAttribute
    {
        public int ItemAttributeID { get; set; }
        [DisplayName("Attribute Name")]
        [Required]
        [StringLength(100)]
        public string AttributeName { get; set; }
        [DefaultValue(OrderBys.None)]
        public OrderBys OrderBy { get; set; }
        public string Notes { get; set; }
    }
}
