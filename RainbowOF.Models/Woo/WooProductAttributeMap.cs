﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RainbowOF.Models.Woo
{
    /// ItemAttributeId	Int	FK to ItemAttrib
    /// WooProductAttributeId Int Links to woo attribute
    public class WooProductAttributeMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid WooProductAttributeMapId { get; set; }
        public int WooProductAttributeId { get; set; }
        public Guid ItemAttributeLookupId { get; set; }
        [DefaultValue(true)]
        [DisplayName("Can Update?")]
        public bool CanUpdate { get; set; }


    }
}
