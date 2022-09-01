using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public class ItemAttributeRepository : Repository<ItemAttribute>, IItemAttributeRepository
    {
        #region Initialisation
        public ItemAttributeRepository(IDbContextFactory<ApplicationDbContext> sourceDbContext, ILoggerManager logger, IUnitOfWork AppUnitOfWork) : base(sourceDbContext, logger, AppUnitOfWork)
        {
            if (logger.IsDebugEnabled()) logger.LogDebug("ItemAttributeRepository initialised...");
        }
        #endregion
        #region Checking routines
        public async Task<bool> IsUniqueItemAttributeAsync(ItemAttribute sourceItemAttribute)
        {
            string statusString = $"Is Unique Item Attribute Async: {sourceItemAttribute}" ;
            if (!CanDoDbAsyncCall(statusString))
                return false;
            bool isUniqueItemAttribute = false;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemAttribute> _table = context.Set<ItemAttribute>();
                var result = await _table.FirstOrDefaultAsync((sourceItemAttribute.ItemAttributeId == Guid.Empty) ?
                           (ia => (ia.ItemId == sourceItemAttribute.ItemId)
                               && (ia.ItemAttributeLookupId == sourceItemAttribute.ItemAttributeLookupId)) :
                            (ia => (ia.ItemId == sourceItemAttribute.ItemId)
                                && (ia.ItemAttributeId != sourceItemAttribute.ItemAttributeId)
                                && (ia.ItemAttributeLookupId == sourceItemAttribute.ItemAttributeLookupId))
                                      );
                isUniqueItemAttribute = result != null;
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Is Unique Item Attribute Async: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return isUniqueItemAttribute;
        }
        #endregion
        #region ItemAttribute related routines
        public List<ItemAttribute> GetEagerItemAttributeByItemIdAsync(Guid sourceItemId)
        {
            List<ItemAttribute> _itemAttributes = null;
            string statusString = $"Get Eager Item Attribute By Item Id Async: {sourceItemId}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = AppDbContext.CreateDbContext();
                DbSet<ItemAttribute> _table = context.Set<ItemAttribute>();
                // if you change this change the sync routine too, not sure how to do both in one routine
                var _query = _table.AsNoTracking()
                    .Include(ia => ia.ItemAttributeVarieties)
                        .ThenInclude(iv => iv.ItemAttributeVarietyDetail).AsNoTracking()
                    .Include(ia => ia.ItemAttributeDetail)
                    .OrderBy(ia => ia.ItemAttributeDetail.AttributeName)
                    .Where(ia => (ia.ItemId == sourceItemId))
                    ;
                if (AppLoggerManager.IsDebugEnabled())
                {
                    var _sql = _query.ToQueryString();
                    AppLoggerManager.LogDebug($"SQL Query is: {_sql}");
                }
                _itemAttributes = _query.ToList();  //&& (i.ItemAttributes.Where(ia => ia.is ) .Any(ia => (ia.IsUsedForItemVariety==true))));
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting Eager Item Attribute By Item Id Async: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _itemAttributes;
        }

        /// <summary>
        /// Return a list of variants associated to the source attribute lookup in the ItemAttribute table
        /// </summary>
        /// <param name="sourceAttributeLookupId">The source attribute lookup id, to use</param>
        /// <returns></returns>
        public List<ItemAttributeVariety> GetAssociatedVarients(Guid sourceAttributeLookupId)
        {
            List<ItemAttributeVariety> _itemAttributeVarieties = null;
            string statusString = $"Finding the first variants associated to attribute lookup id: {sourceAttributeLookupId}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = AppDbContext.CreateDbContext();
                DbSet<ItemAttribute> _table = context.Set<ItemAttribute>();
                var _query = _table
                    .Include(ia => ia.ItemAttributeVarieties)
                    .Where(ia => ia.ItemAttributeLookupId == sourceAttributeLookupId);
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Query: {_query.ToQueryString()}");
                var _itemAttribute = _query.FirstOrDefault();
                _itemAttributeVarieties = _itemAttribute.ItemAttributeVarieties;
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first variants: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _itemAttributeVarieties;
        }
        //-> async version of above
        public async Task<List<ItemAttributeVariety>> GetAssociatedVarientsAsync(Guid sourceAttributeLookupId)
        {
            List<ItemAttributeVariety> _itemAttributeVarieties = null;
            string statusString = $"Finding the first variants associated to attribute lookup id: {sourceAttributeLookupId} async";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemAttribute> _table = context.Set<ItemAttribute>();
                var _query = _table
                    .Include(ia => ia.ItemAttributeVarieties)
                    .Where(ia => ia.ItemAttributeLookupId == sourceAttributeLookupId);
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Query: {_query.ToQueryString()}");
                var _itemAttribute = await _query.FirstOrDefaultAsync();
                if (_itemAttribute == null)
                    return null;
                _itemAttributeVarieties = _itemAttribute.ItemAttributeVarieties;
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first variants async: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _itemAttributeVarieties;
        }
        public async Task<ItemAttributeLookup> GetItemAttributeLookupByIdAsync(Guid sourceItemAttributeLookupId)
        {
            ItemAttributeLookup _itemAttributeLookup;
            string statusString = $"Getting item attribute id: {sourceItemAttributeLookupId}, with no tracking";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemAttributeLookup> _table = context.Set<ItemAttributeLookup>();
                var _query = _table
                    .AsNoTracking()
                    .Where(ial => ial.ItemAttributeLookupId == sourceItemAttributeLookupId);
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Query: {_query.ToQueryString()}");
                _itemAttributeLookup = await _query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!ErrorGetting item attribute id: {ex.Message} - Inner Exception {ex.InnerException}");
                _itemAttributeLookup = null;
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _itemAttributeLookup;
        }
        /// <summary>
        /// Get an ItemAttribute by Id and disable tracking
        /// </summary>
        /// <param name="sourceAttributeLookupId">Id to search for</param>
        /// <returns></returns>
        public async Task<ItemAttribute> GetItemAttributeByIdNoTrackingAsync(Guid sourceItemAttributeId)
        {
            ItemAttribute _itemAttribute = null;
            string statusString = $"Getting item attribute id: {sourceItemAttributeId}, with no tracking";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemAttribute> _table = context.Set<ItemAttribute>();
                var _query = _table
                    .AsNoTracking()
                    .Where(ia => ia.ItemAttributeId == sourceItemAttributeId);
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Query: {_query.ToQueryString()}");
                _itemAttribute = await _query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!ErrorGetting item attribute id: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _itemAttribute;
        }
        /// <summary>
        /// Update on the item attribute
        /// </summary>
        /// <param name="sourceItemAttribute"></param>
        /// <returns></returns>
        public async Task<int> UpdateItemAttributeAsync(ItemAttribute sourceItemAttribute)
        {
            int _result = UnitOfWork.CONST_WASERROR;
            string statusString = $"Updating item attribute id: {sourceItemAttribute.ItemAttributeId} ";
            if (!CanDoDbAsyncCall(statusString))
                return _result;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemAttribute> _table = context.Set<ItemAttribute>();
                _table.Update(sourceItemAttribute);
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Query: {_table.ToQueryString()}");
                context.Entry(sourceItemAttribute).State = EntityState.Modified;
                _result = await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error updating variants: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _result;
        }
    }
    #endregion
}
