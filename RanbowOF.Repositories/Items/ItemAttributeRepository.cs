using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Items;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    internal class ItemAttributeRepository : AppRepository<ItemAttribute>, IItemAttributeRepository
    {
        #region Privates
        #endregion
        #region Passed in Items
        private ApplicationDbContext _Context = null;
        private ILoggerManager _Logger { get; set; }
        private IAppUnitOfWork _AppUnitOfWork { get; set; }
        #endregion
        #region Initialisation
        public ItemAttributeRepository(ApplicationDbContext dbContext, ILoggerManager logger, IAppUnitOfWork appUnitOfWork) : base(dbContext, logger, appUnitOfWork)
        {
            _Context = dbContext;
            _Logger = logger;
            _AppUnitOfWork = appUnitOfWork;
            _Logger.LogDebug("ItemAttributeRepository initialised...");
        }
        #endregion
        #region Routines specific to this Interface
        /// <summary>
        /// Return a list of variants associated to the source attribute lookup in the ItemAttribute table
        /// </summary>
        /// <param name="sourceAttributeLookupId">The source attribute lookup id, to use</param>
        /// <returns></returns>
        public List<ItemAttributeVariety> GetAssociatedVarients(Guid sourceAttributeLookupId)
        {
            List<ItemAttributeVariety> _itemAttributeVarieties = null;
            DbSet<ItemAttribute> _table = _Context.Set<ItemAttribute>();
            _AppUnitOfWork.ClearErrorMessage(); //-- clear any error message

            try
            {
                _Logger.LogDebug($"Finding the first variants associated to attribute lookup id: {sourceAttributeLookupId}");
                var _query = _table
                    .Include(ia => ia.ItemAttributeVarieties);
                var _itemAttribute =  _query.FirstOrDefault(ia => ia.ItemAttributeLookupId == sourceAttributeLookupId);
                _itemAttributeVarieties = _itemAttribute.ItemAttributeVarieties;
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first variants: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _itemAttributeVarieties;
        }
        //-> async version of above
        public async Task<List<ItemAttributeVariety>> GetAssociatedVarientsAsync(Guid sourceAttributeLookupId)
        {
            List<ItemAttributeVariety> _itemAttributeVarieties = null;
            DbSet<ItemAttribute> _table = _Context.Set<ItemAttribute>();
            _AppUnitOfWork.ClearErrorMessage(); //-- clear any error message

            try
            {
                _Logger.LogDebug($"Finding the first variants associated to attribute lookup id: {sourceAttributeLookupId}");
                var _query = _table
                    .Include(ia => ia.ItemAttributeVarieties);
                var _itemAttribute = await _query.FirstOrDefaultAsync(ia => ia.ItemAttributeLookupId == sourceAttributeLookupId);
                if (_itemAttribute == null)
                    return null;
                _itemAttributeVarieties = _itemAttribute.ItemAttributeVarieties;
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first variants: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _itemAttributeVarieties;
        }
        #endregion
    }
}
