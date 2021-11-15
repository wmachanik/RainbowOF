using AutoMapper;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    public class GridViewRepository<TEntity> : IGridViewRepository<TEntity> where TEntity : class
    {
        #region Common variables
        public GridSettings _GridSettings { get; set; } = new();
        #endregion
        #region passed in variables
        public ILoggerManager _Logger { get; set; }
        public IAppUnitOfWork _AppUnitOfWork { get; set; }
        //        public IMapper _Mapper { get; set; }
        #endregion
        #region initialisation routine
        public GridViewRepository(ILoggerManager sourceLogger,
                               IAppUnitOfWork sourceAppUnitOfWork) //, IMapper sourceMapper)
        {
            _Logger = sourceLogger;
            _AppUnitOfWork = sourceAppUnitOfWork;
            //_Mapper = sourceMapper;
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
            IAppRepository<TEntity> appRepository = _AppUnitOfWork.Repository<TEntity>();
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
            IAppRepository<TEntity> appRepository = _AppUnitOfWork.Repository<TEntity>();
            var _result = await appRepository.FindFirstByAsync(predicate);
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
                    _GridSettings.PopUpRef.ShowNotification(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"A duplicate of {newEntityDescription} was found in the database.");
                    return null;
                }
                else
                {
                    IAppRepository<TEntity> appRepository = _AppUnitOfWork.Repository<TEntity>();
                    var _result = await appRepository.AddAsync(newEntity);
                    if (_result == null)
                        _GridSettings.PopUpRef.ShowNotification(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"Error adding: {newEntityDescription}.");
                    else
                        _GridSettings.PopUpRef.ShowNotification(Components.Modals.PopUpAndLogNotification.NotificationType.Success, $"{newEntityDescription} added.");
                    return _result;
                }
            }
            else
            {
                _GridSettings.PopUpRef.ShowNotification(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"{newEntityDescription} is not valid.");
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
        public async Task<int> DeleteViewRowByIdAsync(object Id, string deletedEntityDescription)
        {
            IAppRepository<TEntity> appRepository = _AppUnitOfWork.Repository<TEntity>();
            var _result = await appRepository.DeleteByIdAsync(Id);
            if (_result == null)
                _GridSettings.PopUpRef.ShowNotification(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"Error deleting: {deletedEntityDescription}.");
            else
                _GridSettings.PopUpRef.ShowNotification(Components.Modals.PopUpAndLogNotification.NotificationType.Success, $"{deletedEntityDescription} deleted.");
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
               // dup check is not correct here, need to do it somewhere else.
                if (await IsDuplicateAsync(updatedEntity))
                {
                    _GridSettings.PopUpRef.ShowNotification(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"A duplicate of {updatedEntityDescription} was found in the database.");
                    return AppUnitOfWork.CONST_WASERROR;
                }
                else
                {
                    IAppRepository<TEntity> _appRepository = _AppUnitOfWork.Repository<TEntity>();
                    var _result = await _appRepository.UpdateAsync(updatedEntity);
                    if (_AppUnitOfWork.IsInErrorState())
                        _GridSettings.PopUpRef.ShowNotification(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"Error updating: {updatedEntityDescription} - {_AppUnitOfWork.GetErrorMessage()}.");
                    else
                        _GridSettings.PopUpRef.ShowNotification(Components.Modals.PopUpAndLogNotification.NotificationType.Success, $"{updatedEntityDescription} updated.");
                    return _result;
               }
            }
            else
            {
                _GridSettings.PopUpRef.ShowNotification(Components.Modals.PopUpAndLogNotification.NotificationType.Error, $"{updatedEntityDescription} is not valid.");
                return AppUnitOfWork.CONST_WASERROR;
            }
        }
        #endregion
    }
}
