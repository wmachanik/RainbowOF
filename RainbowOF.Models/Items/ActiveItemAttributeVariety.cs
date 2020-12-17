using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RainbowOF.Models.Items
{
    //    Desc                          Type         Comments
    // -------------------------------+-------------+-----------------------------------------
    // ItemAttributeVarietiesActiveId   Int           Pk
    // ItemId                           Int           Link to the Item that has this attribute
    // ItemAttributeId                  Int           Links to parent attribute (could be excluded)
    // IsDefault	                    Bool	      Is this the default variety (of variabletype)
    // UoMId	                        Int?	      ->ItemUoM.Id
    // UoMQtyPerItem                    double(12,6)  The quantity this is for UoM
    public class ActiveItemAttributeVariety
    {
        public int ActiveItemAttributeVarietyId { get; set; }
        public int ItemId { get; set; }
        public int ItemAttributeId { get; set; }
        [DefaultValue(false)]
        public bool IsDefault{ get; set; }
        public int? ItemUoMId { get; set; }
        [DefaultValue(1.0)]
        public double UoMQtyPerItem { get; set; }

        [ForeignKey(nameof(ItemUoMId))]
        public virtual ItemUoM ItemUoM { get; set; }
    }
}
