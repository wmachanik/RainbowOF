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
        // generics
        IAppRepository<TEntity> Repository<TEntity>() where TEntity : class;
        // custom repositories
        public IItemRepository itemRepository();
        public ISysPrefsRepository sysPrefsRepository();
        public IWooSyncLogRepository wooSyncLogRepository();
        public IItemCategoryLookupRepository itemCategoryLookupRepository();
        public IItemAttributeLookupRepository itemAttributeLookupRepository();
        public IItemAttributeVarietyLookupRepository itemAttributeVarietyLookupRepository();
        // Variables that load from the database, used mainly to prevent reloading.
        public Dictionary<Guid, string> GetListOfUoMSymbols(bool IsForceReload = false);
        // Centralised Context Handling
        void BeginTransaction();
        int Complete();
        Task<int> CompleteAsync();
        void RollbackTransaction();
        // "Error Messages"
        bool IsInErrorState();
        void SetErrorMessage(string ErrorMessage);
        void ClearErrorMessage();
        string GetErrorMessage();
        void LogAndSetErrorMessage(string ErrorMessage);

    }
}
