using Blazorise;
using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        #region Privates & Publics
        //        internal DbSet<TEntity> CurrentDbSet { get; set; } // = null;
        internal ILoggerManager AppLoggerManager { get; }
        internal IUnitOfWork AppUnitOfWork { get; set; }
        internal IDbContextFactory<ApplicationDbContext> AppDbContext { get; set; }
        #endregion
        #region Initialise
        /// should not need this had it her to stop the error: "There is no argument given that corresponds to the required formal parameter"
        /// if it is asking for that you need to ass  : base (dbContext, logger, UnitOfWork) to the definition 
        //public AppRepository() { }
        public Repository(IDbContextFactory<ApplicationDbContext> context,
                          ILoggerManager logger,
                          IUnitOfWork unitOfWork)
        {
            AppDbContext = context;
            AppLoggerManager = logger;
            AppUnitOfWork = unitOfWork;
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"AppRepository Initialised with type {typeof(TEntity).Name}.");
        }
        #endregion
        #region Public Access to Privates
        //public ApplicationDbContext appDbContext { get { return appDbContext; } set { appDbContext = value; } }
        //private ApplicationDbContext _appDbContext; // = null;
        //public ApplicationDbContext AppDbContext
        //{
        //    get { return _appDbContext; }
        //    set { _appDbContext = value; }
        //}
        #endregion
        #region Generic DBSet stuff
        public async Task<int> CountAsync()
        {
            using var context = AppDbContext.CreateDbContext();
            var _table = context.Set<TEntity>();
            return await _table.CountAsync();
        }
        /// <summary>
        /// The below handle if a transaction is currently running to prevent re-entry of DBcontext.
        /// </summary>
        private bool _transactionIsBusy = false;
        public bool CanDoDbAsyncCall(string startString)
        {
            if (_transactionIsBusy)
            {
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"{startString} -- called when an Async call is already busy.");
                return false;
            }
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"{startString} -> begin.");
            _transactionIsBusy = true;
            return true;
        }
        public void DbCallDone(string endString)
        {
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"{endString} -> end.");
            _transactionIsBusy = false;
        }
        public int Add(TEntity newEntity)
        {
            int _added = UnitOfWork.CONST_WASERROR;    // return if error
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Adding entity: {newEntity}");
            try
            {
                using var context = AppDbContext.CreateDbContext();
                var _table = context.Set<TEntity>();
                //contextFactory.Entry(newEntity).State = EntityState.Added;  --> only use if not tracking changes, since it marks only the parent as changed
                _table.Add(newEntity);   // use this method since it tracks changes
                _added = context.SaveChanges();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Adding Item: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _added;
        }
        public async Task<TEntity> AddAsync(TEntity newEntity)
        {
            int _added = UnitOfWork.CONST_WASERROR;
            string statusString = $"Adding entity (async) type: {newEntity.GetType().Name}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                var _table = context.Set<TEntity>();
                //contextFactory.Entry(newEntity).State = EntityState.Added;  -> removed since if adding children will only mark the one entry as new
                var _result = await _table.AddAsync(newEntity);
                newEntity = _result.Entity;
                _added = await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Adding Item (async): {ex.Message} - Inner Exception {ex.InnerException}");

#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            if (_added == UnitOfWork.CONST_WASERROR) return null;
                                                else return newEntity;
        }
        public async Task<int> AddRangeAsync(List<TEntity> newEntities)
        {
            int _added = UnitOfWork.CONST_WASERROR;    // return if error
            string statusString = $"Adding range (async): {newEntities.GetType().Name}";
            if (!CanDoDbAsyncCall(statusString))
                return _added;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                var _table = context.Set<TEntity>();
                foreach (var newEntity in newEntities)
                {
                    context.Entry(newEntity).State = EntityState.Added;
                }
                await context.AddRangeAsync(newEntities);     // !!!!!!   id field is set once is save by EF - no need to set it
                _added = await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Adding range (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _added;
        }
        public int DeleteByEntity(TEntity sourceEntity)
        {
            int _deleted = UnitOfWork.CONST_WASERROR;    // return if error
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Deleting by id entity: {sourceEntity.ToString}");
            try
            {
                //TEntity _entity = GetById(sourceId);  --> 
                using var context = AppDbContext.CreateDbContext();
                // appDbContext.Entry(_entity).State = EntityState.Deleted; //--> removed as only affects parent not sub classes / children
                context.Remove(sourceEntity);
                _deleted = context.SaveChanges();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Deleting Item: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }

            return _deleted;
        }
        public async Task<int> DeleteByEntityAsync(TEntity sourceEntity)
        {
            int _deleted = UnitOfWork.CONST_WASERROR;    // return if error
            string statusString = $"Deleting by id entity (async): {sourceEntity.ToString}";
            if (!CanDoDbAsyncCall(statusString))
                return _deleted;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                // AppUnitOfWork.AppDbContext.Entry(_entity).State = EntityState.Deleted; //--> removed as only affects parent not sub classes / children
                context.Remove(sourceEntity);
                _deleted = await context.SaveChangesAsync();
                //TEntity _entity = await GetByIdAsync(sourceId);
                //if (_entity != null)
                //{
                //    AppUnitOfWork.BeginTransaction();
                //    //AppUnitOfWork.AppDbContext.Entry(_entity).State = EntityState.Deleted;  // With No Tracking enabled we need this 
                //    AppUnitOfWork.AppDbContext.Remove(_entity);
                //    _deleted = await AppUnitOfWork.CompleteAsync();  // Save();
                //}
            }
            catch (Exception ex)
            {
                _deleted = UnitOfWork.CONST_WASERROR;
                AppLoggerManager.LogError($"!!!Error Deleting entity (async): {ex.Message} - Inner Exception {ex.InnerException}");
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
        public int DeleteBy(Expression<Func<TEntity, bool>> predicate)
        {
            int _deleted = UnitOfWork.CONST_WASERROR;    // return if error
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Deleting entity by: {predicate}");
            try
            {
                using var context = AppDbContext.CreateDbContext();
                var _table = context.Set<TEntity>();
                var _entity = _table.Where(predicate);
                if (_entity != null)
                {
                    // AppUnitOfWork.AppDbContext.Entry(_entity).State = EntityState.Deleted;  // With No Tracking enabled we need this 
                    context.Remove(_entity);
                    _deleted = context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _deleted = UnitOfWork.CONST_WASERROR;
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Deleting By: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _deleted;
        }
        public async Task<int> DeleteByAsync(Expression<Func<TEntity, bool>> predicate)

        {
            int _deleted = UnitOfWork.CONST_WASERROR;    // return if error
            string statusString = $"Deleting entity by (async): {predicate}";
            if (!CanDoDbAsyncCall(statusString))
                return _deleted;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                var _table = context.Set<TEntity>();
                var _entity = await _table.FirstOrDefaultAsync(predicate);
                if (_entity != null)
                {
                    // AppUnitOfWork.AppDbContext.Entry(_entity).State = EntityState.Deleted;
                    context.Remove(_entity);
                    _deleted = await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Deleting By (async): {ex.Message} - Inner Exception {ex.InnerException}");
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
        public TEntity FindFirst()
        {
            TEntity _entity = null;
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Finding First entity of type: {typeof(TEntity)}");
            try
            {
                using var context = AppDbContext.CreateDbContext();
                var _table = context.Set<TEntity>();
                _entity = _table.FirstOrDefault();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public TEntity FindFirstBy(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity _entity = null;
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Finding First {predicate} entity of type: {typeof(TEntity)}");
            if (AppUnitOfWork.DBTransactionIsStillRunning())
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                using var context = AppDbContext.CreateDbContext();
                var _table = context.Set<TEntity>();
                _entity = _table.FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first predicate entity: {ex.Message} - Inner Exception {ex.InnerException}");

#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public async Task<TEntity> FindFirstAsync()
        {
            TEntity _entity = null;
            string statusString = $"Finding First entity of type: {typeof(TEntity)}";
            if (!CanDoDbAsyncCall(statusString))
                return _entity;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                var _table = context.Set<TEntity>();
                _entity = await _table.FirstOrDefaultAsync();
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
            return _entity;
        }
        public async Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity _entity = null;
            string statusString = $"Finding First {predicate} entity of type: {typeof(TEntity)}";
            if (!CanDoDbAsyncCall(statusString))
                return _entity;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                var _table = context.Set<TEntity>();
                _entity = await _table.FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                string errorMessage = $"!!!Error Finding first predicate entity: {ex.Message} - Inner Exception {ex.InnerException}";
                AppUnitOfWork.LogAndSetErrorMessage(errorMessage);
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _entity;
        }
        public IEnumerable<TEntity> GetAll()
        {
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Getting all records in Table of type: {typeof(TEntity)}");
            try
            {
                using var context = AppDbContext.CreateDbContext();
                var _table = context.Set<TEntity>();
                return _table
                    //.AsNoTracking() --->>> probably not needed since context is disposed// done here because it is generic and we cannot be sure it is not going to conflict 
                    .ToList();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public IEnumerable<TEntity> GetAllOrderBy(Func<TEntity, object> orderByExpression, bool sortDesc = false)
        {
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Getting all records in Table of type: {typeof(TEntity)} order by {orderByExpression}");
            try
            {
                using var context = AppDbContext.CreateDbContext();
                var _table = context.Set<TEntity>();
                //.AsNoTracking(); // done here because it is generic and we cannot be sure it is not going to conflict 
                var _result = sortDesc
                    ? _table.OrderByDescending(orderByExpression)
                    : _table.OrderBy(orderByExpression);
                return _result
                    .ToList();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            List<TEntity> _result = null;
            string statusString = $"Getting all records (async) in Table of type: {typeof(TEntity)}";
            if (!CanDoDbAsyncCall(statusString))
                return _result;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                var _table = context.Set<TEntity>();
                _result = await _table     //.AsNoTracking(); not here must be specific to implementation
                  .ToListAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async): {ex.Message} - Inner Exception {ex.InnerException}");
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
        public async Task<IEnumerable<TEntity>> GetAllOrderByAsync<TKey>(Expression<Func<TEntity, TKey>> orderByExpression, bool sortDesc = false)
        {
            List<TEntity> _result = null;
            string statusString = $"Getting all records (async) in Table of type: {typeof(TEntity)} order by {orderByExpression}";
            if (!CanDoDbAsyncCall(statusString))
                return _result;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                var _table = context.Set<TEntity>();    //.AsNoTracking(); not here must be specific to implementation
                var orderedQyuery = sortDesc
                    ? _table.OrderByDescending(orderByExpression)
                    : _table.OrderBy(orderByExpression);
                _result = await orderedQyuery.ToListAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async) order by: {ex.Message} - Inner Exception {ex.InnerException}");
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
        public async Task<IEnumerable<TEntity>> GetAllEagerAsync(params Expression<Func<TEntity, object>>[] properties)
        {
            List<TEntity> _result = null;
            if (properties == null)
                return null;
            string statusString = $"Get By all eager loading (async) {properties} from table of type: {typeof(TEntity)}";
            if (!CanDoDbAsyncCall(statusString))
                return _result;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                ///---> not sure the DbSet<TEntity> before the included is correct, but it is the only way it will compile
                var _table = context.Set<TEntity>();
                _table = properties
                           .Aggregate(_table, (current, property) => (DbSet<TEntity>)current.Include(property));    //.AsNoTracking(); not here must be specific to implementation
                _result = await _table.ToListAsync();     //read only
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error eager loading: {ex.Message} - Inner Exception {ex.InnerException}");
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
        public async Task<IEnumerable<TEntity>> GetPagedEagerAsync(int startPage, int currentPageSize, params Expression<Func<TEntity, object>>[] properties)
        {
            List<TEntity> _result = null;
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            string statusString = $"Get By all eager loading (async) {properties} from table of type: {typeof(TEntity)}";
            if (!CanDoDbAsyncCall(statusString))
                return _result;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                var _table = context.Set<TEntity>(); // _dbSet = dbContext.Set<TEntity>()
                _table = properties
                           .Aggregate(_table, (current, property) => (DbSet<TEntity>)current.Include(property));    //.AsNoTracking(); not here must be specific to implementation
                ///---> not sure the DbSet<TEntity> before the included is correct, but it is the only way it will compile
                _result = await _table
                    .Skip(startPage * currentPageSize)
                    .Take(currentPageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error eager loading: {ex.Message} - Inner Exception {ex.InnerException}");
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
        public IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate)
        {
            List<TEntity> _result = null;
            string statusString = $"Get By all {predicate} from table of type: {typeof(TEntity)}";
            if (!CanDoDbAsyncCall(statusString))
                return _result;
            try
            {
                using var context = AppDbContext.CreateDbContext();
                var _table = context.Set<TEntity>();
                _result = _table
                            .Where(predicate)    //.AsNoTracking(); not here must be specific to implementation
                            .ToList();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting by: {ex.Message} - Inner Exception {ex.InnerException}");
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
        public async Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            IEnumerable<TEntity> _result = null;
            string statusString = $"Get By all (async) {predicate} from table of type: {typeof(TEntity)}";
            if (!CanDoDbAsyncCall(statusString))
                return _result;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                var _table = context.Set<TEntity>();
                _result = await _table.Where(predicate)
                       .ToListAsync();
                if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Get By all (async) table {nameof(_table)} returned: {_result.Count()} rows");
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting by (async): {ex.Message} - Inner Exception {ex.InnerException}");
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

        public TEntity GetById(object Id)
        {
            string statusString = $"Get By Id (async) {Id} from table of type: {typeof(TEntity)}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = AppDbContext.CreateDbContext();
                var _table = context.Set<TEntity>(); // as IQueryable<TEntity>;
                return _table.Find(Id);
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Get by Id: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return null;
        }
        public async Task<TEntity> GetByIdAsync(object Id)
        {
            TEntity _result = null;
            string statusString = $"Get By Id (async) {Id} from table of type: {typeof(TEntity)}";
            if (!CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                context.ChangeTracker.LazyLoadingEnabled = true;
                var _table = context.Set<TEntity>();
                _result = await _table
                    .FindAsync(Id);
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Get by Id (async): {ex.Message} - Inner Exception {ex.InnerException}");
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
        //public async Task<bool> Save()
        //{
        //    var _saved = false;

        //    try
        //    {
        //        _saved = (await appDbContext.SaveChangesAsync() > 0);
        //    }
        //    catch (Exception ex)
        //    {
        //        AppLoggerManager.LogError($"!!!Error Saving: {ex.Message} - Inner Exception {ex.InnerException}");
        //    }

        //    return (_saved);
        //}

        //public async Task<int> Update(TEntity updatedEntity)
        public int Update(TEntity updatedEntity)
        {
            int _updated = UnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
            string statusString = $"Updating entity: {updatedEntity}";
            if (!CanDoDbAsyncCall(statusString))
                return _updated;
            try
            {
                using var context = AppDbContext.CreateDbContext();
                //  AppUnitOfWork.AppDbContext.Entry(updatedEntity).State = EntityState.Modified;   //-> if tracking disabled need this
                var _result = context.Update(updatedEntity);
                updatedEntity = _result.Entity;
                _updated = context.SaveChanges();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _updated;
        }
        public async Task<int> UpdateAsync(TEntity updatedEntity)
        {
            string statusString = $"Updating entity (async): {updatedEntity}";
            if (!CanDoDbAsyncCall(statusString))
                return UnitOfWork.CONST_WASERROR;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                // AppDbContext.Entry(updatedEntity).State = EntityState.Modified; //--> only updates this item not the children
                var _result = context.Update(updatedEntity);
                updatedEntity = _result.Entity;
                return await context.SaveChangesAsync();
                // doing this is completeAsync AppDbContext.ChangeTracker.Clear(); //---> added this after item would not update as per: https://stackoverflow.com/questions/36856073/the-instance-of-entity-type-cannot-be-tracked-because-another-instance-of-this-t
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table (async): {ex.Message} - Inner Exception {ex.InnerException}");
                return UnitOfWork.CONST_WASERROR;
            }
            finally
            {
                DbCallDone(statusString);
            }
        }
        //            int _updated = UnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
        //            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"Updating entity (async): {updatedEntity}");
        //            try
        //            {
        //                AppUnitOfWork.BeginTransaction();
        //                //_Table.Attach   (updatedEntity);
        //                //AppDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        //                //AppDbContext.Entry(updatedEntity).State = EntityState.Detached; // -> tracking turned off global in options
        //                _appDbContext.Entry(updatedEntity).State = EntityState.Modified;  // With No Tracking enabled we need this 
        //                AppUnitOfWork.AppDbContext.Update(updatedEntity);
        //                _updated = await AppUnitOfWork.CompleteAsync();  // Save();
        //            }
        //            catch (Exception ex)
        //            {
        //                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table (async): {ex.Message} - Inner Exception {ex.InnerException}");
        //#if DebugMode
        //                throw;     // #Debug?
        //#endif
        //            }
        //            return _updated;
        //        }

        public async Task<int> UpdateRangeAsync(List<TEntity> updateEntities)
        {
            int _updated = UnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
            string statusString = $"Updating range entity (async): {updateEntities}";
            if (!CanDoDbAsyncCall(statusString))
                return _updated;
            try
            {
                using var context = await AppDbContext.CreateDbContextAsync();
                //  AappDbContext.Entry(updateEntities).State = EntityState.Modified;  //-> if tracking is disabled need this
                context.UpdateRange(updateEntities);
                _updated = await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            finally
            {
                DbCallDone(statusString);
            }
            return _updated;
        }
        #endregion
    }
}
