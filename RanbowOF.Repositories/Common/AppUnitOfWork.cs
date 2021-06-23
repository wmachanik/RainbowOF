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

namespace RainbowOF.Repositories.Common
{
    public class AppUnitOfWork : IAppUnitOfWork
    {
        // CONST
        public const int CONST_WASERROR = -1;
        public const int CONST_MAX_DETAIL_PAGES = 50;
        // generics
        private ApplicationDbContext _Context;
        private IDbContextTransaction dbTransaction = null;
        private ILoggerManager _Logger { get; }
        // Generics Repos
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        /// Custom Repos
        private IItemRepository _ItemRepository = null;
        private ISysPrefsRepository _SysPrefsRepository = null;
        private IWooSyncLogRepository _WooSyncLogRepository = null;
        private IItemCategoryLookupRepository _ItemCategoryLookupRepository = null;
        private IItemAttributeLookupRepository _ItemAttributeLookupRepository = null;
        private IItemAttributeVarietyLookupRepository _ItemAttributeVarietyLookupRepository = null;
        // Internal vars
        private Dictionary<Guid, string> _ListOfUoMSymbols;

        // Unit of Work Error handling
        private string _ErrorMessage = String.Empty;

        public Dictionary<Type, object> Repositories
        {
            get { return _repositories; }
            set { Repositories = value; }
        }
        public AppUnitOfWork(ApplicationDbContext context, ILoggerManager logger)
        {
            _Context = context;
            _Logger = logger;
        }
        public IAppRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (Repositories.ContainsKey(typeof(TEntity)))
            {
                return Repositories[typeof(TEntity)] as AppRepository<TEntity>;
            }
            IAppRepository<TEntity> sourceRepo = new AppRepository<TEntity>(_Context, _Logger, this);
            Repositories.Add(typeof(TEntity), sourceRepo);
            return sourceRepo;
        }
        public IItemRepository itemRepository()
        {
            if (_ItemRepository == null)
                _ItemRepository = new ItemRepository(_Context, _Logger, this);
            return _ItemRepository;
        }
        public ISysPrefsRepository sysPrefsRepository()
        {
            if (_SysPrefsRepository == null)
                _SysPrefsRepository = new SysPrefsRepository(_Context, _Logger, this);
            return _SysPrefsRepository;
        }
        public IWooSyncLogRepository wooSyncLogRepository()
        {
            if (_WooSyncLogRepository == null)
                _WooSyncLogRepository = new WooSyncLogRepository(_Context, _Logger, this);
            return _WooSyncLogRepository;
        }
        public IItemCategoryLookupRepository itemCategoryLookupRepository()
        {
            if (_ItemCategoryLookupRepository == null)
                _ItemCategoryLookupRepository = new ItemCategoryLookupRepository(_Context, _Logger, this);
            return _ItemCategoryLookupRepository;
        }
        public IItemAttributeLookupRepository itemAttributeLookupRepository()
        {
            if (_ItemAttributeLookupRepository == null)
                _ItemAttributeLookupRepository = new ItemAttributeLookupRepository(_Context, _Logger, this);
            return _ItemAttributeLookupRepository;
        }
        public IItemAttributeVarietyLookupRepository itemAttributeVarietyLookupRepository()
        {
            if (_ItemAttributeVarietyLookupRepository == null)
                _ItemAttributeVarietyLookupRepository = new ItemAttributeVarietyLookupRepository(_Context, _Logger, this);
            return _ItemAttributeVarietyLookupRepository;
        }
        public Dictionary<Guid, string> GetListOfUoMSymbols(bool IsForceReload = false)
        {
            if (IsForceReload)
            {
                _ListOfUoMSymbols.Clear();
                _ListOfUoMSymbols = null; // would prefer to dispose it but cannot see how
            }
            if (_ListOfUoMSymbols == null)
            {
                IAppRepository<ItemUoM> appRepository = this.Repository<ItemUoM>();
                var _itemUoMs = appRepository.GetAll(); // (await _UoMRepository.GetAllAsync()).ToList();
                _ListOfUoMSymbols = new();
                if (_itemUoMs != null)
                {
                    foreach (var item in _itemUoMs)
                    {
                        _ListOfUoMSymbols.Add(item.ItemUoMId, item.UoMSymbol);
                    }
                }
            }
            return _ListOfUoMSymbols;
        }
        public bool DBTransactionIsStillRunning()
        {
            return dbTransaction != null;
        }
        public void BeginTransaction()
        {
            ClearErrorMessage();  // assume an error has been cleared
            if (dbTransaction == null)    // should be null - if not should we not throw an error?
                dbTransaction = _Context.Database.BeginTransaction();
            else
                _Logger.LogDebug("Second transaction started before current transaction completed!"); 
        }
        public int Complete()
        {
            int recsCommited = CONST_WASERROR;
            try
            {
                _Logger.LogDebug("Saving changes...");
                recsCommited = _Context.SaveChanges(true);
                if (dbTransaction != null)
                {
                    dbTransaction.Commit();
                    dbTransaction.Dispose();
                    dbTransaction = null;
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
                _Logger.LogDebug("Committing changes (async).");
               recsCommited = await _Context.SaveChangesAsync(true);
                if (dbTransaction != null)
                {
                    dbTransaction.Commit();
                    dbTransaction.Dispose();
                    dbTransaction = null;
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
            if (dbTransaction != null)
            {
                dbTransaction.Rollback();
                dbTransaction.Dispose();
                dbTransaction = null;
                _Context.Database.RollbackTransaction();  // not sure if this will correct the fact that the context is in error
                //_context.Dispose();
                //_context = new ApplicationDbContext();
            }
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _Context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
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
            _Logger.LogError(sourceErrorMessage);
            RollbackTransaction();
        }

    }
}
