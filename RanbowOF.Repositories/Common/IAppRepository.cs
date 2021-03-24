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

        TEntity GetById(object Id);
        Task<TEntity> GetByIdAsync(object Id);
        IEnumerable<TEntity> GetAll();
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetAllEagerAsync(params Expression<Func<TEntity, object>>[] properties);
        IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity FindFirst();
        TEntity FindFirst(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> FindFirstAsync();
        Task<TEntity> FindFirstAsync(Expression<Func<TEntity, bool>> predicate);
        int Add(TEntity newEntity);
        Task<int> AddAsync(TEntity newEntity);
        Task<int> AddRangeAsync(List<TEntity> newEntities);
        int DeleteById(object Id);
        Task<int> DeleteByIdAsync(object Id);
        int DeleteBy(Expression<Func<TEntity, bool>> predicate);
        Task<int> DeleteByAsync(Expression<Func<TEntity, bool>> predicate);
        int Update(TEntity updatedEntity);
        Task<int> UpdateAsync(TEntity updatedEntity);
        Task<int> UpdateRangeAsync(List<TEntity> updateEntities);
        //Task<bool> Save();
    }
}
