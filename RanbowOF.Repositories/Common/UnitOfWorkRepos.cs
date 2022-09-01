using RainbowOF.Repositories.Items;
using RainbowOF.Repositories.Logs;
using RainbowOF.Repositories.Lookups;
using RainbowOF.Repositories.System;
using System;
using System.Collections.Generic;

namespace RainbowOF.Repositories.Common
{
    /// <summary>
    /// This part of the partial class is where all the Repositories variables and routines are found
    /// </summary>
    /// AppUnitOfWork      - the general database and app related routines and variables
    /// AppUnitOfWorkLists - All lists that are used for select lists combo boxes
    public partial class UnitOfWork : IUnitOfWork
    {

        #region Generic and Custom Repos
        // Generics Repos
        private Dictionary<Type, object> _repositories = new();
        // Custom Repos
        #endregion
        #region Custom Repos
        private IItemRepository _itemRepository = null;
        private IItemCategoryRepository _itemCategoryRepository = null;
        private IItemAttributeRepository _itemAttributeRepository = null;
        private ISysPrefsRepository _sysPrefsRepository = null;
        private IWooSyncLogRepository _wooSyncLogRepository = null;
        private IItemCategoryLookupRepository _itemCategoryLookupRepository = null;
        private IItemAttributeLookupRepository _itemAttributeLookupRepository = null;
        private IItemAttributeVarietyLookupRepository _itemAttributeVarietyLookupRepository = null;
        #endregion
        #region Public's of the unit of work repos
        public Dictionary<Type, object> Repositories
        {
            get { return _repositories; }
            set { _repositories = value; }
        }
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (Repositories.ContainsKey(typeof(TEntity)))
            {
                return Repositories[typeof(TEntity)] as Repository<TEntity>;
            }
            IRepository<TEntity> sourceRepo = new Repository<TEntity>(appDbContext, AppLoggerManager, this);
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Repository type: {typeof(TEntity).Name} retrieved / initialised.");
            Repositories.Add(typeof(TEntity), sourceRepo);
            return sourceRepo;
        }
        public IItemRepository ItemRepository
        {
            get
            {
                if (_itemRepository == null)
                    _itemRepository = new ItemRepository(appDbContext, AppLoggerManager, this);
                return _itemRepository;
            }
        }
        public IItemCategoryRepository ItemCategoryRepo
        {
            get
            {
                if (_itemCategoryRepository == null)
                    _itemCategoryRepository = new ItemCategoryRepository(appDbContext, AppLoggerManager, this);    ///////add using
                return _itemCategoryRepository;
            }
        }
        public IItemAttributeRepository ItemAttributeRepo
        {
            get
            {
                if (_itemAttributeRepository == null)
                    _itemAttributeRepository = new ItemAttributeRepository(appDbContext, AppLoggerManager, this);    ///////add using
                return _itemAttributeRepository;
            }
        }
        public ISysPrefsRepository SysPrefsRepository
        {
            get
            {
                if (_sysPrefsRepository == null)
                    _sysPrefsRepository = new SysPrefsRepository(appDbContext, AppLoggerManager, this);
                return _sysPrefsRepository;
            }
        }
        public IWooSyncLogRepository WooSyncLogRepository
        {
            get
            {
                if (_wooSyncLogRepository == null)
                    _wooSyncLogRepository = new WooSyncLogRepository(appDbContext, AppLoggerManager, this);
                return _wooSyncLogRepository;
            }
        }
        public IItemCategoryLookupRepository ItemCategoryLookupRepository
        {
            get
            {
                if (_itemCategoryLookupRepository == null)
                    _itemCategoryLookupRepository = new ItemCategoryLookupRepository(appDbContext, AppLoggerManager, this);
                return _itemCategoryLookupRepository;
            }
        }
        public IItemAttributeLookupRepository ItemAttributeLookupRepository
        {
            get
            {
                if (_itemAttributeLookupRepository == null)
                    _itemAttributeLookupRepository = new ItemAttributeLookupRepository(appDbContext, AppLoggerManager, this);
                return _itemAttributeLookupRepository;
            }
        }
        public IItemAttributeVarietyLookupRepository ItemAttributeVarietyLookupRepository
        {
            get
            {
                if (_itemAttributeVarietyLookupRepository == null)
                    _itemAttributeVarietyLookupRepository = new ItemAttributeVarietyLookupRepository(appDbContext, AppLoggerManager, this);
                return _itemAttributeVarietyLookupRepository;
            }
        }
        #endregion

    }
}
