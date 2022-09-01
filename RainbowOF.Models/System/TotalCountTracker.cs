using System;
using System.ComponentModel.DataAnnotations;

namespace RainbowOF.Models.System
{
    public class TotalCountTracker
    {
        public int TotalCountTrackerId { get; set; }
        public DateTime CountDate { get; set; }
        public int TotalCount { get; set; }
        public string Comments { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

    }
}
