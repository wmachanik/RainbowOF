using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RainbowOF.Models.Items
{
    /// <summary>
    /// ItemVariableUoM
    //  Des     c	             Type	    Comments
    // -----------------------+----------+-----------
    //  ItemVariableUoMID       Int         Pk
    //  UoMName                 String(100) Should be smaller
    //  UoMSymbo                String(10)
    //  BaseUoMID               Int?        If>0 points to the BaseUoM
    //  BaseConversationFactor  Double      If not a base what is the conversion to base
    //  RoundTo                 Int         Number of decimal to round to(default 4)
    /// </summary>
    public class ItemUoM
    {
        public int ItemUoMID { get; set; }
        [Required]
        [StringLength(100)]
        [DisplayName("UoM Name")]
        public string UoMName { get; set; }
        [Required]
        [StringLength(10, ErrorMessage = "Unit of measure symbol must be of length 1-10")]
        [DisplayName("Unit of Measure symbol")]
        public string UoMSymbol { get; set; }
        [DisplayName("Does this UoM has a base")]
        [DefaultValue(0)]
        public int BaseUoMID { get; set; }
        [DisplayName("What is the conversion base")]
        [DefaultValue(0.0)]
        public double BaseConversationFactor { get; set; }
        [DisplayName("Round conversion too")]
        [DefaultValue(4)]
        public int RoundTo { get; set; }

        //[Timestamp]
        //public byte[] RowVersion { get; set; }
    }
}
