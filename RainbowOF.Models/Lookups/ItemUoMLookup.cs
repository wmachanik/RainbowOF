﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RainbowOF.Models.Lookups
{
    /// <summary>
    /// ItemVariableUoM
    //  Des     c	             Type	    Comments
    // -----------------------+----------+-----------
    //  ItemVariableUoMId       Guid         Pk
    //  UoMName                 String(100) Should be smaller
    //  UoMSymbo                String(10)
    //  BaseUoMId               Guid?        If <> null points to the BaseUoM
    //  BaseConversationFactor  Double      If not a base what is the conversion to base
    //  RoundTo                 Int         Number of decimal to round to(default 4)
    /// </summary>
    public class ItemUoMLookup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemUoMLookupId { get; set; }   
        [Required]
        [StringLength(100)]
        [DisplayName("UoM Name")]
        public string UoMName { get; set; }
        [Required]
        [StringLength(10, ErrorMessage = "Unit of measure symbol must be of length 1-10")]
        [DisplayName("Unit of Measure symbol")]
        public string UoMSymbol { get; set; }
        [DisplayName("Does this UoM have a base")]
        public Guid? BaseUoMId { get; set; }
        [DisplayName("What is the conversion base")]
        [DefaultValue(0.0)]
        public double BaseConversationFactor { get; set; }
        [DisplayName("Round conversion too")]
        [DefaultValue(4)]
        public int RoundTo { get; set; }
        [ForeignKey("BaseUoMId")]
        public virtual ItemUoMLookup BaseUoM { get; set; }

        //[Timestamp]
        //public byte[] RowVersion { get; set; }
    }
}
