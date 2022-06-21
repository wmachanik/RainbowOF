using Microsoft.EntityFrameworkCore.Storage;
using RainbowOF.Data.SQL;
using RainbowOF.Tools;
using RainbowOF.Repositories.Logs;
using RainbowOF.Repositories.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RainbowOF.Repositories.Items;
using RainbowOF.Repositories.Lookups;
using RainbowOF.Models.Items;
using System.Linq;
using RainbowOF.Models.Lookups;
using System.Linq.Expressions;

namespace RainbowOF.Repositories.Common
{
    /// <summary>
    /// This part of the partial class is all the general database and app related routines and variables
    /// </summary>
    /// Other partial classes:
    /// AppUnitOfWorkRepos - Generic and Custom Repos
    /// AppUnitOfWorkLists - All lists that are used for select lists combo boxes
    public partial class UnitOfWork : IUnitOfWork
    {
        #region Public constants
        public const int CONST_WASERROR = -1;
        public const int CONST_INVALID_ID = 0;
        public const int CONST_MAX_DETAIL_PAGES = 50;
        #endregion
        #region Generic privates
        // generics
        private ApplicationDbContext appContext;
        private IDbContextTransaction appDbTransaction = null;
        private ILoggerManager appLoggerManager { get; }
        #endregion
        #region Unit of Work Error handling
        private string _ErrorMessage = String.Empty;
        #endregion
        #region Initialisation
        public UnitOfWork(ApplicationDbContext context, ILoggerManager logger)
        {
            appContext = context;
            appLoggerManager = logger;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("AppUnitOfWork initialised.");
        }
        #endregion
        #region Centralised Context Handling
        public void BeginTransaction()
        {
            ClearErrorMessage();  // assume an error has been cleared
            if (appDbTransaction == null)    // should be null - if not should we not throw an error?
                appDbTransaction = appContext.Database.BeginTransaction();
            else
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
        }
        public int Complete()
        {
            int recsCommited = CONST_WASERROR;
            try
            {
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Saving changes...");
                recsCommited = appContext.SaveChanges(true);
                if (appDbTransaction != null)
                {
                    appDbTransaction.Commit();
                    appDbTransaction.Dispose();
                    appDbTransaction = null;
                }
            }
            catch (Exception ex)
            {
                LogAndSetErrorMessage($"ERROR committing changes:  {ex.Message} - Inner Exception {ex.InnerException}");
            }
            return recsCommited;
        }
        public async Task<int> CompleteAsync()
        {
            int recsCommited = CONST_WASERROR;   // -1 means error only returned if there is one
            try
            {
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Committing changes (async).");
                recsCommited = await appContext.SaveChangesAsync(true);
                if (appDbTransaction != null)
                {
                    appDbTransaction.Commit();
                    appDbTransaction.Dispose();
                    appDbTransaction = null;
                }
            }
            catch (Exception ex)
            {
                LogAndSetErrorMessage($"ERROR committing changes (async):  {ex.Message} - Inner Exception {ex.InnerException}");
            }
            return recsCommited;
        }
        public void RollbackTransaction()
        {
            if (appDbTransaction != null)
            {
                appDbTransaction.Rollback();
                appDbTransaction.Dispose();
                appDbTransaction = null;
                appContext.Database.RollbackTransaction();  // not sure if this will correct the fact that the context is in error
                //appContext.Dispose();
                //appContext = new ApplicationDbContext();
            }
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    appContext.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        #region Error Messages
        public bool IsInErrorState()
        {
            return _ErrorMessage != string.Empty;
        }
        public void SetErrorMessage(string sourceErrorMessage)
        {
            _ErrorMessage = sourceErrorMessage;
        }
        public void ClearErrorMessage()
        {
            _ErrorMessage = string.Empty;
        }
        public string GetErrorMessage()
        {
            return _ErrorMessage;
        }
        public void LogAndSetErrorMessage(string sourceErrorMessage)
        {
            SetErrorMessage(sourceErrorMessage);
            appLoggerManager.LogError(sourceErrorMessage);
            RollbackTransaction();
        }
        #endregion

    }
}
