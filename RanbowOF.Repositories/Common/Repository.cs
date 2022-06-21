using RainbowOF.Data.SQL;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        #region Privates
        internal ApplicationDbContext appContext { get; set; }  // = null;
        internal DbSet<TEntity> appDbSet = null;
        internal ILoggerManager appLoggerManager { get; }
        internal IUnitOfWork appUnitOfWork { get; set; }
        #endregion
        #region Initialise
        /// should not need this had it her to stop the error: "There is no argument given that corresponds to the required formal parameter"
        /// if it is asking for that you need to ass  : base (dbContext, logger, UnitOfWork) to the definition 
        //public AppRepository() { }
        public Repository(ApplicationDbContext dbContext,
                          ILoggerManager logger,
                          IUnitOfWork unitOfWork)
        {
            appContext = dbContext;
            appDbSet = dbContext.Set<TEntity>();
            appLoggerManager = logger;
            appUnitOfWork = unitOfWork;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"AppRepository Initialised with type {typeof(TEntity).Name}.");
        }
        #endregion
        #region Public Access to Privates
        //public ApplicationDbContext appDbContext { get { return appContext; } set { appContext = value; } }
        public ApplicationDbContext AppDbContext
        {
            get
            {
                return appContext;
            }
        }
        #endregion
        #region Generic DBSet stuff
        public async Task<int> CountAsync()
        {
            return await appDbSet.CountAsync();
        }
        public int Add(TEntity newEntity)
        {
            int _added = UnitOfWork.CONST_WASERROR;    // return if error
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Adding entity: {newEntity.ToString()}");
            appUnitOfWork.BeginTransaction();
            try
            {
                appContext.Entry(newEntity).State = EntityState.Added;
                //dbSet.Add(newEntity);
                _added = appUnitOfWork.Complete();  // Save();
                //appContext.Database.CommitTransaction();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Adding Item: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _added;
        }
        public async Task<TEntity> AddAsync(TEntity newEntity)
        {
            int _added = UnitOfWork.CONST_WASERROR;    // return if error
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Adding entity (async) type: {newEntity.GetType().Name} -- begin.");
            try
            {
                appUnitOfWork.BeginTransaction();
                //appContext.Entry(newEntity).State = EntityState.Added;
                newEntity = (await appDbSet.AddAsync(newEntity)).Entity;
                _added = await appUnitOfWork.CompleteAsync();  // Save
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Adding Item (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Adding entity (async): {newEntity.GetType().Name} -- end.");
            if (_added == UnitOfWork.CONST_WASERROR)
                return null;
            else
                return newEntity;
        }
        public async Task<int> AddRangeAsync(List<TEntity> newEntities)
        {
            int _added = UnitOfWork.CONST_WASERROR;    // return if error
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Adding range (async): {newEntities.GetType().Name} - begin");
            try
            {
                appUnitOfWork.BeginTransaction();
                foreach (var newEntity in newEntities)
                {
                    appContext.Entry(newEntity).State = EntityState.Added;
                }
                await appDbSet.AddRangeAsync(newEntities);     // !!!!!!   id field is set once is save by EF - no need to set it
                _added = await appUnitOfWork.CompleteAsync();  // Save
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Adding range (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Adding range (async): {newEntities.GetType().Name} - end");
            return _added;
        }
        public int DeleteByPrimaryId(object sourceId)
        {
            int _deleted = UnitOfWork.CONST_WASERROR;    // return if error
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Deleting by id entity: {sourceId.ToString()}");
            appUnitOfWork.BeginTransaction();
            try
            {
                TEntity _entity = GetById(sourceId);
                if (_entity != null)
                {
                    appContext.Entry(_entity).State = EntityState.Deleted;
                    appDbSet.Remove(_entity);
                    _deleted = appUnitOfWork.Complete();  // Save();
                }

            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Deleting Item: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }

            return _deleted;
        }
        public async Task<int> DeleteByPrimaryIdAsync(object sourceId)
        {
            int _deleted = UnitOfWork.CONST_WASERROR;    // return if error
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Deleting by id entity (async): {sourceId.GetType().FullName} -- begin");
            try
            {
                TEntity _entity = await GetByIdAsync(sourceId);
                if (_entity != null)
                {
                    appUnitOfWork.BeginTransaction();
                    //appContext.Entry(_entity).State = EntityState.Deleted;  -- this line does what the next line does.
                    appDbSet.Remove(_entity);
                    _deleted = await appUnitOfWork.CompleteAsync();  // Save();
                }
            }
            catch (Exception ex)
            {
                appLoggerManager.LogError($"!!!Error Deleting entity (async): {ex.Message} - Inner Exception {ex.InnerException}");
                appUnitOfWork.RollbackTransaction();
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Deleting by id entity (async): {sourceId.GetType().FullName} -- end");
            return _deleted;
        }
        public int DeleteBy(Expression<Func<TEntity, bool>> predicate)
        {
            int _deleted = UnitOfWork.CONST_WASERROR;    // return if error
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Deleting entity by: {predicate.ToString()}");
            appUnitOfWork.BeginTransaction();
            try
            {
                var _entity = appDbSet.Find(predicate);
                if (_entity != null)
                {
                    //appContext.Entry(_entity).State = EntityState.Deleted;  -- this line does what the next line does.
                    appDbSet.Remove(_entity);
                    _deleted = appUnitOfWork.Complete();  // Save();
                }

            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Deleting By: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _deleted;
        }
        public async Task<int> DeleteByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            int _deleted = UnitOfWork.CONST_WASERROR;    // return if error
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Deleting entity by (async): {predicate.ToString()} -- begin");
            try
            {
                var _entity = await appDbSet.FirstOrDefaultAsync(predicate);
                if (_entity != null)
                {
                    appUnitOfWork.BeginTransaction();
                    appContext.Entry(_entity).State = EntityState.Deleted;
                    appDbSet.Remove(_entity);
                    _deleted = await appUnitOfWork.CompleteAsync();  // Save();
                }
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Deleting By (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Deleting entity by (async): {predicate.ToString()} -- end");
            return _deleted;
        }
        public TEntity FindFirst()
        {
            TEntity _entity = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Finding First entity of type: {typeof(TEntity)}");

            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                var _query = appDbSet
                       .AsNoTracking();
                _entity = _query.FirstOrDefault();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public TEntity FindFirstBy(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity _entity = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Finding First {predicate.ToString()} entity of type: {typeof(TEntity)}");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                var _query = appDbSet
                       .AsNoTracking();
                _entity = _query.FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first predicate entity: {ex.Message} - Inner Exception {ex.InnerException}");

#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public async Task<TEntity> FindFirstAsync()
        {
            TEntity _entity = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Finding First entity of type: {typeof(TEntity)} -- begin");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                var _query = appDbSet
                       .AsNoTracking();
                _entity = await _query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Finding First entity of type: {typeof(TEntity)} -- end");
            return _entity;
        }
        public async Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity _entity = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Finding First {predicate.ToString()} entity of type: {typeof(TEntity)} - begin");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                var _query = appDbSet
                       .AsNoTracking();
                _entity = await _query.FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                string errorMessage = $"!!!Error Finding first predicate entity: {ex.Message} - Inner Exception {ex.InnerException}";
                appUnitOfWork.LogAndSetErrorMessage(errorMessage);
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Finding First {predicate.ToString()} entity of type: {typeof(TEntity)} - end");
            return _entity;
        }
        public IEnumerable<TEntity> GetAll()
        {
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Getting all records in Table of type: {typeof(TEntity)}");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                return appDbSet.AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public IEnumerable<TEntity> GetAllOrderBy(Func<TEntity, object> orderByExpression, bool sortDesc = false)
        {
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Getting all records in Table of type: {typeof(TEntity)} order by {orderByExpression}");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                var query = appDbSet.AsNoTracking();
                var _result = sortDesc
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);
                return _result.ToList();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            List<TEntity> _result = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Getting all records (async) in Table of type: {typeof(TEntity)} -- begin");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                _result = await appDbSet
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Getting all records (async) in Table of type: {typeof(TEntity)} -- end");
            return _result;
        }
        public async Task<IEnumerable<TEntity>> GetAllOrderByAsync<TKey>(Expression<Func<TEntity, TKey>> orderByExpression, bool sortDesc = false)
        {
            List<TEntity> _result = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Getting all records (async) in Table of type: {typeof(TEntity)} order by {orderByExpression} -- begin");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                var query = appDbSet.AsNoTracking();
                var orderedQyuery = sortDesc
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);
                _result  = await orderedQyuery.ToListAsync();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async) order by: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Getting all records (async) in Table of type: {typeof(TEntity)} order by {orderByExpression} -- end");
            return _result;
        }
        public async Task<IEnumerable<TEntity>> GetAllEagerAsync(params Expression<Func<TEntity, object>>[] properties)
        {
            List<TEntity> _result = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By all eager loading (async) {properties.ToString()} from table of type: {typeof(TEntity)} -- begin");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            var query = appDbSet as IQueryable<TEntity>; // _dbSet = dbContext.Set<TEntity>()
            query = properties
                       .Aggregate(query, (current, property) => current.Include(property))
                       .AsNoTracking();
            try
            {
                _result = await query.ToListAsync();    // AsNoTracking().ToList();    //read only
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error eager loading: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By all eager loading (async) {properties.ToString()} from table of type: {typeof(TEntity)} -- end");
            return _result;
        }
        public async Task<IEnumerable<TEntity>> GetPagedEagerAsync(int startPage, int currentPageSize, params Expression<Func<TEntity, object>>[] properties)
        {
            List<TEntity> _result = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By all eager loading (async) {properties.ToString()} from table of type: {typeof(TEntity)} -- begin");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            var query = appDbSet as IQueryable<TEntity>; // _dbSet = dbContext.Set<TEntity>()
            query = properties
                       .Aggregate(query, (current, property) => current.Include(property))
                       .AsNoTracking();
            try
            {
                _result = await query
                    .Skip(startPage * currentPageSize)
                    .Take(currentPageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error eager loading: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By all eager loading (async) {properties.ToString()} from table of type: {typeof(TEntity)} -- begin");
            return _result;
        }
        public IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate)
        {
            List<TEntity> _result = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By all {predicate.ToString()} from table of type: {typeof(TEntity)} -- begin");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                _result = appDbSet.Where(predicate)
                       .AsNoTracking()
                       .ToList();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting by: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By all {predicate.ToString()} from table of type: {typeof(TEntity)} -- end");
            return _result;
        }
        public async Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            IEnumerable<TEntity> _rows = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By all (async) {predicate.ToString()} from table of type: {typeof(TEntity)} -- begin");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                _rows = await appDbSet.Where(predicate)
                       .AsNoTracking()
                       .ToListAsync();
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By all (async) table {nameof(appDbSet)} returned: {_rows.Count()} rows");
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting by (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By all (async) {predicate.ToString()} from table of type: {typeof(TEntity)} -- end");
            return _rows;
        }

        public TEntity GetById(object Id)
        {
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By Id (async) {Id.ToString()} from table of type: {typeof(TEntity)}");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                return appDbSet.Find(Id);
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Get by Id: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<TEntity> GetByIdAsync(object Id)
        {
            TEntity _result = null;
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By Id (async) {Id.ToString()} from table of type: {typeof(TEntity)} -- begin");
            if (appUnitOfWork.DBTransactionIsStillRunning())
                if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                appContext.ChangeTracker.LazyLoadingEnabled = true;
                _result = await appDbSet.FindAsync(Id);
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Get by Id (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Get By Id (async) {Id.ToString()} from table of type: {typeof(TEntity)} -- end");
            return _result;
        }
        //public async Task<bool> Save()
        //{
        //    var _saved = false;

        //    try
        //    {
        //        _saved = (await appContext.SaveChangesAsync() > 0);
        //    }
        //    catch (Exception ex)
        //    {
        //        appLoggerManager.LogError($"!!!Error Saving: {ex.Message} - Inner Exception {ex.InnerException}");
        //    }

        //    return (_saved);
        //}

        //public async Task<int> Update(TEntity updatedEntity)
        public int Update(TEntity updatedEntity)
        {
            int _updated = UnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Updating entity: {updatedEntity.ToString()}");
            appUnitOfWork.BeginTransaction();
            try
            {
                appContext.Entry(updatedEntity).State = EntityState.Modified;
                appDbSet.Update(updatedEntity);
                _updated = appUnitOfWork.Complete();  // Save();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _updated;
        }
        public async Task<int> UpdateAsync(TEntity updatedEntity)
        {
            int _updated = UnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Updating entity (async): {updatedEntity.ToString()}");
            try
            {
                appUnitOfWork.BeginTransaction();
                //appContext.Entry(updatedEntity).State = EntityState.Modified;
                //_Table.Attach   (updatedEntity);
                appContext.Entry(updatedEntity).State = EntityState.Detached;
                appDbSet.Update(updatedEntity);
                _updated = await appUnitOfWork.CompleteAsync();  // Save();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _updated;
        }

        public async Task<int> UpdateRangeAsync(List<TEntity> updateEntities)
        {
            int _updated = UnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Updating range entity (async): {updateEntities.ToString()} -- begin");
            try
            {
                appUnitOfWork.BeginTransaction();
                appContext.Entry(updateEntities).State = EntityState.Modified;
                appDbSet.UpdateRange(updateEntities);
                _updated = await appUnitOfWork.CompleteAsync();  // Save();
            }
            catch (Exception ex)
            {
                appUnitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug($"Updating range entity (async): {updateEntities.ToString()} -- end");
            return _updated;
        }
        #endregion
    }
}
