using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RainbowOF.Models.Woo
{
    /// ItemAttributeId	Int	FK to ItemAttrib
    /// WooProductAttributeId Int Links to woo attribute
    public class WooProductMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid WooProductMapId { get; set; }
        public int WooProductId { get; set; }
        public Guid ItemId { get; set; }
        [DefaultValue(true)]
        [DisplayName("Can Update?")]
        public bool CanUpdate { get; set; }

    }
}
