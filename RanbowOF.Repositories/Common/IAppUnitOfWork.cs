using NLog.Web.LayoutRenderers;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Items;
using RainbowOF.Repositories.Logs;
using RainbowOF.Repositories.Lookups;
using RainbowOF.Repositories.System;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public interface IAppUnitOfWork : IDisposable
    {

        #region Generics
        IAppRepository<TEntity> Repository<TEntity>() where TEntity : class;
        #endregion
        #region Custom repositories
        IItemRepository itemRepository();
        IItemAttributeRepository itemAttributeRepository();
        ISysPrefsRepository sysPrefsRepository();
        IWooSyncLogRepository wooSyncLogRepository();
        IItemCategoryLookupRepository itemCategoryLookupRepository();
        IItemAttributeLookupRepository itemAttributeLookupRepository();
        IItemAttributeVarietyLookupRepository itemAttributeVarietyLookupRepository();
        #endregion
        #region Variables that can be used outside the UoW
        bool DBTransactionIsStillRunning();
        Dictionary<Type, object> LookupLists { get; set; }
        #endregion
        #region Variables that load from the database, used mainly to prevent reloading.
        List<TLookupItem> GetListOf<TLookupItem>(bool IsForceReload = false, Func<TLookupItem, object> orderByExpression = null) where TLookupItem : class;
        Task<List<TLookupItem>> GetListOfAsync<TLookupItem>(bool IsForceReload = false, Expression<Func<TLookupItem, object>> orderByExpression = null) where TLookupItem : class;

        Dictionary<Guid, string> GetListOfUoMSymbols(bool IsForceReload = false);
        //Dictionary<Guid, string> GetListOfCategories(bool IsForceReload = false);
        //Dictionary<Guid, string> GetListOfAttributes(bool IsForceReload = false);
        List<ItemAttributeVarietyLookup> GetListOfAttributeVarieties(Guid parentAttributeLookupId, bool IsForceReload = false);
        List<ItemAttributeVariety> GetListOfItemAttributeVarieties(Guid sourceAssoicatedItemAttributeId, bool IsForceReload = false);
        Task<List<ItemAttributeVariety>> GetListOfItemAttributeVarietiesAsync(Guid sourceAssoicatedItemAttributeId, bool IsForceReload = false);
        #endregion
        #region Centralised Context Handling
        void BeginTransaction();
        int Complete();
        Task<int> CompleteAsync();
        void RollbackTransaction();
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
