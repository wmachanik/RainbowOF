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
    public class ItemAttributeLookupRepository : AppRepository<ItemAttributeLookup>, IItemAttributeLookupRepository
    {
        #region Injected Items
        private ApplicationDbContext _Context = null;
        private ILoggerManager _Logger { get; set; }
        private IAppUnitOfWork _AppUnitOfWork { get; set; }
        #endregion

        #region Initialisation
        public ItemAttributeLookupRepository(ApplicationDbContext sourceDbContext, ILoggerManager sourceLogger, IAppUnitOfWork sourceAppUnitOfWork) : base(sourceDbContext, sourceLogger, sourceAppUnitOfWork)
        {
            _Context = sourceDbContext;
            _Logger = sourceLogger;
            _AppUnitOfWork = sourceAppUnitOfWork;
        }
        #endregion

        #region DataGrid Handling
        List<OrderByParameter<ItemAttributeLookup>> GetOrderByExpressions(List<SortParam> currentSortParams)
        {
            if (currentSortParams == null)
                return null;

            List<OrderByParameter<ItemAttributeLookup>> _orderByExpressions = new List<OrderByParameter<ItemAttributeLookup>>();
            foreach (var col in currentSortParams)
            {
                switch (col.FieldName)
                {
                    case nameof(ItemAttributeLookup.AttributeName):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = ial => ial.AttributeName });
                        break;

                    case nameof(ItemAttributeLookup.OrderBy):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = ial => ial.OrderBy });
                        break;

                    case nameof(ItemAttributeLookup.ItemAttributeVarietyLookups):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = ial => ial.ItemAttributeVarietyLookups.Count });
                        break;

                    case nameof(ItemAttributeLookup.Notes):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = ial => ial.Notes });
                        break;

                    default:
                        break;
                }
            }
            return _orderByExpressions;
        }

        private List<Expression<Func<ItemAttributeLookup, bool>>> GetFilterByExpressions(List<FilterParam> currentFilterParams)
        {
            if (currentFilterParams == null)
                return null;
            List<Expression<Func<ItemAttributeLookup, bool>>> _filterByExpressions = new List<Expression<Func<ItemAttributeLookup, bool>>>();
            foreach (var col in currentFilterParams)
            {
                col.FilterBy = col.FilterBy.ToLower();
                switch (col.FieldName)
                {
                    case nameof(ItemAttributeLookup.AttributeName):
                        _filterByExpressions.Add(ial => ial.AttributeName.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(ItemAttributeLookup.OrderBy):
                        _filterByExpressions.Add(ial => ial.OrderBy.ToString().ToLower().Contains(col.FilterBy));
                        break;

                    //case nameof(ItemAttributeLookup.ItemAttributeVarietyLookups):
                    //    _filterByExpressions.Add(ial => ial.ItemAttributeVarietyLookups.Select(ial => ial.ItemAttributeLookup).Where(iavl => iavl.VarietyName.ToLower().Contains(col.FilterBy))) ;
                    //    //bool _IsTrue = ((col.FilterBy.Contains("y", StringComparison.OrdinalIgnoreCase)) || (col.FilterBy.Contains("enable", StringComparison.OrdinalIgnoreCase)));      // assume yes and no / enable or disable
                    //    //_filterByExpressions.Add(ial => (ial.UsedForPrediction == _IsTrue));
                    //   break;

                    case nameof(ItemAttributeLookup.Notes):
                        _filterByExpressions.Add(ial => ial.Notes.ToLower().Contains(col.FilterBy));
                        break;

                    default:
                        break;
                }
            }
            return _filterByExpressions;
        }
        public async Task<DataGridItems<ItemAttributeLookup>> GetPagedDataEagerWithFilterAndOrderByAsync(DataGridParameters currentDataGridParameters) // (int startPage, int currentPageSize)
        {
            DataGridItems<ItemAttributeLookup> _dataGridData = null;
            DbSet<ItemAttributeLookup> _table = _Context.Set<ItemAttributeLookup>();

            try
            {
                _Logger.LogDebug($"Getting all records with eager loading of ItemAttributeLookup order by an filter Data Grid Parameters: {currentDataGridParameters.ToString()}");
                // get a list of Order bys and filters
                List<OrderByParameter<ItemAttributeLookup>> _orderByExpressions = GetOrderByExpressions(currentDataGridParameters.SortParams);
                List<Expression<Func<ItemAttributeLookup, bool>>> _filterByExpressions = GetFilterByExpressions(currentDataGridParameters.FilterParams);

                // start with a basic Linq Query with Eager loading
                IQueryable<ItemAttributeLookup> _query = _table.Include(ial => ial.ItemAttributeVarietyLookups.Take(AppUnitOfWork.CONST_MAX_DETAIL_PAGES));    //only take the first 50
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
                    _query = _query.OrderBy(ial => ial.OrderBy);

                //This is functionally comes from
                //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/how-to-use-expression-trees-to-build-dynamic-queries
                //now the per column filters
                if (!String.IsNullOrEmpty(currentDataGridParameters.CustomFilter) || _filterByExpressions != null)
                {
                    Expression<Func<ItemAttributeLookup, bool>> _queryWhere = PredicateBuilder.New<ItemAttributeLookup>(true);
                    if (!String.IsNullOrEmpty(currentDataGridParameters.CustomFilter))
                    {
                        _queryWhere = _queryWhere.And(ial => ((ial.AttributeName.ToLower().Contains(currentDataGridParameters.CustomFilter))
                                                           || (ial.Notes.ToLower().Contains(currentDataGridParameters.CustomFilter))));
                    }
                    if (_filterByExpressions != null)
                    {
                        foreach (var flt in _filterByExpressions)
                            _queryWhere = _queryWhere.And(flt);
                    }
                    _query = _query.Where(_queryWhere);   //  (_queryWhere);
                }

                //now we can add the page stuff - first get the count to return
                _dataGridData = new DataGridItems<ItemAttributeLookup>();
                _dataGridData.TotalRecordCount = _query.Count();
                _query = _query
                   .Skip((currentDataGridParameters.CurrentPage - 1) * currentDataGridParameters.PageSize)
                   .Take(currentDataGridParameters.PageSize);   // take 3 pages at least.
                // and execute
                _dataGridData.Entities = await _query.ToListAsync();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all records from ItemAttributeLookupRepository: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _dataGridData;
        }
        #endregion

    }

}
