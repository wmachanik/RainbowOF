using RainbowOF.Components.Modals;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Common;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public class FormRepository<TEntity> : IFormRepository<TEntity> where TEntity : class
    {
        #region Common variables
        public FormSettings CurrFormSettings { get; set; } = new();
        private IRepository<TEntity> appRepository {get;}
        #endregion
        #region passed in variables
        public ILoggerManager AppLoggerManager { get; set; }
        public IUnitOfWork AppUnitOfWork { get; set; }
        //        public IMapper AppMapper { get; set; }
        #endregion
        #region initialisation routine
        public FormRepository(ILoggerManager sourceLogger,
                               IUnitOfWork sourceAppUnitOfWork) //, IMapper sourceMapper)
        {
            AppLoggerManager = sourceLogger;
            AppUnitOfWork = sourceAppUnitOfWork;
            appRepository = AppUnitOfWork.Repository<TEntity>();

            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug($"FromRepository of type {typeof(TEntity).Name} initialised.");
            //AppMapper = sourceMapper;
        }
        #endregion
        #region Recommended Over writable Form Classes
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
            var _result = await appRepository.GetByIdAsync(Id);
            return _result;
        }
        /// <summary>
        /// Search for the Entity using a predicate.
        /// Logic: Using the AppUnitOfWork.repostitory<TEntity> FindFirstByAsync using the predicate object passed in?
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<TEntity> FidnFirstByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            //IRepository<TEntity> appRepository = AppUnitOfWork.Repository<TEntity>();
            var _result = await appRepository.GetByIdAsync(predicate);
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
                    CurrFormSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"A duplicate of {newEntityDescription} was found in the database.");
                    return null;
                }
                else
                {
                    //IRepository<TEntity> appRepository = AppUnitOfWork.Repository<TEntity>();
                    var _result = await appRepository.AddAsync(newEntity);
                    if (_result == null)
                        CurrFormSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error adding: {newEntityDescription}.");
                    else
                        CurrFormSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"{newEntityDescription} added.");
                    return _result;
                }
            }
            else
            {
                CurrFormSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"{newEntityDescription} is not valid.");
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
        public async Task<int> DeleteViewRowByIdAsync(TEntity sourceEntity, string deletedEntityDescription)
        {
            //IRepository<TEntity> appRepository = AppUnitOfWork.Repository<TEntity>();
            var _result = await appRepository.DeleteByEntityAsync(sourceEntity);
            if (_result == null)
                CurrFormSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error deleting: {deletedEntityDescription}.");
            else
                CurrFormSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"{deletedEntityDescription} deleted.");
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
                //dup check is not correct here, need to do it somewhere else.
                //if (await IsDuplicateAsync(updatedEntity))
                //{
                //    CurrFormSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"A duplicate of {updatedEntityDescription} was found in the database.");
                //    return UnitOfWork.CONST_WASERROR;
                //}
                //else
                //{
                    //IRepository<TEntity> _appRepository = AppUnitOfWork.Repository<TEntity>();
                    var _result = await appRepository.UpdateAsync(updatedEntity);

                    if (AppUnitOfWork.IsInErrorState())
                        CurrFormSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error updating: {updatedEntityDescription} - {AppUnitOfWork.GetErrorMessage()}.");
                    else
                        CurrFormSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"{updatedEntityDescription} updated.");
                    return _result;
                //}
            }
            else
            {
                CurrFormSettings.PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"{updatedEntityDescription} is not valid.");
                return UnitOfWork.CONST_WASERROR;
            }
        }
        #endregion
    }
}
