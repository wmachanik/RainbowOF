using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Items;
using RainbowOF.Repositories.Logs;
using RainbowOF.Repositories.Lookups;
using RainbowOF.Repositories.System;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public interface IUnitOfWork : IDisposable
    {
        #region Generics
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        #endregion
        #region Custom repositories - implementation in partial class AppUnitOfWorkRepos
        IItemRepository ItemRepository { get; }
        IItemCategoryRepository ItemCategoryRepo { get; }
        IItemAttributeRepository ItemAttributeRepo { get; }
        ISysPrefsRepository SysPrefsRepository { get; }
        IWooSyncLogRepository WooSyncLogRepository { get; }
        IItemCategoryLookupRepository ItemCategoryLookupRepository { get; }
        IItemAttributeLookupRepository ItemAttributeLookupRepository { get; }
        IItemAttributeVarietyLookupRepository ItemAttributeVarietyLookupRepository { get; }
        #endregion
        #region Variables that can be used outside the UoW
        bool DBTransactionIsStillRunning();
        Dictionary<Type, object> LookupLists { get; set; }
        #endregion
        #region Variables that load from the database, used mainly to prevent reloading. Implementation is partial class AppUnitOfWorkLists
        List<TLookupItem> GetListOf<TLookupItem>(bool IsForceReload = false, Func<TLookupItem, object> orderByExpression = null) where TLookupItem : class;
        Task<List<TLookupItem>> GetListOfAsync<TLookupItem>(bool IsForceReload = false, Expression<Func<TLookupItem, object>> orderByExpression = null) where TLookupItem : class;
        public List<Item> GetListOfSimilarItems(Guid sourceItemId, Guid? sourceItemPrimaryCategoryId, bool IsForceReload = false);
        Dictionary<Guid, string> GetListOfUoMSymbols(bool IsForceReload = false);
        List<ItemCategoryLookup> GetListOfAnItemsCategories(Guid sourceItemId, bool IsForceReload = false);
        //Dictionary<Guid, string> GetListOfCategories(bool IsForceReload = false);
        Dictionary<Guid, string> GetListOfAttributes(bool IsForceReload = false);
        List<ItemAttributeVarietyLookup> GetListOfAttributeVarieties(Guid parentAttributeLookupId, bool IsForceReload = false);
        List<ItemAttribute> GetListOfAnItemsVariableAttributes(Guid sourceItemId, bool IsForceReload = false);
        List<ItemAttributeVariety> GetListOfAnItemsAttributeVarieties(Guid sourceItemId, Guid sourceAssoicatedAttributeId, bool IsForceReload = false);
        Task<List<ItemAttributeVariety>> GetListOfItemAttributeVarietiesAsync(Guid sourceItemId, Guid sourceAttributeLookupId, bool IsForceReload = false);
        //        Task<List<ItemAttributeVariety>> GetListOfItemAttributeVarietiesAsync(Guid sourceAssoicatedItemAttributeId, bool IsForceReload = false);
        #endregion
        #region Centralised Context Handling
        // Don't expose the factory as they must inject or use dir3ectly
        //IDbContextFactory<ApplicationDbContext> AppDbContext { get; }
        //DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class;
        //void BeginTransaction();
        //int Complete();
        //Task<int> CompleteAsync();
        //void RollbackTransaction();
        #endregion
        #region Error Messages
        bool IsInErrorState();
        void SetErrorMessage(string sourceErrorMessage);
        void ClearErrorMessage();
        string GetErrorMessage();
        void LogAndSetErrorMessage(string sourceErrorMessage);
        #endregion
    }
}
