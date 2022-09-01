using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Lookups
{
    public class ItemAttributeVarietyLookup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemAttributeVarietyLookupId { get; set; }
        public Guid ItemAttributeLookupId { get; set; }
        [Required]
        [StringLength(100)]
        [DisplayName("Variety Type")]
        public string VarietyName { get; set; }
        [DisplayName("Type of Unit of Measure (option)")]
        public Guid? UoMId { get; set; }
        [DisplayName("If this UoM is not the base UoM then what is this factor")]
        [DefaultValue(1.0)]
        public double UoMQtyPerItem { get; set; }    // added 17 Nov 2021
        [StringLength(10)]
        [DisplayName("Default Suffix for variant's SKU")]
        public string DefaultSKUSuffix { get; set; }  // added 17 Nov 2021
        public int SortOrder { get; set; }
        [StringLength(2)]
        public string Symbol { get; set; }
        [StringLength(11)]
        public string FGColour { get; set; }
        [StringLength(11)]
        [DisplayName("Background Colour")]
        public string BGColour { get; set; }
        [DisplayName("Additional Notes")]
        public string Notes { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; }

        [ForeignKey("ItemAttributeLookupId")]
        public ItemAttributeLookup ItemAttributeLookup;
        [ForeignKey("UoMId")]
        public ItemUoMLookup UoM;

        public const string CONST_NULL_COLOUR = "#00000000";
    };
}
