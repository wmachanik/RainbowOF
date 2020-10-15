using Microsoft.EntityFrameworkCore.Storage;
using RainbowOF.Data.SQL;
using RainbowOF.Tools;
using RanbowOF.Repositories.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RanbowOF.Repositories.Common
{
    public class AppUnitOfWork : IAppUnitOfWork
    {
        // generics
        private ApplicationDbContext _context;
        private IDbContextTransaction dbTransaction = null;
        public ILoggerManager _logger { get; }
        // Generics Repos
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        /// Custom Repos
        private ISysPrefsRepository _sysPrefsRepository = null;

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
        public ISysPrefsRepository sysPrefsRepository()
        {
            if (_sysPrefsRepository == null)
            {
                _sysPrefsRepository = new SysPrefsRepository(_context, _logger, this);
            }
            return _sysPrefsRepository;
        }

        public void BeginTransaction()
        {
            if (dbTransaction == null)
                dbTransaction = _context.Database.BeginTransaction();
        }
        public int Complete()
        {
            int recsCommited = 0;
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
                _logger.LogError($"ERROR!! Saving changes: {ex.Message}");
                if (dbTransaction != null)
                {
                    RollbackTransaction();
                }
            }
            return recsCommited;
        }
        public async Task<int> CompleteAsync()
        {
            int recsCommited = 0;
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
                _logger.LogError($"ERROR!! Saving changes (async): {ex.Message}");
                if (dbTransaction != null)
                {
                    RollbackTransaction();
                }
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
    }
}
