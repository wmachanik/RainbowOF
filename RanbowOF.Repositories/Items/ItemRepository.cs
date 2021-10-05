using LinqKit;
using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Items;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Common;
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

        #region Injected Items
        private ApplicationDbContext _Context = null;
        private ILoggerManager _Logger { get; set; }
        private IAppUnitOfWork _AppUnitOfWork { get; set; }
        #endregion

        #region Initialisation
        public ItemRepository(ApplicationDbContext dbContext, ILoggerManager logger, IAppUnitOfWork appUnitOfWork) : base(dbContext, logger, appUnitOfWork)
        {
            _Context = dbContext;
            _Logger = logger;
            _AppUnitOfWork = appUnitOfWork;
        }
        #endregion

        #region DataGrid Handling
        List<OrderByParameter<Item>> GetOrderByExpressions(List<SortParam> currentSortParams)
        {
            if (currentSortParams == null)
                return null;

            List<OrderByParameter<Item>> _orderByExpressions = new List<OrderByParameter<Item>>();
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

        private List<Expression<Func<Item, bool>>> GetFilterByExpressions(List<FilterParam> currentFilterParams)
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
                        bool _IsTrue = ((col.FilterBy.Contains("y", StringComparison.OrdinalIgnoreCase)) || (col.FilterBy.Contains("enable", StringComparison.OrdinalIgnoreCase)));      // assume yes and no / enable or disable
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
            DbSet<Item> _table = _Context.Set<Item>();

            try
            {
                _Logger.LogDebug($"Getting all records with eager loading of Item order by an filter Data Grid Parameters: {currentDataGridParameters.ToString()}");
                if (_AppUnitOfWork.DBTransactionIsStillRunning())
                    _Logger.LogDebug("Second transaction started before current transaction completed!");
                // get a list of Order bys and filters
                List<OrderByParameter<Item>> _orderByExpressions = GetOrderByExpressions(currentDataGridParameters.SortParams);
                List<Expression<Func<Item, bool>>> _filterByExpressions = GetFilterByExpressions(currentDataGridParameters.FilterParams);
                // start with a basic Linq Query with Eager loading
                // do eager loading since we are working with paging, so load parent, replacement, categories, attributes and attribute varieties
                IQueryable<Item> _query = _table
//                    .Include(itm => itm.ParentItem)  --> variant
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
                _dataGridData = new DataGridItems<Item>();
                _dataGridData.TotalRecordCount = _query.Count();
                _query = _query
                   .Skip((currentDataGridParameters.CurrentPage - 1) * currentDataGridParameters.PageSize)
                   .Take(currentDataGridParameters.PageSize);   // take 3 pages at least.
                // and execute
                _dataGridData.Entities = await _query.ToListAsync();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all records from ItemRepository: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _dataGridData;
        }
        #endregion

        #region Database retrieval and manipulation 
        public async Task<Item> FindFirstEagerLoadingItemAsync(Expression<Func<Item, bool>> predicate)
        {
            Item _item = null;
            DbSet<Item>  _table = _Context.Set<Item>();

            try
            {
                _Logger.LogDebug($"Finding first eager loading - whole item using predicate: {predicate.ToString()}");
                _item = await _table
                    .Include(itm => itm.ReplacementItem)
                    .Include(itm => itm.ItemCategories)
                        .ThenInclude(itmCat => itmCat.ItemCategoryDetail)
                    .Include(itm => itm.ItemAttributes)
                        .ThenInclude(itmAtts => itmAtts.ItemAttributeDetail)
                    .Include(itm => itm.ItemAttributes)
                        .ThenInclude(itmAtts => itmAtts.ItemAttributeVarieties)
                        .ThenInclude(itmAttVars => itmAttVars.ItemAttributeVarietyDetail)
                    .Include(itm => itm.ItemImages)
                    .FirstOrDefaultAsync(predicate);

                /// -> do we include variants
                    ///.Include(itm => itm.ParentItem)

            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _item;
        }

        public async Task<Item> FindFirstItemBySKUAsync(string sourceSKU)
        {
            return await FindFirstAsync(i => i.SKU == sourceSKU);            
        }

        public async Task<int> AddItemAsync(Item newItem)
        {
            if (!String.IsNullOrEmpty(newItem.SKU))
            {
                if (await FindFirstItemBySKUAsync(newItem.SKU) != null)
                {
                    _AppUnitOfWork.LogAndSetErrorMessage($"ERROR adding Item:  {newItem.ToString()} - duplicate SKU found in database");
                    return AppUnitOfWork.CONST_WASERROR;
                }
            }
            // if we get here then the SKU is null or does not exists
            return await AddAsync(newItem);
        }
        #endregion
    }
}
