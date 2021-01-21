using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RainbowOF.Models.Items
{
    public class ItemAttributeVariety
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemAttributeVarietyId { get; set; }
        public Guid ItemAttributeId { get; set; }  
        [Required]
        [StringLength(100)]
        [DisplayName("Variety Type")]
        public string VarietyName { get; set; }
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

        [ForeignKey("ItemAttributeId")]
        public ItemAttribute ItemAttribute;
    }
}
