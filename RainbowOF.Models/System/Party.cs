using iSele.Models.Lookups;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace iSele.Models.System
{
    public class Party
    {
        public int PartyID { get; set; }
        [Required]
        [StringLength(50)]
        public string PartysName { get; set; }
        [StringLength(5)]
        public string Abbreviation { get; set; }
        public bool Enabled { get; set; }
        [DisplayName("Does this party Fulfill Orders")]
        public bool IsForOrderFulfillment { get; set; }
        public int? NormalDeliveryDoWID { get; set; }
        [StringLength(255)]
        public string LoginUserID { get; set; }
        [Timestamp] 
        public byte[] RowVersion { get; set; }
        [DisplayName("Does this party Fulfill Orders")]
        [ForeignKey("NormalDeliveryDoWID ")]
        public Lookups.WeekDay NormalDeliveryDoW { get; set; }
    }

}
