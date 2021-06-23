using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public class AppRepository<TEntity> : IAppRepository<TEntity> where TEntity : class
    {
        private ApplicationDbContext _Context { get; set; }  // = null;
        private DbSet<TEntity> _Table = null;
        private ILoggerManager _Logger { get; }
        private IAppUnitOfWork _AppUnitOfWork { get; set; }
        /// should not need this had it her to stop the error: "There is no argument given that corresponds to the required formal parameter"
        /// if it is asking for that you need to ass  : base (dbContext, logger, UnitOfWork) to the def 
        //public AppRepository() { }
        public AppRepository(ApplicationDbContext dbContext, ILoggerManager logger, IAppUnitOfWork unitOfWork)
        {
            _Context = dbContext;
            _Table = dbContext.Set<TEntity>();
            _Logger = logger;
            _AppUnitOfWork = unitOfWork;
        }

        //public ApplicationDbContext appDbContext { get { return _context; } set { _context = value; } }
        public ApplicationDbContext GetAppDbContext()
        {
            return _Context;
        }
        //public ILoggerManager loggerManager { get { return _logger; } }
        public async Task<int> CountAsync()
        {
            return await _Table.CountAsync();
        }
        public int Add(TEntity newEntity)
        {
            int _added = AppUnitOfWork.CONST_WASERROR;    // return if error
            _Logger.LogDebug($"Adding entity: {newEntity.ToString()}");
            _AppUnitOfWork.BeginTransaction();
            try
            {
                _Context.Entry(newEntity).State = EntityState.Added;
                _Table.Add(newEntity);
                _added = _AppUnitOfWork.Complete();  // Save();
                //_context.Database.CommitTransaction();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Adding Item: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _added;
        }
        //public static IEnumerable<string> GetIdFields(TEntity Entity)
        //{
        //    var ids = from p in Entity.GetType().GetProperties().Where(e  => e.PropertyType ==  )


        //              where (from a in p.GetCustomAttributes(false)
        //                     where a is   EdmScalarPropertyAttribute &&
        //                       ((EdmScalarPropertyAttribute)a).EntityKeyProperty
        //                     select true).FirstOrDefault()
        //              select p.Name;
        //    return ids;
        //}

        //public static string GetIdField<TEntity>() where TEntity : EntityObject
        //{
        //    IEnumerable<string> ids = GetIdFields<TEntity>();
        //    string id = ids.Where(s => s.Trim().StartsWith(typeof(TEntity).Name.
        //                  Trim())).FirstOrDefault();
        //    if (string.IsNullOrEmpty(id)) id = ids.First();
        //    return id;
        //}

        public async Task<int> AddAsync(TEntity newEntity)
        {
            int _added = AppUnitOfWork.CONST_WASERROR;    // return if error
            _Logger.LogDebug($"Adding entity (async): {newEntity.ToString()}");
            _AppUnitOfWork.BeginTransaction();
            try
            {
                //_context.Entry(newEntity).State = EntityState.Added;
                await _Table.AddAsync(newEntity);
                _added = await _AppUnitOfWork.CompleteAsync();  // Save
                // !!!!!!   id field is set once is save by EF - no need to set it
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Adding Item (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _added;
        }
        public async Task<int> AddRangeAsync(List<TEntity> newEntities)
        {
            int _added = AppUnitOfWork.CONST_WASERROR;    // return if error
            _Logger.LogDebug($"Adding range (async): {newEntities.ToString()}");
            _AppUnitOfWork.BeginTransaction();
            try
            {
                foreach (var newEntity in newEntities)
                {
                    _Context.Entry(newEntity).State = EntityState.Added;
                }
                await _Table.AddRangeAsync(newEntities);     // !!!!!!   id field is set once is save by EF - no need to set it
                _added = await _AppUnitOfWork.CompleteAsync();  // Save
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Adding range (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _added;
        }
        public int DeleteById(object sourceId)
        {
            int _deleted = AppUnitOfWork.CONST_WASERROR;    // return if error
            _Logger.LogDebug($"Deleting  by id entity: {sourceId.ToString()}");
            _AppUnitOfWork.BeginTransaction();
            try
            {
                TEntity _entity = GetById(sourceId);
                if (_entity != null)
                {
                    _Context.Entry(_entity).State = EntityState.Deleted;
                    _Table.Remove(_entity);
                    _deleted = _AppUnitOfWork.Complete();  // Save();
                }

            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Deleting Item: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }

            return _deleted;
        }
        public async Task<int> DeleteByIdAsync(object sourceId)
        {
            int _deleted = AppUnitOfWork.CONST_WASERROR;    // return if error
            _Logger.LogDebug($"Deleting by id entity (async): {sourceId.ToString()}");
            _AppUnitOfWork.BeginTransaction();
            try
            {
                TEntity _entity = await GetByIdAsync(sourceId);
                if (_entity != null)
                {
                    _Context.Entry(_entity).State = EntityState.Deleted;
                    _Table.Remove(_entity);
                    _deleted = await _AppUnitOfWork.CompleteAsync();  // Save();
                }

            }
            catch (Exception ex)
            {
                _Logger.LogError($"!!!Error Deleting entity (async): {ex.Message} - Inner Exception {ex.InnerException}");
                _AppUnitOfWork.RollbackTransaction();
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _deleted;
        }

        public int DeleteBy(Expression<Func<TEntity, bool>> predicate)
        {
            int _deleted = AppUnitOfWork.CONST_WASERROR;    // return if error
            _Logger.LogDebug($"Deleting entity by: {predicate.ToString()}");
            _AppUnitOfWork.BeginTransaction();
            try
            {
                var _entity = _Table.Find(predicate);
                if (_entity != null)
                {
                    _Context.Entry(_entity).State = EntityState.Deleted;
                    _Table.Remove(_entity);
                    _deleted = _AppUnitOfWork.Complete();  // Save();
                }

            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Deleting By: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _deleted;
        }
        public async Task<int> DeleteByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            int _deleted = AppUnitOfWork.CONST_WASERROR;    // return if error
            _Logger.LogDebug($"Deleting entity by (async): {predicate.ToString()}");
            _AppUnitOfWork.BeginTransaction();
            try
            {
                var _entity = await _Table.FindAsync(predicate);
                if (_entity != null)
                {
                    _Context.Entry(_entity).State = EntityState.Deleted;
                    _Table.Remove(_entity);
                    _deleted = await _AppUnitOfWork.CompleteAsync();  // Save();
                }

            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Deleting By (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _deleted;
        }

        public TEntity FindFirst()
        {

            TEntity _entity = null;
            _Logger.LogDebug($"Finding First entity of type: {typeof(TEntity)}");

            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                _entity = _Table.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public TEntity FindFirst(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity _entity = null;
            _Logger.LogDebug($"Finding First {predicate.ToString()} entity of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                _entity = _Table.FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first predicate entity: {ex.Message} - Inner Exception {ex.InnerException}");

#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public async Task<TEntity> FindFirstAsync()
        {
            TEntity _entity = null;
            _Logger.LogDebug($"Finding First entity of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                _entity = await _Table.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public async Task<TEntity> FindFirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity _entity = null;
            _Logger.LogDebug($"Finding First {predicate.ToString()} entity of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                _entity = await _Table.FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                string errorMessage = $"!!!Error Finding first predicate entity: {ex.Message} - Inner Exception {ex.InnerException}";
                _AppUnitOfWork.LogAndSetErrorMessage(errorMessage);
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public IEnumerable<TEntity> GetAll()
        {
            _Logger.LogDebug($"Getting all records in Table of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                return _Table.ToList();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            _Logger.LogDebug($"Getting all records (async) in Table of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                var _result =  await _Table.ToListAsync();
                return _result;
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<IEnumerable<TEntity>> GetAllEagerAsync(params Expression<Func<TEntity, object>>[] properties)
        {
            _Logger.LogDebug($"Get By all eager loading (async) {properties.ToString()} from table of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            var query = _Table as IQueryable<TEntity>; // _dbSet = dbContext.Set<TEntity>()

            query = properties
                       .Aggregate(query, (current, property) => current.Include(property));

            try
            {
                var result = await query.ToListAsync();    // AsNoTracking().ToList();    //readonly
                return result;
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error eager loading: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<IEnumerable<TEntity>> GetPagedEagerAsync(int startPage, int currentPageSize, params Expression<Func<TEntity, object>>[] properties)
        {
            _Logger.LogDebug($"Get By all eager loading (async) {properties.ToString()} from table of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            var query = _Table as IQueryable<TEntity>; // _dbSet = dbContext.Set<TEntity>()

            query = properties
                       .Aggregate(query, (current, property) => current.Include(property));

            try
            {
                var result = await query.Skip(startPage * currentPageSize).Take(currentPageSize).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error eager loading: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate)
        {
            _Logger.LogDebug($"Get By all {predicate.ToString()} from table of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                return _Table.Where(predicate).ToList();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting by: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            IEnumerable<TEntity> _Rows = null;
            _Logger.LogDebug($"Get By all (async) {predicate.ToString()} from table of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                _Rows = await _Table.Where(predicate).ToListAsync();
                _Logger.LogDebug($"Get By all (async) table {nameof(_Table)} returned: {_Rows.Count()} rows");
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Getting by (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _Rows;
        }

        public TEntity GetById(object Id)
        {
            _Logger.LogDebug($"Get By Id (async) {Id.ToString()} from table of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                return _Table.Find(Id);
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Get by Id: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<TEntity> GetByIdAsync(object Id)
        {
            _Logger.LogDebug($"Get By Id (async) {Id.ToString()} from table of type: {typeof(TEntity)}");
            if (_AppUnitOfWork.DBTransactionIsStillRunning())
                _Logger.LogDebug("Second transaction started before current transaction completed!");
            try
            {
                return await _Table.FindAsync(Id);
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Get by Id (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        //public async Task<bool> Save()
        //{
        //    var _saved = false;

        //    try
        //    {
        //        _saved = (await _context.SaveChangesAsync() > 0);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"!!!Error Saving: {ex.Message} - Inner Exception {ex.InnerException}");
        //    }

        //    return (_saved);
        //}

        //public async Task<int> Update(TEntity updatedEntity)
        public int Update(TEntity updatedEntity)
        {
            int _updated = AppUnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
            _Logger.LogDebug($"Updating entity: {updatedEntity.ToString()}");
            _AppUnitOfWork.BeginTransaction();
            try
            {
                _Context.Entry(updatedEntity).State = EntityState.Modified;
                _Table.Update(updatedEntity);
                _updated = _AppUnitOfWork.Complete();  // Save();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _updated;
        }
        public async Task<int> UpdateAsync(TEntity updatedEntity)
        {
            int _updated = AppUnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
            _Logger.LogDebug($"Updating entity (async): {updatedEntity.ToString()}");
            _AppUnitOfWork.BeginTransaction();
            try
            {
                _Context.Entry(updatedEntity).State = EntityState.Modified;
                _Table.Update(updatedEntity);
                _updated = await _AppUnitOfWork.CompleteAsync();  // Save();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _updated;
        }

        public async Task<int> UpdateRangeAsync(List<TEntity> updateEntities)
        {
            int _updated = AppUnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
            _Logger.LogDebug($"Updating range entity (async): {updateEntities.ToString()}");
            _AppUnitOfWork.BeginTransaction();
            try
            {
                _Context.Entry(updateEntities).State = EntityState.Modified;
                _Table.UpdateRange(updateEntities);
                _updated = await _AppUnitOfWork.CompleteAsync();  // Save();
            }
            catch (Exception ex)
            {
                _AppUnitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _updated;
        }
    }
}
