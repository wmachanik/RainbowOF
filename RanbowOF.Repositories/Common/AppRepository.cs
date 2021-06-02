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
        private ApplicationDbContext _context { get; set; }  // = null;
        private DbSet<TEntity> _table = null;
        private ILoggerManager _logger { get; }
        private IAppUnitOfWork _unitOfWork { get; set; }
        /// should not need this had it her to stop the error: "There is no argument given that corresponds to the required formal parameter"
        /// if it is asking for that you need to ass  : base (dbContext, logger, unitofwork) to the def 
        //public AppRepository() { }
        public AppRepository(ApplicationDbContext dbContext, ILoggerManager logger, IAppUnitOfWork unitOfWork)
        {
            _context = dbContext;
            _table = dbContext.Set<TEntity>();
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public ApplicationDbContext appDbContext { get { return _context; } set { _context = value; } }

        public ILoggerManager loggerManager { get { return _logger; } }
        public async Task<int> CountAsync()
        {
            return await _table.CountAsync();
        }
        public int Add(TEntity newEntity)
        {
            int _added = AppUnitOfWork.CONST_WASERROR;    // return if error
            _logger.LogDebug($"Adding entity: {newEntity.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                _context.Entry(newEntity).State = EntityState.Added;
                _table.Add(newEntity);
                _added = _unitOfWork.Complete();  // Save();
                //_context.Database.CommitTransaction();
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Adding Item: {ex.Message} - Inner Exception {ex.InnerException}");
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
            _logger.LogDebug($"Adding entity (async): {newEntity.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                //_context.Entry(newEntity).State = EntityState.Added;
                await _table.AddAsync(newEntity);
                _added = await _unitOfWork.CompleteAsync();  // Save
                // !!!!!!   id field is set once is save by EF - no need to set it
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Adding Item (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _added;
        }
        public async Task<int> AddRangeAsync(List<TEntity> newEntities)
        {
            int _added = AppUnitOfWork.CONST_WASERROR;    // return if error
            _logger.LogDebug($"Adding range (async): {newEntities.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                foreach (var newEntity in newEntities)
                {
                    _context.Entry(newEntity).State = EntityState.Added;
                }
                await _table.AddRangeAsync(newEntities);     // !!!!!!   id field is set once is save by EF - no need to set it
                _added = await _unitOfWork.CompleteAsync();  // Save
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Adding range (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _added;
        }
        public int DeleteById(object Id)
        {
            int _deleted = AppUnitOfWork.CONST_WASERROR;    // return if error
            _logger.LogDebug($"Deleting entity: {Id.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                TEntity _entity = GetById(Id);
                if (_entity != null)
                {
                    _context.Entry(_entity).State = EntityState.Deleted;
                    _table.Remove(_entity);
                    _deleted = _unitOfWork.Complete();  // Save();
                }

            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Deleting Item: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }

            return _deleted;
        }
        public async Task<int> DeleteByIdAsync(object Id)
        {
            int _deleted = AppUnitOfWork.CONST_WASERROR;    // return if error
            _logger.LogDebug($"Deleting entity (async): {Id.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                TEntity _entity = await GetByIdAsync(Id);
                if (_entity != null)
                {
                    _context.Entry(_entity).State = EntityState.Deleted;
                    _table.Remove(_entity);
                    _deleted = await _unitOfWork.CompleteAsync();  // Save();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"!!!Error Deleting entity (async): {ex.Message} - Inner Exception {ex.InnerException}");
                _unitOfWork.RollbackTransaction();
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _deleted;
        }

        public int DeleteBy(Expression<Func<TEntity, bool>> predicate)
        {
            int _deleted = AppUnitOfWork.CONST_WASERROR;    // return if error
            _logger.LogDebug($"Deleting entity by: {predicate.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                var _entity = _table.Find(predicate);
                if (_entity != null)
                {
                    _context.Entry(_entity).State = EntityState.Deleted;
                    _table.Remove(_entity);
                    _deleted = _unitOfWork.Complete();  // Save();
                }

            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Deleting By: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _deleted;
        }
        public async Task<int> DeleteByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            int _deleted = AppUnitOfWork.CONST_WASERROR;    // return if error
            _logger.LogDebug($"Deleting entity by (async): {predicate.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                var _entity = await _table.FindAsync(predicate);
                if (_entity != null)
                {
                    _context.Entry(_entity).State = EntityState.Deleted;
                    _table.Remove(_entity);
                    _deleted = await _unitOfWork.CompleteAsync();  // Save();
                }

            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Deleting By (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _deleted;
        }

        public TEntity FindFirst()
        {
            TEntity _entity = null;
            _logger.LogDebug($"Finding First entity of type: {typeof(TEntity)}");
            try
            {
                _entity = _table.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public TEntity FindFirst(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity _entity = null;
            _logger.LogDebug($"Finding First {predicate.ToString()} entity of type: {typeof(TEntity)}");
            try
            {
                _entity = _table.FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Finding first predicate entity: {ex.Message} - Inner Exception {ex.InnerException}");

#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public async Task<TEntity> FindFirstAsync()
        {
            TEntity _entity = null;
            _logger.LogDebug($"Finding First entity of type: {typeof(TEntity)}");
            try
            {
                _entity = await _table.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Finding first entity: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public async Task<TEntity> FindFirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity _entity = null;
            _logger.LogDebug($"Finding First {predicate.ToString()} entity of type: {typeof(TEntity)}");
            try
            {
                _entity = await _table.FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                string errorMessage = $"!!!Error Finding first predicate entity: {ex.Message} - Inner Exception {ex.InnerException}";
                _unitOfWork.LogAndSetErrorMessage(errorMessage);
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public IEnumerable<TEntity> GetAll()
        {
            _logger.LogDebug($"Getting all records in Table of type: {typeof(TEntity)}");
            try
            {
                return _table.ToList();
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            _logger.LogDebug($"Getting all records (async) in Table of type: {typeof(TEntity)}");
            try
            {
                var _result =  await _table.ToListAsync();
                return _result;
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Getting all (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<IEnumerable<TEntity>> GetAllEagerAsync(params Expression<Func<TEntity, object>>[] properties)
        {
            _logger.LogDebug($"Get By all eager loading (async) {properties.ToString()} from table of type: {typeof(TEntity)}");
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            var query = _table as IQueryable<TEntity>; // _dbSet = dbContext.Set<TEntity>()

            query = properties
                       .Aggregate(query, (current, property) => current.Include(property));

            try
            {
                var result = await query.ToListAsync();    // AsNoTracking().ToList();    //readonly
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error eager loading: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<IEnumerable<TEntity>> GetPagedEagerAsync(int startPage, int currentPageSize, params Expression<Func<TEntity, object>>[] properties)
        {
            _logger.LogDebug($"Get By all eager loading (async) {properties.ToString()} from table of type: {typeof(TEntity)}");
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            var query = _table as IQueryable<TEntity>; // _dbSet = dbContext.Set<TEntity>()

            query = properties
                       .Aggregate(query, (current, property) => current.Include(property));

            try
            {
                var result = await query.Skip(startPage * currentPageSize).Take(currentPageSize).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error eager loading: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate)
        {
            _logger.LogDebug($"Get By all {predicate.ToString()} from table of type: {typeof(TEntity)}");
            try
            {
                return _table.Where(predicate).ToList();
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Getting by: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            IEnumerable<TEntity> _Rows = null;
            _logger.LogDebug($"Get By all (async) {predicate.ToString()} from table of type: {typeof(TEntity)}");
            try
            {
                _Rows = await _table.Where(predicate).ToListAsync();
                _logger.LogDebug($"Get By all (async) table {nameof(_table)} returned: {_Rows.Count()} rows");
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Getting by (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _Rows;
        }

        public TEntity GetById(object Id)
        {
            _logger.LogDebug($"Get By Id (async) {Id.ToString()} from table of type: {typeof(TEntity)}");
            try
            {
                return _table.Find(Id);
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Get by Id: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return null;
        }
        public async Task<TEntity> GetByIdAsync(object Id)
        {
            _logger.LogDebug($"Get By Id (async) {Id.ToString()} from table of type: {typeof(TEntity)}");
            try
            {
                return await _table.FindAsync(Id);
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Get by Id (async): {ex.Message} - Inner Exception {ex.InnerException}");
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
            _logger.LogDebug($"Updating entity: {updatedEntity.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                _context.Entry(updatedEntity).State = EntityState.Modified;
                _table.Update(updatedEntity);
                _updated = _unitOfWork.Complete();  // Save();
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _updated;
        }
        public async Task<int> UpdateAsync(TEntity updatedEntity)
        {
            int _updated = AppUnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
            _logger.LogDebug($"Updating entity (async): {updatedEntity.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                _context.Entry(updatedEntity).State = EntityState.Modified;
                _table.Update(updatedEntity);
                _updated = await _unitOfWork.CompleteAsync();  // Save();
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _updated;
        }

        public async Task<int> UpdateRangeAsync(List<TEntity> updateEntities)
        {
            int _updated = AppUnitOfWork.CONST_WASERROR;   // -1 means error only returned if there is one
            _logger.LogDebug($"Updating range entity (async): {updateEntities.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                _context.Entry(updateEntities).State = EntityState.Modified;
                _table.UpdateRange(updateEntities);
                _updated = await _unitOfWork.CompleteAsync();  // Save();
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Updating Table (async): {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _updated;
        }
    }
}
