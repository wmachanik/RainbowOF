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
        private ApplicationDbContext _context { get; set; } = null;
        private DbSet<TEntity> _table = null;
        private ILoggerManager _logger { get; }
        private IAppUnitOfWork _unitOfWork {get ;set;}

        /// should nto need this had it her to stop the error: "There is no argument given that corresponds to the required formal parameter"
        /// if it is asking for that you need to ass  : base (dbContext, logger, unitofwork) to the def 
        //public AppRepository() { }
        public AppRepository(ApplicationDbContext dbContext, ILoggerManager logger, IAppUnitOfWork unitOfWork)
        {
            _context = dbContext;
            _table = _context.Set<TEntity>();
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public ApplicationDbContext appDbContext { get { return _context; } set { _context = value; } }

        public ILoggerManager loggerManager { get { return _logger; } }

        public int Add(TEntity newEntity)
        {
            int _added = 0;
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
                _logger.LogError($"!!!Error Adding Item: {ex.Message}");
                _unitOfWork.RollbackTransaction();
                throw;     // #Debug?
            }
            return _added;
        }
        public async Task<int> AddAsync(TEntity newEntity)
        {
            int _added = 0;
            _logger.LogDebug($"Adding item (async): {newEntity.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                _context.Entry(newEntity).State = EntityState.Added;
                await _table.AddAsync(newEntity);
                _added = await _unitOfWork.CompleteAsync();  // Save();
                //_context.Database.CommitTransaction();
            }
            catch (Exception ex)
            {
                _logger.LogError($"!!!Error Adding Item (async): {ex.Message}");
                _unitOfWork.RollbackTransaction();
                throw;     // #Debug?
            }
            return _added;
        }

        public  int Delete(object Id)
        {
            int _deleted = 0;
            _logger.LogDebug($"Deleting item: {Id.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                TEntity _entity = GetById(Id);
                if (_entity != null)
                {
                    _context.Entry(_entity).State = EntityState.Deleted;
                    _table.Remove(_entity);
                    _deleted =  _unitOfWork.Complete();  // Save();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"!!!Error Deleting Item: {ex.Message}");
                _unitOfWork.RollbackTransaction();
                throw;     // #Debug?
            }

            return _deleted;
        }
        public async Task<int> DeleteAsync(object Id)
        {
            int _deleted = 0;
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
                _logger.LogError($"!!!Error Deleting Item (async): {ex.Message}");
                _unitOfWork.RollbackTransaction();
                throw;     // #Debug?
            }

            return _deleted;
        }

        public int DeleteBy(Expression<Func<TEntity, bool>> predicate)
        {
            int _deleted = 0;
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
                _logger.LogError($"!!!Error Deleting By: {ex.Message}");
                _unitOfWork.RollbackTransaction();
                throw;     // #Debug?
            }

            return _deleted;
        }
        public async Task<int> DeleteByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            int _deleted = 0;
            _logger.LogDebug($"Deleting By (async): {predicate.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                var _entity =  await _table.FindAsync(predicate);
                if (_entity != null)
                {
                    _context.Entry(_entity).State = EntityState.Deleted;
                    _table.Remove(_entity);
                    _deleted = await _unitOfWork.CompleteAsync();  // Save();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"!!!Error Deleting By (async): {ex.Message}");
                _unitOfWork.RollbackTransaction();
                throw;     // #Debug?
            }

            return _deleted;
        }

        public TEntity FindFirst(Expression<Func<TEntity, bool>> predicate)
        {
            return _table.FirstOrDefault(predicate);
        }
        public async Task<TEntity> FindFirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _table.FirstOrDefaultAsync(predicate);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _table.ToList();
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _table.ToListAsync();
        }

        public IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate)
        {
            return _table.Where(predicate).ToList();
        }
        public async Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _table.Where(predicate).ToListAsync();
        }

        public TEntity GetById(object Id)
        {
            return _table.Find(Id);
        }
        public async Task<TEntity> GetByIdAsync(object Id)
        {
            return await _table.FindAsync(Id);
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
        //        _logger.LogError($"!!!Error Saving: {ex.Message}");
        //    }

        //    return (_saved);
        //}

        //public async Task<int> Update(TEntity updatedEntity)
        public int Update(TEntity updatedEntity)
        {
            int _updated = 0;

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
                _logger.LogError($"!!!Error Updating Table: {ex.Message}");
                _unitOfWork.RollbackTransaction();
                throw;
            }
            return _updated;
        }
        public async Task<int> UpdateAsync(TEntity updatedEntity)
        {
            int _updated = 0;

            _logger.LogDebug($"Updating item (async): {updatedEntity.ToString()}");
            _unitOfWork.BeginTransaction();
            try
            {
                _context.Entry(updatedEntity).State = EntityState.Modified;
                _table.Update ( updatedEntity);
                _updated =  await _unitOfWork.CompleteAsync();  // Save();
            }
            catch (Exception ex)
            {
                _logger.LogError($"!!!Error Updating Table (async): {ex.Message}");
                _unitOfWork.RollbackTransaction();
                throw;
            }
            return _updated;
        }
    }
}
