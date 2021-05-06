using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.ViewModels.Common
{
    public class SortParam
    {
        public string FieldName { get; set; }
        public Blazorise.SortDirection Direction { get; set; }
    }
    public class FilterParam
    {
        public string FieldName { get; set; }
        public string FilterBy { get; set; }
    }
    public class DataGridParameters
    {
        public int CurrentPage { get; set; } = 0;
        public int PageSize { get; set; }
        public string CustomFilter { get; set; } = string.Empty;
        public List<SortParam> SortParams { get; set; } = null;
        public List<FilterParam> FilterParams { get; set; } = null;

    }

}
