using NLog.Web.LayoutRenderers;
using RainbowOF.Repositories.Items;
using RainbowOF.Repositories.Logs;
using RainbowOF.Repositories.Lookups;
using RainbowOF.Repositories.System;
using System;
using System.Collections.Generic;
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
        ISysPrefsRepository sysPrefsRepository();
        IWooSyncLogRepository wooSyncLogRepository();
        IItemCategoryLookupRepository itemCategoryLookupRepository();
        IItemAttributeLookupRepository itemAttributeLookupRepository();
        IItemAttributeVarietyLookupRepository itemAttributeVarietyLookupRepository();
        #endregion
        #region Variables that load from the database, used mainly to prevent reloading.
        bool DBTransactionIsStillRunning();
        Dictionary<Guid, string> GetListOfCategories(bool IsForceReload = false);
        Dictionary<Guid, string> GetListOfUoMSymbols(bool IsForceReload = false);
        Dictionary<Guid, string> GetListOfAttributes(bool IsForceReload = false);
        Dictionary<Guid, string> GetListOfAttributeVarieties(Guid parentAttributeLookupId, bool IsForceReload = false);
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
