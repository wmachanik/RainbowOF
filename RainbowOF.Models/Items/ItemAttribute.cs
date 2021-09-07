using RainbowOF.Models.Lookups;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Items
{
    // Desc                 Type    Comments
    // ---------------------+-------+-----------------------------------------
    // ItemAttributeId        Guid    Pk with ItemId
    // ItemId                 Guid    Link to the Item that has this attribute
    // ItemAttributeLookupId  Guid    The actual Attribute FK  
    // IsUsedForVariableType  Bool    Is this used as a variable type 
    public class ItemAttribute
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemAttributeId { get; set; }
        public Guid ItemId { get; set; }
        public Guid ItemAttributeLookupId { get; set; }
        public bool IsUsedForItemVariety { get; set; }
        [ForeignKey("ItemAttributeLookupId")]
        public ItemAttributeLookup ItemAttributeDetail { get; set; }
        [ForeignKey("ItemAttributeId")]
        public List<ItemAttributeVariety> ItemAttributeVarieties { get; set; }
    }
}