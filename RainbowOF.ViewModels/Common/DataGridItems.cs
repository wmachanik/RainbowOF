using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.ViewModels.Common
{
    public class DataGridItems
        <TEntity> where TEntity : class
    {
        public int TotalRecordCount { get; set; }
        public IEnumerable<TEntity> Entities { get; set; }
    }
}
