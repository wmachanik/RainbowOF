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
        private ApplicationDbContext _context;
        private IDbContextTransaction dbTransaction = null;
        public ILoggerManager _logger { get; }
        // Generics Repos
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        /// Custom Repos
        private IItemRepository _itemRepository = null;
        private ISysPrefsRepository _sysPrefsRepository = null;
        private IWooSyncLogRepository _wooSyncLogRepository = null;
        private IItemCategoryLookupRepository _itemCategoryLookupRepository = null;
        private IItemAttributeLookupRepository _itemAttributeLookupRepository = null;
        private IItemAttributeVarietyLookupRepository _itemAttributeVarietyLookupRepository = null;
        // Internal vars
        private Dictionary<Guid, string> _listOfUoMSymbols;


        // Unit of Work Error handling
        private string _ErrorMessage = String.Empty;

        public Dictionary<Type, object> Repositories
        {
            get { return _repositories; }
            set { Repositories = value; }
        }
        public AppUnitOfWork(ApplicationDbContext context, ILoggerManager logger)
        {
            _context = context;
            _logger = logger;
        }
        public IAppRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (Repositories.ContainsKey(typeof(TEntity)))
            {
                return Repositories[typeof(TEntity)] as AppRepository<TEntity>;
            }
            IAppRepository<TEntity> sourceRepo = new AppRepository<TEntity>(_context, _logger, this);
            Repositories.Add(typeof(TEntity), sourceRepo);
            return sourceRepo;
        }
        public IItemRepository itemRepository()
        {
            if (_itemRepository == null)
                _itemRepository = new ItemRepository(_context, _logger, this);
            return _itemRepository;
        }
        public ISysPrefsRepository sysPrefsRepository()
        {
            if (_sysPrefsRepository == null)
                _sysPrefsRepository = new SysPrefsRepository(_context, _logger, this);
            return _sysPrefsRepository;
        }
        public IWooSyncLogRepository wooSyncLogRepository()
        {
            if (_wooSyncLogRepository == null)
                _wooSyncLogRepository = new WooSyncLogRepository(_context, _logger, this);
            return _wooSyncLogRepository;
        }
        public IItemCategoryLookupRepository itemCategoryLookupRepository()
        {
            if (_itemCategoryLookupRepository == null)
                _itemCategoryLookupRepository = new ItemCategoryLookupRepository(_context, _logger, this);
            return _itemCategoryLookupRepository;
        }
        public IItemAttributeLookupRepository itemAttributeLookupRepository()
        {
            if (_itemAttributeLookupRepository == null)
                _itemAttributeLookupRepository = new ItemAttributeLookupRepository(_context, _logger, this);
            return _itemAttributeLookupRepository;
        }
        public IItemAttributeVarietyLookupRepository itemAttributeVarietyLookupRepository()
        {
            if (_itemAttributeVarietyLookupRepository == null)
                _itemAttributeVarietyLookupRepository = new ItemAttributeVarietyLookupRepository(_context, _logger, this);
            return _itemAttributeVarietyLookupRepository;
        }
        public Dictionary<Guid, string> GetListOfUoMSymbols(bool IsForceReload = false)
        {
            if (IsForceReload)
            {
                _listOfUoMSymbols.Clear();
                _listOfUoMSymbols = null; // would prefer to dispose it but cannot see how
            }
            if (_listOfUoMSymbols == null)
            {
                IAppRepository<ItemUoM> appRepository = this.Repository<ItemUoM>();
                List<ItemUoM> _itemUoMs = appRepository.GetAll().ToList(); // (await _UoMRepository.GetAllAsync()).ToList();
                _listOfUoMSymbols = new();
                if (_itemUoMs != null)
                {
                    foreach (var item in _itemUoMs)
                    {
                        _listOfUoMSymbols.Add(item.ItemUoMId, item.UoMSymbol);
                    }
                }
            }
            return _listOfUoMSymbols;
            }


        public void BeginTransaction()
        {
            ClearErrorMessage();  // assume an error has been cleared
            if (dbTransaction == null)    // should be null - if not should we not throw an error?
                dbTransaction = _context.Database.BeginTransaction();
        }
        public int Complete()
        {
            int recsCommited = CONST_WASERROR;   
            try
            {
                _logger.LogDebug("Saving changes...");
                recsCommited = _context.SaveChanges(true);
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
                _logger.LogDebug("Saving changes (async).");
                recsCommited = await _context.SaveChangesAsync(true);
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
                _context.Database.RollbackTransaction();  // not sure if this will correct the fact that the context is in error
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
                    _context.Dispose();
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
        public void SetErrorMessage(string ErrorMessage)
        {
            _ErrorMessage = ErrorMessage;
        }
        public void ClearErrorMessage()
        {
            _ErrorMessage = string.Empty;
        }
        public string GetErrorMessage()
        {
            return _ErrorMessage;
        }
        public void LogAndSetErrorMessage(string ErrorMessage)
        {
            SetErrorMessage(ErrorMessage);
            _logger.LogError(ErrorMessage);
            RollbackTransaction();
        }

    }
}
