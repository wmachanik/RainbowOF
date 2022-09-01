using System;
using System.Linq.Expressions;

namespace RainbowOF.ViewModels.Common
{
    public class OrderByParameter<TEntity> where TEntity : class
    {
        public bool IsAscending { get; set; }
        public Expression<Func<TEntity, object>> OrderByExperssion { get; set; }
    }
}
