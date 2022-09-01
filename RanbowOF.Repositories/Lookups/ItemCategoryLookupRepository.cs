﻿using LinqKit;
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
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Lookups
{
    public class ItemCategoryLookupRepository : Repository<ItemCategoryLookup>, IItemCategoryLookupRepository
    {
        #region Private vars of parameters
        // these are no internals to the generic repo
        //private IDbContextFactory<ApplicationDbContext> appDbContext { get; set; }
        //private ILoggerManager appLoggerManager { get; set; }
        //private IUnitOfWork appUnitOfWork { get; set; }
        #endregion
        public ItemCategoryLookupRepository(IDbContextFactory<ApplicationDbContext> sourceDbContext, ILoggerManager sourceLogger, IUnitOfWork sourceAppUnitOfWork) : base(sourceDbContext, sourceLogger, sourceAppUnitOfWork)
        {
            //appDbContext = sourceDbContext;
            //appLoggerManager = sourceLogger;
            //appUnitOfWork = sourceAppUnitOfWork;
            if (sourceLogger.IsDebugEnabled()) sourceLogger.LogDebug("ItemCategoryLookupRepository initialised...");
        }

        public async Task<List<ItemCategoryLookup>> GetAllEagerLoadingAsync()
        {
            string statusString = $"Getting all records with eager loading of ItemCategoryLookup";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            List<ItemCategoryLookup> _items = null;
            using var context = await AppDbContext.CreateDbContextAsync();
            DbSet<ItemCategoryLookup> _table = context.Set<ItemCategoryLookup>();

            try
            {
                _items = await _table.Include(icl => icl.ParentCategory).ToListAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all records from ItemCategoryLookupRepository: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _items;
        }

        static List<OrderByParameter<ItemCategoryLookup>> GetOrderByExpressions(List<SortParam> currentSortParams)
        {
            if (currentSortParams == null)
                return null;

            List<OrderByParameter<ItemCategoryLookup>> _orderByExpressions = new();
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

        private static List<Expression<Func<ItemCategoryLookup, bool>>> GetFilterByExpressions(List<FilterParam> currentFilterParams)
        {
            if (currentFilterParams == null)
                return null;
            List<Expression<Func<ItemCategoryLookup, bool>>> _filterByExpressions = new();
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
                        bool _IsTrue = ((col.FilterBy.Contains('y', StringComparison.OrdinalIgnoreCase)) || (col.FilterBy.Contains("enable", StringComparison.OrdinalIgnoreCase)));      // assume yes and no / enable or disable
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
            string statusString = $"Getting all records with eager loading of ItemCategoryLookup order by an filter Data Grid Parameters: {currentDataGridParameters}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            DataGridItems<ItemCategoryLookup> _dataGridData = null;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();

                DbSet<ItemCategoryLookup> _table = context.Set<ItemCategoryLookup>();
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
                    _query = _query.OrderBy(icl => icl.ParentCategory.CategoryName)
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
                _dataGridData = new()
                {
                    TotalRecordCount = _query.Count()
                };
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
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all records from ItemCategoryLookupRepository: {ex.Message} - Inner Exception {ex.InnerException}");
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

    }

}
