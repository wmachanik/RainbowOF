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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Lookups
{
    public class ItemAttributeVarietyLookupRepository : AppRepository<ItemAttributeVarietyLookup>, IItemAttributeVarietyLookupRepository
    {
        private ApplicationDbContext _Context = null;
        private ILoggerManager _Logger { get; set; }
        private IAppUnitOfWork _AppUnitOfWork { get; set; }
        public ItemAttributeVarietyLookupRepository(ApplicationDbContext sourceDbContext, ILoggerManager sourceLogger, IAppUnitOfWork sourceAppUnitOfWork) : base(sourceDbContext, sourceLogger, sourceAppUnitOfWork)
        {
            _Context = sourceDbContext;
            _Logger = sourceLogger;
            _AppUnitOfWork = sourceAppUnitOfWork;
        }

        List<OrderByParameter<ItemAttributeVarietyLookup>> GetOrderByExpressions(List<SortParam> currentSortParams)
        {
            if (currentSortParams == null)
                return null;

            List<OrderByParameter<ItemAttributeVarietyLookup>> _orderByExpressions = new List<OrderByParameter<ItemAttributeVarietyLookup>>();
            foreach (var col in currentSortParams)
            {
                switch (col.FieldName)
                {
                    case nameof(ItemAttributeVarietyLookup.VarietyName):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeVarietyLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = iavl => iavl.VarietyName });
                        break;

                    case nameof(ItemAttributeVarietyLookup.UoM):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeVarietyLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = iavl => iavl.UoM.UoMName});
                        break;

                    case nameof(ItemAttributeVarietyLookup.Symbol):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeVarietyLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = iavl => iavl.Symbol });
                        break;

                    case nameof(ItemAttributeVarietyLookup.FGColour):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeVarietyLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = iavl => iavl.FGColour });
                        break;

                    case nameof(ItemAttributeVarietyLookup.BGColour):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeVarietyLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = iavl => iavl.BGColour });
                        break;

                    case nameof(ItemAttributeVarietyLookup.SortOrder):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeVarietyLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = iavl => iavl.SortOrder });
                        break;

                    case nameof(ItemAttributeVarietyLookup.Notes):
                        _orderByExpressions.Add(new OrderByParameter<ItemAttributeVarietyLookup>() { IsAscending = col.Direction == Blazorise.SortDirection.Ascending, OrderByExperssion = iavl => iavl.Notes });
                        break;

                    default:
                        break;
                }
            }
            return _orderByExpressions;
        }

        private List<Expression<Func<ItemAttributeVarietyLookup, bool>>> GetFilterByExpressions(List<FilterParam> currentFilterParams)
        {
            if (currentFilterParams == null)
                return null;
            List<Expression<Func<ItemAttributeVarietyLookup, bool>>> _filterByExpressions = new List<Expression<Func<ItemAttributeVarietyLookup, bool>>>();
            foreach (var col in currentFilterParams)
            {
                col.FilterBy = col.FilterBy.ToLower();
                switch (col.FieldName)
                {
                    case nameof(ItemAttributeVarietyLookup.VarietyName):
                        _filterByExpressions.Add(iavl => iavl.VarietyName.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(ItemAttributeVarietyLookup.UoM):
                        _filterByExpressions.Add(iavl => iavl.UoM.ToString().ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(ItemAttributeVarietyLookup.SortOrder):
                        _filterByExpressions.Add(iavl => iavl.SortOrder.ToString().ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(ItemAttributeVarietyLookup.Symbol):
                        _filterByExpressions.Add(iavl => iavl.Symbol.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(ItemAttributeVarietyLookup.FGColour):
                        _filterByExpressions.Add(iavl => iavl.FGColour.ToLower().Contains(col.FilterBy));
                        break;

                    case nameof(ItemAttributeVarietyLookup.BGColour):
                        _filterByExpressions.Add(iavl => iavl.BGColour.ToLower().Contains(col.FilterBy));
                        break;

                    //case nameof(ItemAttributeVarietyLookup.ItemAttributeVarietyVarietyLookups):
                    //    _filterByExpressions.Add(iavl => iavl.ItemAttributeVarietyVarietyLookups.Select(iavl => iavl.ItemAttributeVarietyLookup).Where(iavl => iavl.VarietyName.ToLower().Contains(col.FilterBy))) ;
                    //    //bool _IsTrue = ((col.FilterBy.Contains("y", StringComparison.OrdinalIgnoreCase)) || (col.FilterBy.Contains("enable", StringComparison.OrdinalIgnoreCase)));      // assume yes and no / enable or disable
                    //    //_filterByExpressions.Add(iavl => (iavl.UsedForPrediction == _IsTrue));
                    //   break;

                    case nameof(ItemAttributeVarietyLookup.Notes):
                        _filterByExpressions.Add(iavl => iavl.Notes.ToLower().Contains(col.FilterBy));
                        break;

                    default:
                        break;
                }
            }
            return _filterByExpressions;
        }
        public async Task<DataGridItems<ItemAttributeVarietyLookup>> GetPagedDataEagerWithFilterAndOrderByAsync(DataGridParameters currentDataGridParameters, Guid sourceParentItemAttributeLookupId) // (int startPage, int currentPageSize)
        {
            DataGridItems<ItemAttributeVarietyLookup> _dataGridData = null;
            DbSet<ItemAttributeVarietyLookup> _table = _Context.Set<ItemAttributeVarietyLookup>();

            try
            {
                _Logger.LogDebug($"Getting all records with eager loading of ItemAttributeVarietyLookup order by an filter Data Grid Parameters: {currentDataGridParameters.ToString()}");
                if (_AppUnitOfWork.DBTransactionIsStillRunning())
                    _Logger.LogDebug("Second transaction started before current transaction completed!");

                // get a list of Order bys and filters
                List<OrderByParameter<ItemAttributeVarietyLookup>> _orderByExpressions = GetOrderByExpressions(currentDataGridParameters.SortParams);
                List<Expression<Func<ItemAttributeVarietyLookup, bool>>> _filterByExpressions = GetFilterByExpressions(currentDataGridParameters.FilterParams);

                // start with a basic Linq Query. No Eager loading getting weird issues which I cannot solve.
                IQueryable<ItemAttributeVarietyLookup> _query = _table.Where(iavl => iavl.ItemAttributeLookupId == sourceParentItemAttributeLookupId); //.Include(iavl=>iavl.UoM) ;  -> this seems to cause problems with building the query, perhaps it must come last 
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
                    _query = _query.OrderBy(iavl => iavl.VarietyName);

                //This is functionally comes from
                //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/how-to-use-expression-trees-to-build-dynamic-queries
                //now the per column filters
                if (!String.IsNullOrEmpty(currentDataGridParameters.CustomFilter) || _filterByExpressions != null)
                {
                    Expression<Func<ItemAttributeVarietyLookup, bool>> _queryWhere = PredicateBuilder.New<ItemAttributeVarietyLookup>(true);
                    if (!String.IsNullOrEmpty(currentDataGridParameters.CustomFilter))
                    {
                        _queryWhere = _queryWhere.And(iavl => ((iavl.VarietyName.ToLower().Contains(currentDataGridParameters.CustomFilter))
                                                           || (iavl.UoM.UoMName.ToLower().Contains(currentDataGridParameters.CustomFilter))
                                                           || (iavl.Notes.ToLower().Contains(currentDataGridParameters.CustomFilter))));
                    }
                    if (_filterByExpressions != null)
                    {
                        foreach (var flt in _filterByExpressions)
                            _queryWhere = _queryWhere.And(flt);
                    }
                    _query = _query.Where(_queryWhere);   //  (_queryWhere);
                }

                //now we can add the page stuff - first get the count to return
                _dataGridData = new DataGridItems<ItemAttributeVarietyLookup>();
                _dataGridData.TotalRecordCount = _query.Count();
                
                _query = _query
                   .Skip((currentDataGridParameters.CurrentPage - 1) * currentDataGridParameters.PageSize)
                   .Take(currentDataGridParameters.PageSize);
                // and execute
                _dataGridData.Entities = await _query.ToListAsync();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all records from ItemAttributeVarietyLookupRepository: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _dataGridData;
        }

    }

}
