using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RanbowOF.Repositories.Common
{
    public interface IAppRepository<TEntity> where TEntity : class
    {

        TEntity GetById(object Id);
        Task<TEntity> GetByIdAsync(object Id);
        IEnumerable<TEntity> GetAll();
        Task<IEnumerable<TEntity>> GetAllAsync();
        IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity FindFirst();
        TEntity FindFirst(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> FindFirstAsync();
        Task<TEntity> FindFirstAsync(Expression<Func<TEntity, bool>> predicate);
        int Add(TEntity newEntity);
        Task<int> AddAsync(TEntity newEntity);
        int Delete(object Id);
        Task<int> DeleteAsync(object Id);
        int DeleteBy(Expression<Func<TEntity, bool>> predicate);
        Task<int> DeleteByAsync(Expression<Func<TEntity, bool>> predicate);
        int Update(TEntity updatedEntity);
        Task<int> UpdateAsync(TEntity updatedEntity);
        //Task<bool> Save();
    }
}
