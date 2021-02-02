using System;

namespace RainbowOF.FrontEnd.Models.Classes
{
    // common area for variables used in class
    public class ImportCounters
    {
        public int TotalUpdated { get; set; }
        public int TotalAdded { get; set; }
        public int TotalImported { get; set; }
        public int PercentOfRecsImported { get; set; }
        public int MaxRecs { get; set; }

        public ImportCounters()
        {
            Reset();
        }
        public void Reset()
        {
            TotalAdded = 0;
            TotalUpdated = 0;
            TotalImported = 0;
            PercentOfRecsImported = 0;  // progress bar uses percentage not tota rec imports so need to convert
            MaxRecs = 0;
        }
        public int CalcPercentage(double TotalSoFar)
        {
            return Convert.ToInt32(Math.Round((TotalSoFar / (double)MaxRecs) * 100, 0));
        }
    }
}


