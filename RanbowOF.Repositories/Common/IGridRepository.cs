using RainbowOF.ViewModels.Common;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public interface IGridRepository<TEntity> where TEntity : class
    {
        #region Common variables
        GridSettings CurrGridSettings { get; set; }
        #endregion
        #region Recommended Over writable Grid Classes
        TEntity NewViewEntityDefaultSetter(TEntity newEntity, Guid Parent); //-> needs to be implemented at a grid class level.
        TEntity NewViewEntityDefaultSetter(TEntity newEntity); //-> needs to be implemented at a grid class level.
        Task<bool> IsDuplicateAsync(TEntity checkEntity);  //-> needs to be implemented at a grid class level.
        bool IsValid(TEntity checkEntity);  //-> needs to be implemented at a grid class level.
        #endregion
        #region Generically implemented interfaces
        Task<TEntity> GetEntityByIdAsync(object Id);
        Task<TEntity> FindFirstByAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> InsertViewRowAsync(TEntity newEntity, string newEntityDescription);
        Task<int> DeleteViewRowByEntityAsync(TEntity sourceEntity, string deletedEntityDescription);
        Task<int> UpdateViewRowAsync(TEntity updatedEntity, string updatedEnityDescription);
        #endregion

    }
}
