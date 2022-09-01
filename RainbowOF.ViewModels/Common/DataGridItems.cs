using System.Collections.Generic;

namespace RainbowOF.ViewModels.Common
{
    public class DataGridItems
        <TEntity> where TEntity : class
    {
        public int TotalRecordCount { get; set; }
        public IEnumerable<TEntity> Entities { get; set; }
    }
}
