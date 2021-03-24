using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Lookups
{
    public class ItemCategoryLookupRepository : AppRepository<ItemCategoryLookup>, IItemCategoryLookupRepository
    {
        private ApplicationDbContext _context = null;
        private ILoggerManager _logger { get; set; }
        private IAppUnitOfWork _appUnitOfWork { get; set; }
        public ItemCategoryLookupRepository(ApplicationDbContext dbContext, ILoggerManager logger, IAppUnitOfWork appUnitOfWork) : base(dbContext, logger, appUnitOfWork)
        {
            _context = dbContext;
            _logger = logger;
            _appUnitOfWork = appUnitOfWork;
        }

        public async Task<List<ItemCategoryLookup>> GetAllEagerLoadingAsync()
        {
            List<ItemCategoryLookup> _items = null;
            DbSet<ItemCategoryLookup> _table = _context.Set<ItemCategoryLookup>();

            try
            {
                _logger.LogDebug($"Getttign all records with eager loading of ItemCategoryLookup");
                _items = await _table.Include(icl => icl.ParentCategory).ToListAsync();
            }
            catch (Exception ex)
            {
                _appUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all recods from ItemCategoryLookupRepository: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _items;
        }

    }

}
