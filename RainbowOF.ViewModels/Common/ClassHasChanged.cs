using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.ViewModels.Common
{
    public class ClassHasChanged<TEntity> where TEntity : class
    {
        public bool HasChanged { get; set; } = false;
        public TEntity Entity { get; set; }
    }
}
