using Microsoft.EntityFrameworkCore.Query.Internal;
using RainbowOF.Data.SQL;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public interface IRepository<TEntity> where TEntity : class
    {
        //ApplicationDbContext AppDbContext { get; }
        Task<int> CountAsync();
        bool CanDoDbAsyncCall(string startString);
        void DbCallDone(string endString);
        TEntity GetById(object Id);
        Task<TEntity> GetByIdAsync(object Id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> GetAllOrderBy(Func<TEntity, object> orderByExpression, bool sortDesc = false);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetAllOrderByAsync<TKey>(Expression<Func<TEntity, TKey>> orderByExpression, bool sortDesc = false);
        Task<IEnumerable<TEntity>> GetAllEagerAsync(params Expression<Func<TEntity, object>>[] properties);
        Task<IEnumerable<TEntity>> GetPagedEagerAsync(int startPage, int currentPageSize, params Expression<Func<TEntity, object>>[] properties);
        IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity FindFirst();
        TEntity FindFirstBy(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> FindFirstAsync();
        Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> predicate);
        int Add(TEntity newEntity);
        Task<TEntity> AddAsync(TEntity newEntity);
        Task<int> AddRangeAsync(List<TEntity> newEntities);
        int DeleteByEntity(TEntity sourceEntity);
        Task<int> DeleteByEntityAsync(TEntity sourceEntity);
        int DeleteBy(Expression<Func<TEntity, bool>> predicate);
        Task<int> DeleteByAsync(Expression<Func<TEntity, bool>> predicate);
        int Update(TEntity updatedEntity);
        Task<int> UpdateAsync(TEntity updatedEntity);
        Task<int> UpdateRangeAsync(List<TEntity> updateEntities);
        //Task<bool> Save();
    }
}
