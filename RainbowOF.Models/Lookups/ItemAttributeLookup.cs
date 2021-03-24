using RainbowOF.Models.System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RainbowOF.Models.Lookups
{
    //  Desc Type    Comments
    //  ItemAttributeId Int Pk
    //  AttributeName String(100) Required, indexed
    //  OrderBy Enum? (custom, name, number and id)	Should this be an enum?
    //  Notes String
    public class ItemAttributeLookup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemAttributeLookupId { get; set; }
        [DisplayName("Attribute Name")]
        [Required]
        [StringLength(100)]
        public string AttributeName { get; set; }
        [DefaultValue(OrderBys.None)]
        public OrderBys OrderBy { get; set; }
        public string Notes { get; set; }
        // Added 24 Feb 2020
        [ForeignKey("ItemAttributeLookupId")]
        public List<ItemAttributeVarietyLookup> ItemAttributeVarietyLookups { get; set; }    /// this can be null - will return a count zero if there is one under eager loading    
    }
}
