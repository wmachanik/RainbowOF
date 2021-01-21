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

namespace RanbowOF.Repositories.Common
{
    public class AppRepository<TEntity> : IAppRepository<TEntity> where TEntity : class
    {
        private ApplicationDbContext _context { get; set; }  // = null;
        private DbSet<TEntity> _table = null;
        private ILoggerManager _logger { get; }
        private IAppUnitOfWork _unitOfWork { get; set; }
        /// should nto need this had it her to stop the error: "There is no argument given that corresponds to the required formal parameter"
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

        public int Add(TEntity newEntity)
        {
            int _added = AppUnitOfWork.CONST_WASERROR;    // return if error
            _logger.LogDebug($"Adding item: {newEntity.ToString()}");
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
            _logger.LogDebug($"Adding item (async): {newEntity.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                _context.Entry(newEntity).State = EntityState.Added;
                await _table.AddAsync(newEntity);
                _added = await _unitOfWork.CompleteAsync();  // Save

                // !!!!!!   id field is set once is save by EF - no need to set it
                /* if ((_added > 0) && (_context.Entry(newEntity).IsKeySet))
                //{
                //    var _idStr = _context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties.Single().Name;
                //    // once saved it if the table has an id field it should be set
                //    if (!String.IsNullOrEmpty(_idStr))
                //    {
                //        // if there was an idfield then it should not be null so set added to the id
                //        var _idProperty = newEntity.GetType().GetProperty(_idStr).GetValue(newEntity, null);
                //        if (_idProperty != null)
                //        {
                //            _added = (int)_idProperty;       //////////////////// what about guid?
                //        }
                //    }
                //}
                */
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

        public int Delete(object Id)
        {
            int _deleted = AppUnitOfWork.CONST_WASERROR;    // return if error
            _logger.LogDebug($"Deleting item: {Id.ToString()}");
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
        public async Task<int> DeleteAsync(object Id)
        {
            int _deleted = AppUnitOfWork.CONST_WASERROR;    // return if error
            _logger.LogDebug($"Deleting item (async): {Id.ToString()}");
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
                _logger.LogError($"!!!Error Deleting Item (async): {ex.Message} - Inner Exception {ex.InnerException}");
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
            _logger.LogDebug($"Deleting By: {predicate.ToString()}");
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
            _logger.LogDebug($"Deleting By (async): {predicate.ToString()}");
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
            _logger.LogDebug($"Finding First item of type: {typeof(TEntity)}");
            try
            {
                _entity = _table.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Fiding first item: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public TEntity FindFirst(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity _entity = null;
            _logger.LogDebug($"Finding First {predicate.ToString()} item of type: {typeof(TEntity)}");
            try
            {
                _entity = _table.FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Fiding first predicate item: {ex.Message} - Inner Exception {ex.InnerException}");

#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public async Task<TEntity> FindFirstAsync()
        {
            TEntity _entity = null;
            _logger.LogDebug($"Finding First item of type: {typeof(TEntity)}");
            try
            {
                _entity = await _table.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Fiding first item: {ex.Message} - Inner Exception {ex.InnerException}");
#if DebugMode
                throw;     // #Debug?
#endif
            }
            return _entity;
        }
        public async Task<TEntity> FindFirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity _entity = null;
            _logger.LogDebug($"Finding First {predicate.ToString()} item of type: {typeof(TEntity)}");
            try
            {
                _entity = await _table.FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                _unitOfWork.LogAndSetErrorMessage($"!!!Error Fiding first predicate item: {ex.Message} - Inner Exception {ex.InnerException}");
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
                return await _table.ToListAsync();
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
            _logger.LogDebug($"Updating item: {updatedEntity.ToString()}");
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
            _logger.LogDebug($"Updating item (async): {updatedEntity.ToString()}");
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

    }
}
