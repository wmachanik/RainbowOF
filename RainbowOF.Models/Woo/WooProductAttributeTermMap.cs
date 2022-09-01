using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Woo
{
    //Desc                       Type    Comments
    // ------------------------+-------+-----------------------------------------
    //ItemAttributeVarietyId     Int     Pk with WooProductAttribTermId
    //WooProductAttributeTermId  Int     Links to woo attribute

    public class WooProductAttributeTermMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid WooProductAttributeTermMapId { get; set; }
        public Guid ItemAttributeVarietyLookupId { get; set; }
        public int WooProductAttributeTermId { get; set; }
        [DefaultValue(true)]
        [DisplayName("Can Update?")]
        public bool CanUpdate { get; set; }

    }
}
