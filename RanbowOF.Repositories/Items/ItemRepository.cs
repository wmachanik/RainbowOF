using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Items;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public class ItemRepository : AppRepository<Item>, IItemRepository
    {

        private ApplicationDbContext _context = null;
        private ILoggerManager _logger { get; set; }
        private IAppUnitOfWork _appUnitOfWork { get; set; }
        public ItemRepository(ApplicationDbContext dbContext, ILoggerManager logger, IAppUnitOfWork appUnitOfWork) : base(dbContext, logger, appUnitOfWork)
        {
            _context = dbContext;
            _logger = logger;
            _appUnitOfWork = appUnitOfWork;
        }

        public async Task<Item> FindFirstEagerLoadingItemAsync(Expression<Func<Item, bool>> predicate)
        {
            Item _item = null;
            DbSet<Item>  _table = _context.Set<Item>();

            try
            {
                _logger.LogDebug($"Finding first whole item using predicate: {predicate.ToString()}");
                _item = await _table
                    .Include(item => item.ItemAttributes)
                    .Include(item => item.ItemAttributeVarieties)
                    .Include(item => item.ItemCategories)
                    .FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                _appUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _item;
        }

        public async Task<Item> FindFirstItemBySKU(string pSKU)
        {
            return await FindFirstAsync(i => i.SKU == pSKU);
            
        }

        public async Task<int> AddItem(Item newItem)
        {
            if (!String.IsNullOrEmpty(newItem.SKU))
            {
                if (await FindFirstItemBySKU(newItem.SKU) != null)
                {
                    _appUnitOfWork.LogAndSetErrorMessage($"ERROR adding Item:  {newItem.ToString()} - duplicate SKU found in database");
                    return AppUnitOfWork.CONST_WASERROR;
                }
            }
            // if we get here then the SKU is null or does not exists
            return await AddAsync(newItem);
        }
    }
}
