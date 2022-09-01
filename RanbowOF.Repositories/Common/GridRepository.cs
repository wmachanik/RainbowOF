using RainbowOF.Repositories.Items;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Common;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public class GridRepository<TEntity> : IGridRepository<TEntity> where TEntity : class
    {
        #region Common variables
        public GridSettings CurrGridSettings { get; set; } = new();
        #endregion
        #region Passed in variables
        public ILoggerManager AppLoggerManager { get; set; }
        public IRepository<TEntity> EntityRepository { get; set; }
        //public IUnitOfWork AppUnitOfWork { get; set; } //-> instead of using a UnitOfWork use a repo?
        //        public IMapper AppMapper { get; set; }
        #endregion
        #region initialisation routine
        public GridRepository(ILoggerManager sourceLogger,
                              IUnitOfWork sourceAppUnitOfWork,
                              IRepository<TEntity> sourceRepository = null) //, IMapper sourceMapper)
        {
            AppLoggerManager = sourceLogger;
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"GridRepository of type {typeof(TEntity)} initialising.");
            // for generic grids that use the generic repos.
            if (sourceRepository == null) EntityRepository = sourceAppUnitOfWork.Repository<TEntity>();  
                                     else EntityRepository = sourceRepository;
            //AppUnitOfWork = sourceAppUnitOfWork;
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"GridRepository of type {typeof(TEntity)} initialised.");
            //AppMapper = sourceMapper;
        }
        #endregion
        #region Recommended Over writable Grid Classes
        /// <summary>
        /// Used to create a new entity when the add item button is pressed, this needs to be overwritten at a class specific level.
        /// Logic: return null, as we have no idea what will make the generic class valid.
        /// </summary>
        /// <param name="newEntity">TEntity newEntity: blank entity to be initialised</param>
        /// <returns>null.</returns>
        public virtual TEntity NewViewEntityDefaultSetter(TEntity newEntity)
        {
            return null;  // should cause a crash if not overridden 
        }
        public virtual TEntity NewViewEntityDefaultSetter(TEntity newEntity, Guid ParentId)
        {
            return null;  // should cause a crash if not overridden 
        }
        /// <summary>
        /// See if entity a duplicate, this needs to be overwritten at a class specific level.
        /// Logic: return false, as we have no idea what will make the generic class a duplicate.
        /// #pragma warning disable used so we do not get the warning in the compiler
        /// </summary>
        /// <param name="checkEntity">Entity to be checked</param>
        /// <returns>Bool true if a duplicate false if not. </returns>
#pragma warning disable
        public virtual async Task<bool> IsDuplicateAsync(TEntity checkEntity)
        {
            return false;
        }
