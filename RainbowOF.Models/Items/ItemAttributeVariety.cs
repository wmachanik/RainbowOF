﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Items
{

    //    Desc                          Type         Comments
    // ------------------------+-------------+-----------------------------------------
    // ItemAttributeVarietyId   Int           Pk
    // ItemId                   Int           Link to the Item that has this attribute
    // ItemAttributeId          Int           Links to parent attribute (could be excluded)
    // IsDefault	            Bool	      Is this the default variety (of variabletype)
    // UoMId	                Int?	      ->ItemUoM.Id
    // UoMQtyPerItem            double(12,6)  The quantity this is for UoM
    public class ItemAttributeVariety
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemAttributeVarietyId { get; set; }
        public Guid ItemId { get; set; }
        public Guid ItemAttributeVarietyLookupId { get; set; }
        [DefaultValue(false)]
        public bool IsDefault { get; set; }
        public Guid? ItemUoMId { get; set; }
        [DefaultValue(1.0)]
        public double UoMQtyPerItem { get; set; }
        [ForeignKey("ItemUoMId")]
        public virtual ItemUoM ItemUoM { get; set; }
    }
}