using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RainbowOF.Models.Items
{
    public class ItemAttributeVariety
    {
        public int ItemAttributeVarietyId { get; set; }
        public int ItemAttributeId { get; set; }  
        [Required]
        [StringLength(100)]
        [DisplayName("Variety Type")]
        public string VarietyName { get; set; }
        [StringLength(2)]
        public string Symbol { get; set; }
        [StringLength(11)]
        public int FGColour { get; set; }
        [StringLength(11)]
        [DisplayName("Background Colour")]
        public string BGColour { get; set; }
        [DisplayName("Additional Notes")]
        public string Notes { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; }
    }
}