#pragma warning enable
        /// <summary>
        /// See if entity valid, this needs to be overwritten at a class specific level.
        /// Logic: return true, as we have no idea what will make the generic class valid.
        /// </summary>
        /// <param name="checkEntity">TEntity sourceEntity: Entity to be checked</param>
        /// <returns>Bool true if a valid false if not. </returns>
        public virtual bool IsValid(TEntity checkEntity)
        {
            return true;
        }
        #endregion
        /// <summary>
        /// Does a generic GetByIdAsync and returns the result
        /// Logic: Using the AppUnitOfWork.repostitory<TEntity> GetByIdAsync the object passed in?
        /// </summary>
        /// <param name="Id">Id to search for</param>
        /// <returns>item found or null if not</returns>
        #region Generic routines       
        public async Task<TEntity> GetEntityByIdAsync(object Id)
        {
            //IRepository<TEntity> appRepository = AppUnitOfWork.Repository<TEntity>();
            var _result = await EntityRepository.GetByIdAsync(Id);
            return _result;
        }
        /// <summary>
        /// Search for the Entity using a predicate.
        /// Logic: Using the AppUnitOfWork.repostitory<TEntity> FindFirstByAsync using the predicate object passed in?
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<TEntity> FindFirstByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            //IRepository<TEntity> appRepository = AppUnitOfWork.Repository<TEntity>();
            //var _result = await appRepository.GetByIdAsync(predicate);
            var _result = await EntityRepository.GetByIdAsync(predicate);
            return _result;
        }
        /// <summary>
        /// Insert the new Entity to the database
        /// Logic: Assumes that the entity does not exists and adds it, should be preceded by FindFirst By Async?
        /// </summary>
        /// <param name="newEntity">new entity to be saved</param>
        /// <param name="newEntityDescription">Description of the entity to be added. Used for notifications.</param>
        /// <returns>TEntity saved entity</returns>
        public async Task<TEntity> InsertViewRowAsync(TEntity newEntity, string newEntityDescription)
        {
            if (IsValid(newEntity))
            {
                if (await IsDuplicateAsync(newEntity))
                {
                    CurrGridSettings.PopUpRef.ShowNotificationAsync(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"A duplicate of {newEntityDescription} was found in the database.");
                    return null;
                }
                else
                {
                    //IRepository<TEntity> appRepository = AppUnitOfWork.Repository<TEntity>();
                    //var _result = await appRepository.AddAsync(newEntity);
                    var _result = await EntityRepository.AddAsync(newEntity);
                    if (_result == null)
                        CurrGridSettings.PopUpRef.ShowNotificationAsync(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"Error adding: {newEntityDescription}.");
                    else
                        CurrGridSettings.PopUpRef.ShowNotificationAsync(Components.Modals.PopUpAndLogNotification.NotificationType.Success, $"{newEntityDescription} added.");
                    return _result;
                }
            }
            else
            {
                CurrGridSettings.PopUpRef.ShowNotificationAsync(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"{newEntityDescription} is not valid.");
                return null;
            }
        }
        /// <summary>
        /// Delete the source Entity from the database
        /// Logic: Uses Delete By Id to delete
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="deletedEntityDescription">Description of the entity to be deleted. Used for notifications.</param>
        /// <returns></returns>
        public async Task<int> DeleteViewRowByEntityAsync(TEntity deletedEntity, string deletedEntityDescription)
        {
            //IRepository<TEntity> appRepository = AppUnitOfWork.Repository<TEntity>();
            //var _result = await appRepository.DeleteByPrimaryIdAsync(Id);
            var _result = await EntityRepository.DeleteByEntityAsync(deletedEntity);
            if (_result == null)
                CurrGridSettings.PopUpRef.ShowNotificationAsync(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"Error deleting: {deletedEntityDescription}.");
            else
                CurrGridSettings.PopUpRef.ShowNotificationAsync(Components.Modals.PopUpAndLogNotification.NotificationType.Success, $"{deletedEntityDescription} deleted.");
            return _result;
        }
        /// <summary>
        /// Update the source Entity in the database
        /// Logic: Assumes the entity was originally got from the database and is current
        /// </summary>
        /// <param name="newEntity"></param>
        /// <param name="updatedEntityDescription"></param>
        /// <returns></returns>
        public async Task<int> UpdateViewRowAsync(TEntity updatedEntity, string updatedEntityDescription)
        {
            if (IsValid(updatedEntity))
            {
                // no need to do a duplicate check since it is an update
                //IRepository<TEntity> _appRepository = AppUnitOfWork.Repository<TEntity>();
                //var _result = await _appRepository.UpdateAsync(updatedEntity);
                var _result = await EntityRepository.UpdateAsync(updatedEntity);
                if (_result == UnitOfWork.CONST_WASERROR)
                    CurrGridSettings.PopUpRef.ShowNotificationAsync(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"Error updating: {updatedEntityDescription}.");
                else
                    CurrGridSettings.PopUpRef.ShowNotificationAsync(Components.Modals.PopUpAndLogNotification.NotificationType.Success, $"{updatedEntityDescription} updated.");
                return _result;
            }
            else
            {
                CurrGridSettings.PopUpRef.ShowNotificationAsync(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"{updatedEntityDescription} is not valid.");
                return UnitOfWork.CONST_WASERROR;
            }
        }
        #endregion
    }
}
