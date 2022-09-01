using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Woo
{
    /// <summary>
    /// Used to store how a Product Variant is mapped to Item Variant.
    /// Field                   Type    Comments
    /// WooProductVariantMapId  Guid    DatabaseGeneratedOption.Identity
    /// WooProductVariantID     int     Links to woo attribute
    /// ItemVariantID           Guid    key with WooProductVariantAttribID
    /// CanUpdate               bool    Set true by default – false if we do not want the Product Variant to be updated
    /// </summary>
    public class WooProductVariantMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid WooProductVariantMapId { get; set; }
        public Guid ItemVariantId { get; set; }
        public int WooProductVariantId { get; set; }  // changed to ulong from v 0.8.4 - changed back to 0.8.3
        [DefaultValue(true)]
        [DisplayName("Can Update?")]
        public bool CanUpdate { get; set; }
    }
}
