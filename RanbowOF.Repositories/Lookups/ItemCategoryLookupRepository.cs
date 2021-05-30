using LinqKit;
using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Lookups
{
     public class ItemCategoryLookupRepository : AppRepository<ItemCategoryLookup>, IItemCategoryLookupRepository
    {
        private ApplicationDbContext _context = null;
        private ILoggerManager _logger { get; set; }
        private IAppUnitOfWork _appUnitOfWork { get; set; }
        public ItemCategoryLookupRepository(ApplicationDbContext dbContext, ILoggerManager logger, IAppUnitOfWork appUnitOfWork) : base (dbContext, logger, appUnitOfWork)
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
                _logger.LogDebug($"Getting all records with eager loading of ItemCategoryLookup");
                _items = await _table.Include(icl => icl.ParentCategory).ToListAsync();
            }
            catch (Exception ex)
            {
                _appUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all records from ItemCategoryLookupRepository: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _items;
        }

        List<OrderByParameter<ItemCategoryLookup>> GetOrderByExpressions(List<SortParam> currentSortParams)
        {
            if (currentSortParams == null)
                return null;

            List<OrderByParameter<ItemCategoryLookup>> _orderByExpressions = new List<OrderByParameter<ItemCategoryLookup>>();
            foreach (var col in currentSortParams)
            {
                switch (col.FieldName)
                {
                    case nameof(ItemCategoryLookup.CategoryName):
                        _orderByExpressions.Add(new OrderByParameter<ItemCategoryLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = icl => icl.CategoryName });
                        break;

                    case nameof(ItemCategoryLookup.ParentCategoryId):
                        _orderByExpressions.Add(new OrderByParameter<ItemCategoryLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = icl => icl.ParentCategory.CategoryName });
                        break;

                    case nameof(ItemCategoryLookup.UsedForPrediction):
                        _orderByExpressions.Add(new OrderByParameter<ItemCategoryLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = icl => icl.UsedForPrediction });
                        break;

                    case nameof(ItemCategoryLookup.Notes):
                        _orderByExpressions.Add(new OrderByParameter<ItemCategoryLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = icl => icl.Notes });
                        break;

                    default:
                        break;
                }
            }
            return _orderByExpressions;
        }

        private List<Expression<Func<ItemCategoryLookup, bool>>> GetFilterByExpressions(List<FilterParam> currentFilterParams)
        {
            if (currentFilterParams == null)
                return null;
            List<Expression<Func<ItemCategoryLookup, bool>>> _filterByExpressions = new List<Expression<Func<ItemCategoryLookup, bool>>>();
            foreach (var col in currentFilterParams)
            {
                col.FilterBy = col.FilterBy.ToLower();
                switch (col.FieldName)
                {
                    case nameof(ItemCategoryLookup.CategoryName):
                        _filterByExpressions.Add(icl => icl.CategoryName.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(ItemCategoryLookup.ParentCategoryId):
                        _filterByExpressions.Add(icl => icl.ParentCategory.CategoryName.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(ItemCategoryLookup.UsedForPrediction):
                        bool _IsTrue = ((col.FilterBy.Contains("y", StringComparison.OrdinalIgnoreCase)) || (col.FilterBy.Contains("enable", StringComparison.OrdinalIgnoreCase)));      // assume yes and no / enable or disable
                        _filterByExpressions.Add(icl => (icl.UsedForPrediction == _IsTrue));
                        break;

                    case nameof(ItemCategoryLookup.Notes):
                        _filterByExpressions.Add(icl => icl.Notes.ToLower().Contains(col.FilterBy));
                        break;

                    default:
                        break;
                }
            }
            return _filterByExpressions;
        }
        public async Task<DataGridItems<ItemCategoryLookup>> GetPagedDataEagerWithFilterAndOrderByAsync(DataGridParameters currentDataGridParameters) // (int startPage, int currentPageSize)
        {
            DataGridItems<ItemCategoryLookup> _dataGridData = null;
            DbSet<ItemCategoryLookup> _table = _context.Set<ItemCategoryLookup>();

            try
            {
                _logger.LogDebug($"Getting all records with eager loading of ItemCategoryLookup order by an filter Data Grid Parameters: {currentDataGridParameters.ToString()}");
                // get a list of Order bys and filters
                List<OrderByParameter<ItemCategoryLookup>> _orderByExpressions = GetOrderByExpressions(currentDataGridParameters.SortParams);
                List<Expression<Func<ItemCategoryLookup, bool>>> _filterByExpressions = GetFilterByExpressions(currentDataGridParameters.FilterParams);

                // start with a basic Linq Query with Eager loading


                IQueryable<ItemCategoryLookup> _query = _table.Include(icl => icl.ParentCategory); //  null;//                // using the parameters set up the sort
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
                    _query = _query.OrderBy(icl => icl.ParentCategoryId)
                                .ThenBy(icl => icl.CategoryName);

                // This is functionally comes from
                // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/how-to-use-expression-trees-to-build-dynamic-queries
                // using LinqKit;
                // now the per column filters
                if (!String.IsNullOrEmpty(currentDataGridParameters.CustomFilter) || _filterByExpressions != null)
                {
                    Expression<Func<ItemCategoryLookup, bool>> _queryWhere = PredicateBuilder.New<ItemCategoryLookup>(true);
                    if (!String.IsNullOrEmpty(currentDataGridParameters.CustomFilter))
                    {
                        _queryWhere = _queryWhere.And(icl => ((icl.CategoryName.ToLower().Contains(currentDataGridParameters.CustomFilter))
                                                           || (icl.Notes.ToLower().Contains(currentDataGridParameters.CustomFilter))));

                    }
                    if (_filterByExpressions != null)
                    {
                        foreach (var flt in _filterByExpressions)
                        {
                            _queryWhere = _queryWhere.And(flt);
                        }
                    }
                    _query = _query.Where(_queryWhere);   //  (_queryWhere);
                }
                //now we can add the page stuff - first get the count to return
                _dataGridData = new DataGridItems<ItemCategoryLookup>();
                _dataGridData.TotalRecordCount = _query.Count();
                _query = _query
                   .Skip((currentDataGridParameters.CurrentPage - 1) * currentDataGridParameters.PageSize)
                   .Take(currentDataGridParameters.PageSize);   // take 3 pages at least.
                // and execute
                _dataGridData.Entities = await _query.ToListAsync();

                //    .Where(icl => icl)
                //var _query = _table.Include(icl => icl.ParentCategory);
                //    .OrderBy(icl => icl.ParentCategoryId)
                //    .Where(icl => icl)
                // .OrderBy(icl => icl.ParentCategory.CategoryName);
            }
            catch (Exception ex)
            {
                _appUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all records from ItemCategoryLookupRepository: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _dataGridData;
        }

    }

}
