using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RainbowOF.Models.Items
{
    //    Desc                          Type         Comments
    // -------------------------------+-------------+-----------------------------------------
    // ItemAttributeVarietiesActiveID   Int           Pk
    // ItemID                           Int           Link to the Item that has this attribute
    // ItemAttributeID                  Int           Links to parent attribute (could be excluded)
    // IsDefault	                    Bool	      Is this the default variety (of variabletype)
    // UoMID	                        Int?	      ->ItemUoM.ID
    // UoMQtyPerItem                    double(12,6)  The quantity this is for UoM
    class ItemAttributeVarietiesActive
    {
        public int ItemAttributeVarietiesActiveID { get; set; }
        public int ItemID { get; set; }
        public int ItemAttributeID { get; set; }
        [DefaultValue(false)]
        public bool IsDefault{ get; set; }
        public int? ItemUoMID { get; set; }
        [DefaultValue(1.0)]
        public double UoMQtyPerItem { get; set; }

        [ForeignKey(nameof(ItemUoMID))]
        public virtual ItemUoM ItemUoM { get; set; }
    }
}
