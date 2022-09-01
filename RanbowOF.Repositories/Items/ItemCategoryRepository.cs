using RainbowOF.Models.Items;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RainbowOF.Data.SQL;

namespace RainbowOF.Repositories.Items
{
    public class ItemCategoryRepository : Repository<ItemCategory>, IItemCategoryRepository
    {
        #region privates
        // not needed as these are now internals in the Generic repot
        //private IDbContextFactory<ApplicationDbContext> appDbContext { get; set; }
        #endregion
        #region Initialisation
        public ItemCategoryRepository(IDbContextFactory<ApplicationDbContext> sourceDbContext, ILoggerManager logger, IUnitOfWork AppUnitOfWork) : base(sourceDbContext, logger, AppUnitOfWork)
        {
            if (logger.IsDebugEnabled()) logger.LogDebug("ItemRepository initialised.");
        }
        #endregion

        #region DataGrid Handling

        public List<ItemCategory> GetAllAnItemsCategories(Guid parentItemId)
        {
            try
            {
                using var context = AppDbContext.CreateDbContext();
                DbSet<ItemCategory> _table = context.Set<ItemCategory>();
                AppUnitOfWork.ClearErrorMessage(); //-- clear any error message
                var _query = _table
                    .Include(ic => ic.ItemCategoryDetail)
                    .Include(ic => ic.ItemUoMBase)
                    .Where(ic => ic.ItemId == parentItemId);

                if (AppLoggerManager.IsDebugEnabled())
                {
                    var _sql = _query.ToQueryString();
                    AppLoggerManager.LogDebug($"Get all an Items Categories SQL Query is: {_sql}");
                }
                var _result = _query.ToList();

                return _result;
            }
            catch (Exception)
            {
                return null;
                throw;
            }
        }
        #endregion
    }
}
