using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RainbowOF.Models.System
{
    public class ClosureDate
    {
        public int ClosureDateId { get; set; }
        [NotNull]
        [StringLength(50)]
        [Display(Name = "Event Name")]
        public string EventName { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Closed")]
        public DateTime DateClosed { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Reopen")]
        public DateTime DateReopen { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Next Preperation DAte")]
        public DateTime? NextPrepDate { get; set; }
        public string Notes { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; }
    }
}
