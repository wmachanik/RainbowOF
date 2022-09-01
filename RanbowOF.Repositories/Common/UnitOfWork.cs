using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RainbowOF.Data.SQL;
using RainbowOF.Tools;
using System;
//using System.Data.Entity;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    /// <summary>
    /// This part of the partial class is all the general database and app related routines and variables
    /// </summary>
    /// Other partial classes:
    /// AppUnitOfWorkRepos - Generic and Custom Repos
    /// AppUnitOfWorkLists - All lists that are used for select lists combo boxes
    public partial class UnitOfWork : IUnitOfWork, IDisposable
    {
        #region Public constants
        public const int CONST_WASERROR = -1;
        public const int CONST_INVALID_ID = 0;
        public const int CONST_MAX_DETAIL_PAGES = 50;
        #endregion
        #region Privates  and internals
        // generics
        private IDbContextFactory<ApplicationDbContext> appDbContext { get; set; }

        //private ApplicationDbContext appDbContext { get; set; } 

        ///  DbContextOptionsBuilder.EnableSensitiveDataLogging to see conflicts
        ///     // disable tracking since this is mainly a read once site, with saving time unknown
        //---- Only use if needed                options.EnableSensitiveDataLogging(true);
        //                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        //private IDbContextTransaction appDbTransaction { get; set; } = null;
        public ILoggerManager AppLoggerManager { get; }
        #endregion
        #region Unit of Work Error handling
        private string errorMessage { get; set; } = String.Empty;
        //private ApplicationDbContext _appDbContext; // = null;
        //public ApplicationDbContext AppDbContext
        //{
        //    get { return _appDbContext; }
        //    set { _appDbContext = value; }
        //}

        #endregion
        #region Initialisation
        public UnitOfWork(IDbContextFactory<ApplicationDbContext> context, ILoggerManager logger)
        {
            appDbContext = context;
            // appDbContext.ChangeTracker.AutoDetectChangesEnabled = false;  //--> https://stackoverflow.com/questions/18925111/turn-off-ef-change-tracking-for-any-instance-of-the-context Setting AutoDetectChangesEnabled to false...It just disables the automatic call of DetectChanges stackoverflow.com/questions/16863382/… – 
            AppLoggerManager = logger;
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("AppUnitOfWork initialised.");
        }
        #endregion
        #region Centralised Context Handling

        #region oldStuff
        //==> we do not need to take explicit control on the DB transaction, because EF core does that - see: https://mehdi.me/ambient-dbcontext-in-ef6/
        //public void BeginTransaction()
        //{
        //    ClearErrorMessage();  // assume an error has been cleared
        //    //if (appDbTransaction == null)    // should be null - if not should we not throw an error?
        //    //    appDbTransaction = appDbContext.Database.BeginTransaction();
        //    //else
        //    //    if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("Second transaction started before current transaction completed!");
        //}
        //public ApplicationDbContext GetAppDbContext() { get => appDbContext; }
        //public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class 
        //    => AppDbContext.Set<TEntity>();

        /// => moved to a factory so don't do this here
        //public int Complete()
        //{
        //    int _result; // = CONST_WASERROR;
        //    try
        //    {
        //        if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("Saving changes...");
        //        int recsCommited = _appDbContext.SaveChanges(true);
        //        _appDbContext.ChangeTracker.Clear();   // without this getting a lot of attach errors - took from: https://stackoverflow.com/questions/36856073/the-instance-of-entity-type-cannot-be-tracked-because-another-instance-of-this-t
        //        //if (appDbTransaction != null)
        //        //{
        //        //    appDbTransaction.Commit();
        //        //    appDbTransaction.Dispose();
        //        //    appDbTransaction = null;
        //        //}
        //        _result = recsCommited;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogAndSetErrorMessage($"ERROR committing changes:  {ex.Message} - Inner Exception {ex.InnerException}");
        //        _result = CONST_WASERROR;   // -1 means error only returned if there is one
        //    }
        //    return _result;
        //}
        //public async Task<int> CompleteAsync()
        //{
        //    int _result; // = CONST_WASERROR; -> new C# not needed
        //    try
        //    {
        //        if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("Committing changes (async).");
        //        int recsCommited = await _appDbContext.SaveChangesAsync(true);
        //        _appDbContext.ChangeTracker.Clear();   // without this getting a lot of attach errors - took from: https://stackoverflow.com/questions/36856073/the-instance-of-entity-type-cannot-be-tracked-because-another-instance-of-this-t
        //        //if (appDbTransaction != null)
        //        //{
        //        //    appDbTransaction.Commit();
        //        //    appDbTransaction.Dispose();
        //        //    appDbTransaction = null;
        //        //}
        //        _result = recsCommited;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogAndSetErrorMessage($"ERROR committing changes (async):  {ex.Message} - Inner Exception {ex.InnerException}");
        //        _result = CONST_WASERROR;   // -1 means error only returned if there is one
        //    }
        //    return _result;
        //  }
        //public void RollbackTransaction()
        //{
        //    //if (appDbTransaction != null)
        //    //{
        //    //    appDbTransaction.Rollback();
        //    //    appDbTransaction.Dispose();
        //    //    appDbTransaction = null;
        //    //    appDbContext.Database.RollbackTransaction();  // not sure if this will correct the fact that the context is in error
        //    //    //AppContext.Dispose();
        //    //    //AppContext = new ApplicationDbContext();
        //    //}
        //}
        #endregion  

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // using dbContextFactoryu so create new each time.
                    //                    _appDbContext.Dispose();
                }
            }
            this._disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        #region Error Messages
        public bool IsInErrorState()
            => errorMessage != string.Empty;

        public void SetErrorMessage(string sourceErrorMessage)
            => errorMessage = sourceErrorMessage;
        public void ClearErrorMessage()
            => errorMessage = string.Empty;
        public string GetErrorMessage()
            => errorMessage;

        public void LogAndSetErrorMessage(string sourceErrorMessage)
        {
            SetErrorMessage(sourceErrorMessage);
            AppLoggerManager.LogError(sourceErrorMessage);
            //RollbackTransaction();
        }
        #endregion

    }
}
