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
    /// This part of the partial class is where all the Repositories variables and routines are found
    /// </summary>
    /// AppUnitOfWork      - the general database and app related routines and variables
    /// AppUnitOfWorkLists - All lists that are used for select lists combo boxes
    public partial class UnitOfWork : IUnitOfWork
    {

        #region Generic and Custom Repos
        // Generics Repos
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        // Custom Repos
        #endregion
        #region Custom Repos
        private IItemRepository _ItemRepository = null;
        private ISysPrefsRepository _SysPrefsRepository = null;
        private IWooSyncLogRepository _WooSyncLogRepository = null;
        private IItemCategoryLookupRepository _ItemCategoryLookupRepository = null;
        private IItemAttributeLookupRepository _ItemAttributeLookupRepository = null;
        private IItemAttributeVarietyLookupRepository _ItemAttributeVarietyLookupRepository = null;
        #endregion
        #region Public's of the unit of work repos
        public Dictionary<Type, object> Repositories
        {
            get { return _repositories; }
            set { Repositories = value; }
        }
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (Repositories.ContainsKey(typeof(TEntity)))
            {
                return Repositories[typeof(TEntity)] as Repository<TEntity>;
            }
            IRepository<TEntity> sourceRepo = new Repository<TEntity>(appContext, appLoggerManager, this);
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Repository type: {typeof(TEntity).Name} retrieved / initialised.");
            Repositories.Add(typeof(TEntity), sourceRepo);
            return sourceRepo;
        }
        public IItemRepository itemRepository
        { 
            get
            {
                if (_ItemRepository == null)
                    _ItemRepository = new ItemRepository(appContext, appLoggerManager, this);
                return _ItemRepository;
            }
        }
        public ISysPrefsRepository sysPrefsRepository
        {
            get
            {
                if (_SysPrefsRepository == null)
                    _SysPrefsRepository = new SysPrefsRepository(appContext, appLoggerManager, this);
                return _SysPrefsRepository;
            }
        }
        public IWooSyncLogRepository wooSyncLogRepository
        {
            get
            {
                if (_WooSyncLogRepository == null)
                    _WooSyncLogRepository = new WooSyncLogRepository(appContext, appLoggerManager, this);
                return _WooSyncLogRepository;
            }
        }
        public IItemCategoryLookupRepository itemCategoryLookupRepository
        {
            get
            {
                if (_ItemCategoryLookupRepository == null)
                    _ItemCategoryLookupRepository = new ItemCategoryLookupRepository(appContext, appLoggerManager, this);
                return _ItemCategoryLookupRepository;
            }
        }
        public IItemAttributeLookupRepository itemAttributeLookupRepository
        {
            get
            {
                if (_ItemAttributeLookupRepository == null)
                    _ItemAttributeLookupRepository = new ItemAttributeLookupRepository(appContext, appLoggerManager, this);
                return _ItemAttributeLookupRepository;
            }
        }
        public IItemAttributeVarietyLookupRepository itemAttributeVarietyLookupRepository
        {
            get
            {
                if (_ItemAttributeVarietyLookupRepository == null)
                    _ItemAttributeVarietyLookupRepository = new ItemAttributeVarietyLookupRepository(appContext, appLoggerManager, this);
                return _ItemAttributeVarietyLookupRepository;
            }
        }
        #endregion

    }
}
