using RainbowOF.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public interface IFormRepository<TEntity> where TEntity : class
    {
        #region Common variables
        public FormSettings formSettings { get; set; }
        #endregion
        #region Recommended Over writable From Classes
        TEntity NewViewEntityDefaultSetter(TEntity newEntity, Guid Parent); //-> needs to be implemented at a form class level.
        TEntity NewViewEntityDefaultSetter(TEntity newEntity); //-> needs to be implemented at a form class level.
        Task<bool> IsDuplicateAsync(TEntity checkEntity);  //-> needs to be implemented at a form class level.
        bool IsValid(TEntity checkEntity);  //-> needs to be implemented at a form class level.
        #endregion
        #region Generically implemented interfaces
        Task<TEntity> GetEntityByIdAsync(object Id);
        Task<TEntity> FidnFirstByAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> InsertViewRowAsync(TEntity newEntity, string newEntityDescription);
        Task<int> DeleteViewRowByIdAsync(object Id, string deletedEntityDescription);
        Task<int> UpdateViewRowAsync(TEntity updatedEntity, string updatedEnityDescription);
        #endregion

    }
}
