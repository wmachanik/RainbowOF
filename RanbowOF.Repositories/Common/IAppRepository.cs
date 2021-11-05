using RainbowOF.Data.SQL;
using RainbowOF.Models.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public interface IAppRepository<TEntity> where TEntity : class
    {
        ApplicationDbContext GetAppDbContext();

        Task<int> CountAsync();
        TEntity GetById(object Id);
        Task<TEntity> GetByIdAsync(object Id);
        IEnumerable<TEntity> GetAll();
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetAllEagerAsync(params Expression<Func<TEntity, object>>[] properties);
        Task<IEnumerable<TEntity>> GetPagedEagerAsync(int startPage, int currentPageSize, params Expression<Func<TEntity, object>>[] properties);
        IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity FindFirst();
        TEntity FindFirstBy(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> FindFirstAsync();
        Task<TEntity> FindFirstByAsync(Expression<Func<TEntity, bool>> predicate);
        int Add(TEntity newEntity);
        Task<TEntity> AddAsync(TEntity newEntity);
        Task<int> AddRangeAsync(List<TEntity> newEntities);
        int DeleteById(object sourceId);
        Task<int> DeleteByIdAsync(object sourceId);
        int DeleteBy(Expression<Func<TEntity, bool>> predicate);
        Task<int> DeleteByAsync(Expression<Func<TEntity, bool>> predicate);
        int Update(TEntity updatedEntity);
        Task<int> UpdateAsync(TEntity updatedEntity);
        Task<int> UpdateRangeAsync(List<TEntity> updateEntities);
        //Task<bool> Save();
    }
}
