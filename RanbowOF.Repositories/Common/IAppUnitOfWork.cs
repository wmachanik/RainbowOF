using NLog.Web.LayoutRenderers;
using RanbowOF.Repositories.Logs;
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
        // custom repositories
        public ISysPrefsRepository sysPrefsRepository();
        public IWooSyncLogRepository wooSyncLogRepository();
        // Centralised Context Handling
        void BeginTransaction();
        int Complete();
        Task<int> CompleteAsync();
        void RollbackTransaction();
        // "Error Messages"
        bool IsInErrorState();
        void SetErrorMessage(string ErrorMessage);
        void ClearErrorMessage();
        string GetErrorMessage();
        void LogAndSetErrorMessage(string ErrorMessage);

    }
}
