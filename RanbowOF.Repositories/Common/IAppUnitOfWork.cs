using NLog.Web.LayoutRenderers;
using RanbowOF.Repositories.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RanbowOF.Repositories.Common
{
    public interface IAppUnitOfWork : IDisposable
    {
        // generics
        IAppRepository<TEntity> Repository<TEntity>() where TEntity : class;
        public ISysPrefsRepository sysPrefsRepository();
        // custom reps
        void BeginTransaction();
        int Complete();
        Task<int> CompleteAsync();
        void RollbackTransaction();
    }
}
