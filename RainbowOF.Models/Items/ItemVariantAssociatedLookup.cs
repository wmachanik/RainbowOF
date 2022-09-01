using RainbowOF.Models.Lookups;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Items
{
    /// <summary>
    /// This links all the associated attributes and attribute variations to a single Item Variant
    /// 
    /// Field                               Type            Comments
    /// ItemVariantAssociationsId           GUID            Unique to IT not imported
    /// ItemVariantId                       GUID            Unique to IT not imported
    /// AssociatedAttributeLookupId         GUID?            Associated Attribute Lookup Id for this variant
    /// AssociatedAttributeVarietyLookupId  GUID?           Unique to IT not imported
    /// </summary>  
    public class ItemVariantAssociatedLookup
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemVariantAssociatedLookupId { get; set; }
        [Required]   // cannot have a variant without a Parent Item this points to the parent item
        public Guid ItemVariantId { get; set; }
        [Required]   // Need an Attribute to exist
        public Guid AssociatedAttributeLookupId { get; set; }
        // can be null for when applicable to all attributes
        public Guid? AssociatedAttributeVarietyLookupId { get; set; }
        [ForeignKey("AssociatedAttributeLookupId ")]
        virtual public ItemAttributeLookup AssociatedAttributeLookup { get; set; }
        [ForeignKey("AssociatedAttributeVarietyLookupId ")]
        virtual public ItemAttributeVarietyLookup AssociatedAttributeVarietyLookup { get; set; }
    }
}
