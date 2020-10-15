using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RainbowOF.Models.System
{
    public class SysPrefs 
    {
        public int SysPrefsId { get; set; }
        [DisplayName("Last Date Recurring Orders Processed")]
        public DateTime? LastReccurringDate { get; set; }
        [DisplayName("Recurring Orders Enabled")]
        [DefaultValue(true)]
        public bool DoReccuringOrders { get; set; }
        [DisplayName("Date the prep dates were last calculated")]
        public DateTime? DateLastPrepDateCalcd { get; set; }
        [DisplayName("Number of days to use in caclulating a reminder is required")]
        [Range(1, 14, ErrorMessage = "Must be a number from 1-14")]
        public int ReminderDaysNumber { get; set; }
        [DisplayName("ID of the Group Item")]
        public int? GroupItemTypeID { get; set; }
        [DisplayName("ID of the Primary Delivery Person")]
        public int? DefaultDeliveryPersonID { get; set; }
        [DisplayName("Image folder path")]
        [StringLength(250)]
        public string ImageFolderPath { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
