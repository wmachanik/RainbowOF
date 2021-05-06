using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.ViewModels.Common
{
    public class OrderByParameter<TEntity> where TEntity : class
    {
        public bool IsAscending { get; set; }
        public Expression<Func<TEntity, object>> OrderByExperssion { get; set; }
    }
}
