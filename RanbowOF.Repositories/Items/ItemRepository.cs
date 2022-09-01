using LinqKit;
using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Common;
using System;
using System.Collections.Generic;
//using System.Data.Entity;  --> conflicting with EntityFrameworkCore
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public class ItemRepository : Repository<Item>, IItemRepository
    {
        #region Initialisation
        ///------> busy converting all these to us factory injection
        public ItemRepository(IDbContextFactory<ApplicationDbContext> appDbContext, ILoggerManager logger, IUnitOfWork AppUnitOfWork) : base(appDbContext, logger, AppUnitOfWork)
        {
            if (logger.IsDebugEnabled()) logger.LogDebug("ItemRepository initialised.");
        }
        #endregion

        #region DataGrid Handling
        static List<OrderByParameter<Item>> GetOrderByExpressions(List<SortParam> currentSortParams)
        {
            if (currentSortParams == null)
                return null;

            List<OrderByParameter<Item>> _orderByExpressions = new();
            foreach (var col in currentSortParams)
            {
                switch (col.FieldName)
                {
                    case nameof(Item.ItemName):
                        _orderByExpressions.Add(new OrderByParameter<Item>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = itm => itm.ItemName });
                        break;

                    case nameof(Item.SKU):
                        _orderByExpressions.Add(new OrderByParameter<Item>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = itm => itm.SKU });
                        break;

                    case nameof(Item.IsEnabled):
                        _orderByExpressions.Add(new OrderByParameter<Item>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = itm => itm.IsEnabled });
                        break;

                    case nameof(Item.ItemDetail):
                        _orderByExpressions.Add(new OrderByParameter<Item>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = itm => itm.ItemDetail });
                        break;

                    case nameof(Item.ItemAbbreviatedName):
                        _orderByExpressions.Add(new OrderByParameter<Item>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = itm => itm.ItemAbbreviatedName });
                        break;

                    case nameof(Item.SortOrder):
                        _orderByExpressions.Add(new OrderByParameter<Item>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = itm => itm.SortOrder });
                        break;

                    case nameof(Item.BasePrice):
                        _orderByExpressions.Add(new OrderByParameter<Item>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = itm => itm.BasePrice });
                        break;

                    default:
                        break;
                }
            }
            return _orderByExpressions;
        }

        static private List<Expression<Func<Item, bool>>> GetFilterByExpressions(List<FilterParam> currentFilterParams)
        {
            if (currentFilterParams == null)
                return null;
            List<Expression<Func<Item, bool>>> _filterByExpressions = new();
            foreach (var col in currentFilterParams)
            {
                col.FilterBy = col.FilterBy.ToLower();
                switch (col.FieldName)
                {
                    case nameof(Item.ItemName):
                        _filterByExpressions.Add(itm => itm.ItemName.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(Item.SKU):
                        _filterByExpressions.Add(itm => itm.SKU.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(Item.IsEnabled):
                        bool _IsTrue = ((col.FilterBy.Contains('y', StringComparison.OrdinalIgnoreCase)) || (col.FilterBy.Contains("enable", StringComparison.OrdinalIgnoreCase)));      // assume yes and no / enable or disable
                        _filterByExpressions.Add(icl => (icl.IsEnabled == _IsTrue));
                        break;

                    case nameof(Item.ItemDetail):
                        _filterByExpressions.Add(itm => itm.ItemDetail.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(Item.ItemAbbreviatedName):
                        _filterByExpressions.Add(itm => itm.ItemAbbreviatedName.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(Item.SortOrder):
                        _filterByExpressions.Add(itm => itm.SortOrder.ToString().ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(Item.BasePrice):
                        _filterByExpressions.Add(itm => itm.BasePrice.ToString().ToLower().Contains(col.FilterBy));
                        break;

                    default:
                        break;
                }
            }
            return _filterByExpressions;
        }
        public async Task<DataGridItems<Item>> GetPagedDataEagerWithFilterAndOrderByAsync(DataGridParameters currentDataGridParameters) // (int startPage, int currentPageSize)
        {
            DataGridItems<Item> _dataGridData = null;
            string statusString = $"Getting all records with eager loading of Item order by an filter Data Grid Parameters: {currentDataGridParameters}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<Item> _table = context.Set<Item>();
                // get a list of Order bys and filters
                List<OrderByParameter<Item>> _orderByExpressions = GetOrderByExpressions(currentDataGridParameters.SortParams);
                List<Expression<Func<Item, bool>>> _filterByExpressions = GetFilterByExpressions(currentDataGridParameters.FilterParams);
                // start with a basic Linq Query with Eager loading
                // do eager loading since we are working with paging, so load parent, replacement, categories, attributes and attribute varieties
                IQueryable<Item> _query = _table
                    .Include(itm => itm.ReplacementItem)
                    .Include(itm => itm.ItemCategories)
                        .ThenInclude(cats => cats.ItemCategoryDetail)
                    .Include(itm => itm.ItemAttributes)
                        .ThenInclude(itmAtts => itmAtts.ItemAttributeVarieties)
                        .ThenInclude(itmAttVars => itmAttVars.ItemAttributeVarietyDetail)
                    .Include(itm => itm.ItemAttributes)
                        .ThenInclude(itmAtts => itmAtts.ItemAttributeDetail)
                    .Include(itm => itm.ItemImages);
                //now add the order by expressions
                if ((_orderByExpressions != null) && (_orderByExpressions.Count > 0))
                {
                    // add order bys
                    if (_orderByExpressions.Count == 1)
                        _query = _orderByExpressions[0].IsAscending
                            ? _query.OrderBy(_orderByExpressions[0].OrderByExperssion)
                            : _query.OrderByDescending(_orderByExpressions[0].OrderByExperssion);
                    else   // we are only catering for one ThenBy, we could add more if else's here
                        _query = _orderByExpressions[0].IsAscending
                            ? (_orderByExpressions[1].IsAscending
                                ? _query.OrderBy(_orderByExpressions[0].OrderByExperssion).ThenBy(_orderByExpressions[1].OrderByExperssion)
                                : _query.OrderBy(_orderByExpressions[0].OrderByExperssion).ThenByDescending(_orderByExpressions[1].OrderByExperssion))
                            : (_orderByExpressions[1].IsAscending
                                ? _query.OrderByDescending(_orderByExpressions[0].OrderByExperssion).ThenBy(_orderByExpressions[1].OrderByExperssion)
                                : _query.OrderByDescending(_orderByExpressions[0].OrderByExperssion).ThenByDescending(_orderByExpressions[1].OrderByExperssion));
                }
                else   // default sort
                    _query = _query.OrderBy(itm => itm.ItemName);

                //This is functionally comes from
                //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/how-to-use-expression-trees-to-build-dynamic-queries
                //now the per column filters
                if (!String.IsNullOrEmpty(currentDataGridParameters.CustomFilter) || _filterByExpressions != null)
                {
                    Expression<Func<Item, bool>> _queryWhere = PredicateBuilder.New<Item>(true);
                    if (!String.IsNullOrEmpty(currentDataGridParameters.CustomFilter))
                    {
                        _queryWhere = _queryWhere.And(itm => ((itm.ItemName.ToLower().Contains(currentDataGridParameters.CustomFilter))
                                                           || (itm.SKU.ToLower().Contains(currentDataGridParameters.CustomFilter))
                                                           || (itm.ItemAbbreviatedName.ToLower().Contains(currentDataGridParameters.CustomFilter))));
                    }
                    if (_filterByExpressions != null)
                    {
                        foreach (var flt in _filterByExpressions)
                            _queryWhere = _queryWhere.And(flt);
                    }
                    _query = _query.Where(_queryWhere);   //  (_queryWhere);
                }

                //now we can add the page stuff - first get the count to return
                _dataGridData = new()
                {
                    TotalRecordCount = _query.Count()
                };
                _query = _query
                   .Skip((currentDataGridParameters.CurrentPage - 1) * currentDataGridParameters.PageSize)
                   .Take(currentDataGridParameters.PageSize);   // take 3 pages at least.
                _dataGridData.Entities = await _query.ToListAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all records from ItemRepository: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _dataGridData;
        }
        #endregion
        #region Database retrieval and manipulation 
        public async Task<Item> FindFirstEagerLoadingItemAsync(Expression<Func<Item, bool>> predicate)
        {
            Item _item = null;
            // AppUnitOfWork.AppDbContext.ChangeTracker.LazyLoadingEnabled = true;
            string statusString = $"Finding first eager loading - whole item using predicate: {predicate}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<Item> _table = context.Set<Item>();
                var _query = _table.AsNoTracking()
                    .Include(itm => itm.ReplacementItem).AsNoTracking()
                    //.Include(itm => itm.ItemCategories)
                    //    .ThenInclude(itmCat => itmCat.ItemCategoryDetail).AsNoTracking()
                    //.Include(itm => itm.ItemCategories)
                    //    .ThenInclude(itmCat => itmCat.ItemUoMBase).AsNoTracking()
                    //.Include(itm => itm.ItemAttributes)
                    //    //.ThenInclude(itmAtts => itmAtts.ItemAttributeDetail).AsNoTracking()
                    //.Include(itm => itm.ItemAttributes)
                    //.ThenInclude(itmAtts => itmAtts.ItemAttributeVarieties)
                    //     .ThenInclude(itmAttVars => itmAttVars.ItemAttributeVarietyDetail).AsNoTracking()
                    //.ThenInclude(iavd => iavd.UoM) => crashes here and cannot figure out why
                    .Include(itm => itm.ItemImages).AsNoTracking();

                if (AppLoggerManager.IsDebugEnabled())
                {
                    var _sql = _query.ToQueryString();
                    AppLoggerManager.LogDebug($"SQL Query is: {_sql}");
                }

                _item = await _query.FirstOrDefaultAsync(predicate);

                //// load VarietyDetail UoM manually as then include crashes
                //if (_item != null)  /// do some ordering 
                //{
                //    _item.ItemCategories = _item.ItemCategories.OrderBy(itemCat => itemCat.ItemCategoryDetail.FullCategoryName).ToList();
                //    //IAppRepository<ItemAttributeVariety> _itemAttributeVarietyRepository = AppUnitOfWork.Repository<ItemAttributeVariety>();
                //    //IRepository<ItemUoMLookup> _ItemUoMLookupRepository = AppUnitOfWork.Repository<ItemUoMLookup>();
                //    //foreach (var itemAtt in _item.ItemAttributes)
                //    //{
                //    //    ////.Include(itm => itm.ItemAttributes)
                //    //    ////    .ThenInclude(itmAtts => itmAtts.ItemAttributeVarieties)
                //    //    ////        .ThenInclude(itmAttVars => itmAttVars.ItemAttributeVarietyDetail)
                //    //    //if (itemAtt.ItemAttributeId != Guid.Empty)
                //    //    //{
                //    //    //    itemAtt.ItemAttributeVarieties = (await _itemAttributeVarietyRepository.GetByAsync(iav => iav.ItemAttributeId == itemAtt.ItemAttributeId)).ToList();
                //    //    //}
                //    //    foreach (var itemAttVar in itemAtt.ItemAttributeVarieties)
                //    //    {
                //    //        //.ThenInclude(iavd => iavd.UoM) => crashes here and cannot figure out why
                //    //        if (itemAttVar.ItemAttributeVarietyDetail.UoMId != null)
                //    //        {
                //    //            itemAttVar.ItemAttributeVarietyDetail.UoM = await _ItemUoMLookupRepository.GetByIdAsync(itemAttVar.ItemAttributeVarietyDetail.UoMId);
                //    //        }
                //    //    }

                //    //}
                //}
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }

            return _item;
        }

        public async Task<Item> FindFirstItemBySKUAsync(string sourceSKU)
        {
            return await GetByIdAsync(i => i.SKU == sourceSKU);
        }

        public async Task<Item> AddItemAsync(Item newItem)
        {
            if (!String.IsNullOrEmpty(newItem.SKU))
            {
                if (await FindFirstItemBySKUAsync(newItem.SKU) != null)
                {
                    AppUnitOfWork.LogAndSetErrorMessage($"ERROR adding Item:  {newItem} - duplicate SKU found in database");
                    return null;
                }
            }
            // if we get here then the SKU is null or does not exists
            return await AddAsync(newItem);
        }
        public async Task<bool> DetachItemById(Guid sourceItemId)
        {
            try
            {
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Detach item using id: {sourceItemId}");
                var tempItem = await GetByIdAsync(sourceItemId);
                //AppDbContext.Entry(tempItem).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                return true;
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Detaching Item by Id: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
                return false;
            }
        }
        #endregion
        #region Item Categories, Attributes and Item Varieties
        /// <summary>
        /// Returns a list of items that are similar based on the item's primary category being the same
        /// </summary>
        /// <param name="sourceItemId"></param>
        /// <param name="sourceItemPrimaryCategoryId"></param>
        /// <returns></returns>
        public List<Item> GetSimilarItems(Guid sourceItemId, Guid? sourceItemPrimaryCategoryId)
        {
            List<Item> _similarItems = null;
            // if you change this change the async routine too, not sure how to do both in one routine
            string statusString = $"Get Similar Items By Primary Category Id: {sourceItemPrimaryCategoryId}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = AppDbContext.CreateDbContext();
                DbSet<Item> _table = context.Set<Item>();

                var _query = _table
                       .Where(itm =>
                              (itm.ItemId != sourceItemId)
                           && (itm.PrimaryItemCategoryLookupId == (sourceItemPrimaryCategoryId ?? null))
                         )
                       .AsNoTracking()
                       .OrderBy(itm => itm.SortOrder)
                       .ThenBy(itm => itm.ItemName);

                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"SQL Query is: {_query.ToQueryString()}");
                _similarItems = _query.ToList();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting Eager Item Category By Item Id: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _similarItems;
        }

        /// workings around selecting all items based on category or parent category
        /// 
        //var getItemCategoryQuery = table.Select(i => i.ItemCategories.Where(ic => ic.ItemCategoryLookupId == sourceItemPrimaryCategoryId)).FirstOrDefault();
        //IQueryable<Item> query = null;
        //        if (getItemCategoryQuery == null)  // no categories?
        //        {
        //        }
        //        /// Select all items that have the same ItemCategoryLookupId as this items PrimaryCategory or if it has a parent the Same Category as the ParentCategory  
        //        else 
        //        {
        //            var CatId = getItemCategoryQuery.Single().ItemCategoryDetail.ParentCategoryId;
        //CatId = (CatId == null) ? sourceItemPrimaryCategoryId : CatId;
        //            query = table
        //                .Where(ia =>
        //                        (ia.ItemId != sourceItemId) && (ia.ItemCategories.Exists(ic => ic.ItemCategoryId == CatId))
        //                    );
        //        }

        /// <summary>
        /// Get a list of item Category Lookups that are marked as used for variety using the ItemId
        /// </summary>
        /// <param name="sourceItemId">Item Id to use</param>
        /// <returns>List of Item Category Lookups including Item CategoryDetail and List of Varieties as per the database</returns>
        public List<ItemCategoryLookup> GetEagerItemsCategoryLookupsByItemId(Guid sourceItemId)
        {
            List<ItemCategoryLookup> _itemsCategoryLookups = null;
            string statusString = $"Get Eager Item Category By Item Id: {sourceItemId}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                // if you change this change the async routine too, not sure how to do both in one routine
                using var context = AppDbContext.CreateDbContext();
                DbSet<ItemCategory> _table = context.Set<ItemCategory>();
                var _query = _table
                    .Include(ia => ia.ItemCategoryDetail)
                    .OrderBy(ia => ia.ItemCategoryDetail.CategoryName)
                    .Where(ia => (ia.ItemId == sourceItemId))
                    ;
                //_query = _query
                //    .OrderBy(itm => itm.ItemCategories.OrderBy(ia => ia.ItemCategoryDetail.CategoryName))     //hope this works
                ;
                var _sql = _query.ToQueryString();
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"SQL Query is: {_sql}");
                var itemCategories = _query; //.ToList(); // && (i.ItemCategories.Any(ia => ia.IsUsedForItemVariety == true)));

                _itemsCategoryLookups = itemCategories.Select(ic => ic.ItemCategoryDetail).ToList();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting Eager Item Category By Item Id: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _itemsCategoryLookups;
        }

        /// <summary>
        /// Get a list of item attributes that are marked as used for variety using the ItemId
        /// </summary>
        /// <param name="sourceItemId">Item Id to use</param>
        /// <returns>List of Item Attributes including Item AttributeDetail and List of Varieties as per the database</returns>
        public List<ItemAttribute> GetEagerItemVariableAttributeByItemId(Guid sourceItemId)
        {
            List<ItemAttribute> _itemAttributes = null;
            string statusString = $"Get Eager Item Attribute By Item Id: {sourceItemId}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                // if you change this change the async routine too, not sure how to do both in one routine
                using var context = AppDbContext.CreateDbContext();
                DbSet<ItemAttribute> _table = context.Set<ItemAttribute>();
                var _query = _table
                    .Include(ia => ia.ItemAttributeDetail)
                        .ThenInclude(iad => iad.ItemAttributeVarietyLookups)
                    .Include(ia => ia.ItemAttributeVarieties)
                    .OrderBy(ia => ia.ItemAttributeDetail.AttributeName)
                    .Where(ia => ((ia.ItemId == sourceItemId) && (ia.IsUsedForItemVariety)))
                    ;
                //_query = _query
                //    .OrderBy(itm => itm.ItemAttributes.OrderBy(ia => ia.ItemAttributeDetail.AttributeName))     //hope this works
                ;
                if (AppLoggerManager.IsDebugEnabled())
                {
                    var _sql = _query.ToQueryString();
                    AppLoggerManager.LogDebug($"SQL Query is: {_sql}");
                }
                _itemAttributes = _query.ToList(); // && (i.ItemAttributes.Any(ia => ia.IsUsedForItemVariety == true)));
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting Eager Item Attribute By Item Id: {ex.Message} - Inner Exception {ex.InnerException}");
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
        public List<ItemAttributeVariety> GetEagerItemAttributeVarietiesByItemIdAndAttributeLookupId(Guid sourceItemId, Guid sourceItemAttributeLookupId)
        {
            List<ItemAttributeVariety> _itemAttributeVarieties = null;
            string statusString = $"Get Eager Item Attribute Variety By Item Id and AttributeLookupId: {sourceItemId}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                // if you change this change the async routine too, not sure how to do both in one routine
                using var context = AppDbContext.CreateDbContext();
                DbSet<ItemAttribute> _table = context.Set<ItemAttribute>();
               var _query = _table.AsNoTracking()
                    .Where(ia => (ia.ItemId == sourceItemId) && (ia.ItemAttributeLookupId == sourceItemAttributeLookupId))
                    .Include(ia => ia.ItemAttributeVarieties)
                        .ThenInclude(iav => iav.ItemAttributeVarietyDetail)
                        ;

                //_query = _query.OrderBy(ia => ia.ItemAttributeVarieties.OrderBy(iav => iav.ItemAttributeVarietyDetail).Select(iav => iav.ItemAttributeVarietyDetail).FirstOrDefault());
                //_query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ItemAttribute, ItemAttributeVarietyLookup>)_query.OrderBy(ia => ia.ItemAttributeVarieties.OrderBy(iav => iav.ItemAttributeVarietyDetail.VarietyName));

                //_query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ItemAttribute, ItemAttributeVarietyLookup>)_query.Where(ia => (ia.ItemId == sourceItemId) && (ia.ItemAttributeLookupId == sourceItemAttributeLookupId));

                if (AppLoggerManager.IsDebugEnabled())
                {
                    var _sql = _query.ToQueryString();
                    AppLoggerManager.LogDebug($"SQL Query is: {_sql}");
                }
                var _itemAttribute = _query.FirstOrDefault(); // ia => (ia.ItemId == sourceItemId) && (ia.ItemAttributeLookupId == sourceItemAttributeLookupId));
                if (_itemAttribute != null)
                {
                    _itemAttributeVarieties = _itemAttribute.ItemAttributeVarieties
                        .OrderBy(iav => iav.ItemAttributeVarietyDetail.SortOrder)
                        .ThenBy(iav => iav.ItemAttributeVarietyDetail.VarietyName).ToList();  // cannot figure out how to do this in the above
                }
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting Eager Item Attribute Variety By Item Id and AttributeLookupId: {ex.Message} - Inner Exception {ex.InnerException}");
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
        public async Task<List<ItemAttributeVariety>> GetEagerItemAttributeVarietiesByItemIdAndAttributeLookupIdAsync(Guid sourceItemId, Guid sourceItemAttributeLookupId)
        {
            List<ItemAttributeVariety> _itemAttributeVarieties = null;
            string statusString = $"Get Eager Item Attribute Variety By Item Id and AttributeLookupId Async: {sourceItemId}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                // if you change this change the async routine too, not sure how to do both in one routine
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemAttribute> _table = context.Set<ItemAttribute>();
                var _query = _table
                    .Include(ia => ia.ItemAttributeVarieties)
                    .ThenInclude(iav => iav.ItemAttributeVarietyDetail)
                    .OrderBy(ia => ia.ItemAttributeVarieties.OrderBy(iav => iav.ItemAttributeVarietyDetail.VarietyName));
                var _sql = _query.ToQueryString();
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"SQL Query is: {_sql}");

                var _itemAttribute = await _query.FirstOrDefaultAsync(ia => (ia.ItemId == sourceItemId) && (ia.ItemAttributeLookupId == sourceItemAttributeLookupId));
                if (_itemAttribute != null)
                    _itemAttributeVarieties = _itemAttribute.ItemAttributeVarieties;
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting Eager Item Attribute Variety By Item Id and AttributeLookupId Async: {ex.Message} - Inner Exception {ex.InnerException}");
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
        public async Task<bool> DoesThisItemHaveVariableAttributesAsync(Guid currentItemId)
        {
            bool _itemHasVariableAttributes = false;
            string statusString = $"Does this item have variable attributes async: {currentItemId}";
            if (!CanDoDbAsyncCall(statusString))
                return false;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemAttribute> _table = context.Set<ItemAttribute>();
                var _query = _table.Where(ita => (ita.ItemId == currentItemId) && (ita.IsUsedForItemVariety == true));
                var _result = await _query.FirstOrDefaultAsync();
                _itemHasVariableAttributes = _result != null;
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _itemHasVariableAttributes;
        }


        //        public List<ItemAttribute> GetAllItemVariableAttributesByItemId(Guid sourceItemId)
        //        {
        //            List<ItemAttribute> _itemAttributes = null;
        //            DbSet<ItemAttribute> _table = appDbContext.Set<ItemAttribute>();
        //            AppUnitOfWork.ClearErrorMessage(); //-- clear any error message

        //            try
        //            {
        //                // if you change this change the async routine too, not sure how to do both in one routine
        //                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Get all Item varaible Attribute By Item Id: {sourceItemId}");
        //                var _query = _table
        //                    .Include(ia => ia.ItemAttributeDetail)
        ////                    .Include(ia => ia.ItemAttributeVarieties)
        //                    .OrderBy(ia => ia.ItemAttributeDetail.AttributeName)
        //                    .Where(ia => ((ia.ItemId == sourceItemId) && (ia.IsUsedForItemVariety)))
        //                    ;
        //                //_query = _query
        //                //    .OrderBy(itm => itm.ItemAttributes.OrderBy(ia => ia.ItemAttributeDetail.AttributeName))     //hope this works
        //                ;
        //                var _sql = _query.ToQueryString();
        //                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"SQL Query is: {_sql}");
        //                _itemAttributes = _query.ToList(); // && (i.ItemAttributes.Any(ia => ia.IsUsedForItemVariety == true)));
        //            }
        //            catch (Exception ex)
        //            {
        //                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting Eager Item Attribute By Item Id: {ex.Message} - Inner Exception {ex.InnerException}");
        //#if DebugMode
        //                throw;     // #Debug?
        //#endif
        //            }
        //            return _itemAttributes;
        //        }
        #endregion

        #region Item Variant routines
        public async Task<List<ItemVariant>> GetAllItemVariantsEagerByItemIdAsync(Guid sourceItemId)
        {
            List<ItemVariant> itemVariants = null;
            // if you change this change the sync routine too, not sure how to do both in one routine
            string statusString = $"Get Eager Item Variants By Item Id : {sourceItemId}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemVariant> _table = context.Set<ItemVariant>();
                var _query = _table
                    .Include(iv => iv.ItemVariantAssociatedLookups)
                        .ThenInclude(iva => iva.AssociatedAttributeVarietyLookup)
                    .Include(iv => iv.ItemVariantAssociatedLookups)
                        .ThenInclude(iva => iva.AssociatedAttributeLookup)
                    .OrderBy(iv => iv.SortOrder)
                        .ThenBy(iv => iv.ItemVariantName)
                    .AsNoTracking()
                    .Where(iv => iv.ItemId == sourceItemId);
                if (AppLoggerManager.IsDebugEnabled())
                {
                    var _sql = _query.ToQueryString();
                    AppLoggerManager.LogDebug($"SQL Query is: {_sql}");
                }
                itemVariants = await _query.ToListAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting Eager Item Variants By Item Id and AttributeLookupId Async: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return itemVariants;
        }
        /// <summary>
        /// Get the Item variant and all the related tables / Lists for the Item Variant whose Id is passed in
        /// </summary>
        /// <param name="sourceItemVariant">Id of the ItemVariant to be returned</param>
        /// <returns>The Item variant with all related lists retrieved.</returns>
        public async Task<ItemVariant> GetItemVariantEagerByItemVariantIdAsync(Guid sourceItemVariantId)
        {
            ItemVariant itemVariant = null;
            // if you change this change the sync routine too, not sure how to do both in one routine
            string statusString = $"Get Eager Item Variant By Item Variant Id : {sourceItemVariantId}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemVariant> _table = context.Set<ItemVariant>();
                var _query = _table
                    .Include(iv => iv.ItemVariantAssociatedLookups)
                        .ThenInclude(iav => iav.AssociatedAttributeLookup)
                        .ThenInclude(iav => iav.ItemAttributeVarietyLookups)
                    .OrderBy(iv => iv.SortOrder)
                    .ThenBy(iv => iv.ItemVariantName)
                    .Where(iv => iv.ItemVariantId == sourceItemVariantId);
                if (AppLoggerManager.IsDebugEnabled())
                {
                    var _sql = _query.ToQueryString();
                    AppLoggerManager.LogDebug($"SQL Query is: {_sql}");
                }
                itemVariant = await _query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting Eager Item Variant By Item Id and AttributeLookupId Async: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return itemVariant;
        }
        ///// <summary>
        ///// Get all the variants that the item has an attribute marked as variable 
        ///// </summary>
        ///// <param name="sourceItemId">The ItemId of the parent Item</param>
        ///// <returns></returns>
        //Task<List<ItemAttributeVariety>> GetAllPossibleVariants(Guid sourceItemId);
        public async Task<bool> DeleteItemVariantAndAssociatedData(ItemVariant sourceItemVariant)
        {
            bool _deleted = false;
            // if you change this change the sync routine too, not sure how to do both in one routine
            string statusString = $"Deleting all the variants of Item Variant Id: {sourceItemVariant.ItemVariantId}";
            if (!CanDoDbAsyncCall(statusString))
                return false;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemVariantAssociatedLookup> _itemVariantAssociatedLookupTable = context.Set<ItemVariantAssociatedLookup>();
                DbSet<ItemVariant> _itemVariantsTable = context.Set<ItemVariant>();
                //delete all the lookups
                foreach (var itemVariantAssociateLookup in sourceItemVariant.ItemVariantAssociatedLookups)
                {
                    if (itemVariantAssociateLookup.ItemVariantAssociatedLookupId != Guid.Empty)  // check it it is not set to "all", this is not a real value
                        _itemVariantAssociatedLookupTable.Remove(itemVariantAssociateLookup);
                }
                _itemVariantsTable.Remove(sourceItemVariant); //
                _deleted &= (await context.SaveChangesAsync()) >= 0;
                // now delete the item itself
                // _deleted = ! AppUnitOfWork.IsInErrorState();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error removing all the Item's Variants By Item Id Async: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _deleted;
        }

        /// <summary>
        /// Delete all the variants of this item
        /// </summary>
        /// <param name="sourceItemId">The Id of the item whose variants we wont to delete</param>
        /// <returns>true of successful</returns>
        public async Task<bool> DeleteAllItemsVariantsAsync(Guid sourceItemId)
        {
            bool _deleted = false;
            // if you change this change the sync routine too, not sure how to do both in one routine
            string statusString = $"Deleting all the variants of Item Id: {sourceItemId}";
            if (!CanDoDbAsyncCall(statusString))
                return false;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                DbSet<ItemVariant> _itemVariantsTable = context.Set<ItemVariant>();
                var _itemVariantsToDelelete = await _itemVariantsTable.Where(iv => iv.ItemId == sourceItemId).ToArrayAsync();  // get all the variants currently in the database.
                _itemVariantsTable.RemoveRange(_itemVariantsToDelelete);
                _deleted = await (context.SaveChangesAsync()) >= 0;  
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error removing all the Item's Variants By Item Id Async: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _deleted;
        }

        public async Task<bool> SetItemTypeAsync(Guid currentItemId, ItemTypes currentItemType)
        {
            bool _done = false;
            // if you change this change the sync routine too, not sure how to do both in one routine
            string statusString = $"Setting item type for Item Id: {currentItemId} to {currentItemType}";
            if (!CanDoDbAsyncCall(statusString))
                return false;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                Item _item = await context.Items.FirstOrDefaultAsync(it => it.ItemId == currentItemId);
                if (_item == null)
                    return false;
                _item.ItemType = currentItemType;
                context.Items.Update(_item);
                _done = (await context.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error removing all the Item's Variants By Item Id Async: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _done;
        }

        /// <summary>
        /// Get all the variants that where the item has an attribute marked as variable but no Item Variant(s) exits that are assigned to an ItemVariant
        /// </summary>
        /// <param name="sourceItemId">The ItemId of the parent Item</param>
        //        /// <returns></returns>
        //        public async Task<List<ItemAttributeVariety>> GetAllPossibleVariants(Guid sourceItemId)
        //        {
        //            List<ItemAttributeVariety> possibleVariants = null;
        //            ApplicationDbContext dbContext = GetappDbContext();
        //            if (dbContext == null) return null;
        //            try
        //            {
        //                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Get All Possible Variants By Item Id : {sourceItemId}");

        //                var result = dbContext.ItemAttributes
        //                    .Where(ia => (ia.ItemId == sourceItemId) && (ia.IsUsedForItemVariety == true))
        //                    .Include(iav => iav.ItemAttributeVarieties)
        //                    .OrderBy(ia => ia.ItemAttributeDetail.OrderBy)
        //                    .ThenBy(ia => ia.ItemAttributeDetail.AttributeName);

        //                possibleVariants = await result.ToListAsync();

        //            }
        //            catch (Exception ex)
        //            {
        //                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting Eager Item Variant By Item Id and AttributeLookupId Async: {ex.Message} - Inner Exception {ex.InnerException}");
        //#if DebugMode
        //                throw;     // #Debug?
        //#endif
        //            }
        //            return possibleVariants;
        //        }
        //var result = dbContext.ItemAttributeVarieties
        //        .Where(iav => !dbContext.ItemVariants
        //                        .Select(iv => iv.ItemId)
        //                        .Contains(sourceItemId))
        //        .Include(iav => iav.ItemAttributeVarietyDetail;
        #endregion
    }
}
