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
        private ApplicationDbContext _context = null;
        private ILoggerManager _logger { get; set; }
        private IAppUnitOfWork _appUnitOfWork { get; set; }
        #endregion

        #region Inititmisation
        public ItemRepository(ApplicationDbContext dbContext, ILoggerManager logger, IAppUnitOfWork appUnitOfWork) : base(dbContext, logger, appUnitOfWork)
        {
            _context = dbContext;
            _logger = logger;
            _appUnitOfWork = appUnitOfWork;
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
            List<Expression<Func<Item, bool>>> _filterByExpressions = new List<Expression<Func<Item, bool>>>();
            foreach (var col in currentFilterParams)
            {
                col.FilterBy = col.FilterBy.ToLower();
                switch (col.FieldName)
                {
                    case nameof(Item.ItemName):
                        _filterByExpressions.Add(itm => itm.ItemName.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(Item.SKU):
                        _filterByExpressions.Add(itm => itm.SKU.ToString().ToLower().Contains(col.FilterBy));
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
            DbSet<Item> _table = _context.Set<Item>();

            try
            {
                _logger.LogDebug($"Getting all records with eager loading of Item order by an filter Data Grid Parameters: {currentDataGridParameters.ToString()}");
                // get a list of Order bys and filters
                List<OrderByParameter<Item>> _orderByExpressions = GetOrderByExpressions(currentDataGridParameters.SortParams);
                List<Expression<Func<Item, bool>>> _filterByExpressions = GetFilterByExpressions(currentDataGridParameters.FilterParams);


//--------------------------->>>> here need to think about what we waht to retrieve? Eager or not?



                // start with a basic Linq Query with Eager loading
                IQueryable<Item> _query = _table.Include(itm => itm.ItemCategories)
                                                .Include(itm => itm.ItemAttributes);    // for load of categories and attributes
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
                _appUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all records from ItemRepository: {ex.Message} - Inner Exception {ex.InnerException}");
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
            DbSet<Item>  _table = _context.Set<Item>();

            try
            {
                _logger.LogDebug($"Finding first whole item using predicate: {predicate.ToString()}");
                _item = await _table
                    .Include(itm => itm.ItemAttributes)
                    .Include(itm => itm.ItemAttributeVarieties)
                    .Include(itm => itm.ItemCategories)
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
        #endregion
    }
}
