using RainbowOF.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.Models.Lookups;

namespace RainbowOF.Repositories.Items
{
    public class ItemAttributeGridRepository : GridRepository<ItemAttribute>, IItemAttributeGridRepository
    {
        public ItemAttributeGridRepository(ILoggerManager sourceLogger, IUnitOfWork sourceAppUnitOfWork) : base(sourceLogger, sourceAppUnitOfWork)
        {
            // base initialises
        }
        #region Recommended Over writable Grid Classes
        /// <summary>
        /// Used to create a new entity when the add item button is pressed, this needs to be overwritten at a class specific level.
        /// Logic: return null, as we have no idea what will make the generic class valid.
        /// </summary>
        /// <param name="newEntity">TEntity newEntity: blank entity to be initialised</param>
        /// <returns>null.</returns>
        public override ItemAttribute NewViewEntityDefaultSetter(ItemAttribute newEntity, Guid ParentId)
        {
            if (newEntity == null)
                newEntity = new ItemAttribute();

            newEntity.ItemId = ParentId;
            newEntity.IsUsedForItemVariety = false;
            newEntity.ItemAttributeLookupId = Guid.Empty;
            return newEntity;
        }
        /// <summary>
        /// See if entity a duplicate, this needs to be overwritten at a class specific level.
        /// Logic: return false, as we have no idea what will make the generic class a duplicate.
        /// #pragma warning disable used so we do not get the warning in the compiler
        /// </summary>
        /// <param name="checkEntity">Entity to be checked</param>
        /// <returns>Bool true if a duplicate false if not. </returns>
        public override async Task<bool> IsDuplicateAsync(ItemAttribute checkEntity)
        {
            if (checkEntity.ItemAttributeLookupId == Guid.Empty)
                return false;
            // here we need to check if the Attribute is the same, as we cannot have duplicate names
            return await appUnitOfWork.itemRepository.IsUniqueItemAttributeAsync(checkEntity);
            //var _result = await appUnitOfWork.itemRepository.GetByIdAsync(
            //                        checkEntity.ItemAttributeId == Guid.Empty ?
            //                            (ia => (ia.ItemId == checkEntity.ItemId)
            //                                && (ia.ItemAttributeLookupId == checkEntity.ItemAttributeLookupId)) :
            //                            (ia => (ia.ItemId == checkEntity.ItemId)
            //                                && (ia.ItemAttributeId != checkEntity.ItemAttributeId)
            //                                && (ia.ItemAttributeLookupId == checkEntity.ItemAttributeLookupId))
            //                          );
            //return (_result != null);
        }
        /// <summary>
        /// See if entity valid, this needs to be overwritten at a class specific level.
        /// Logic: return true, as we have no idea what will make the generic class valid.
        /// </summary>
        /// <param name="checkEntity">TEntity sourceEntity: Entity to be checked</param>
        /// <returns>Bool true if a valid false if not. </returns>
        public override bool IsValid(ItemAttribute checkEntity)
        {
            return (checkEntity.ItemAttributeLookupId != Guid.Empty);
        }
        #endregion
        #region Interface specific routines
        /// <summary>
        /// Get the Item Attribute Lookup Referenced by its Id
        /// Logic: Use generic GetById to return the ItemAttributeLookup by Id
        /// </summary>
        /// <param name="sourceItemAttributeLookupId">Id to search for</param>
        /// <returns>ItemAttributeLookup item, if found or null</returns>
        public async Task<ItemAttributeLookup> GetItemAttributeByIdAsync(Guid sourceItemAttributeLookupId)
        {
            IRepository<ItemAttributeLookup> _itemAttributeLookupRepository = appUnitOfWork.Repository<ItemAttributeLookup>();
            ItemAttributeLookup _itemAttributeLookup = await _itemAttributeLookupRepository.GetByIdAsync(sourceItemAttributeLookupId);
            return _itemAttributeLookup;
        }
        /// <summary>
        /// Get only the data in the item attribute record that matches the sourceItemAttribute
        /// </summary>
        /// <param name="sourceItemAttributeId">the source ItemAttributeId</param>
        /// <returns>the first ItemAttribute that matches the Id or nil</returns>
        public async Task<ItemAttribute> GetOnlyItemAttributeAsync(Guid sourceItemAttributeId)
        {
///-> this is only called when we save, is this the issue, dop we need to rather use the existing repo? Should we put this in ItemRepo?
// --> getting already tracked error
            ItemAttribute _itemAttribute = await appUnitOfWork.itemRepository.GetByIdNoTrackingAsync(sourceItemAttributeId);
            return _itemAttribute;
        }
        /// <summary>
        /// Updates only the ItemAttribute header of the ItemAttribute class
        /// </summary>
        /// <param name="updatedItemAttribute">the updated ITemAttribute</param>
        /// <returns></returns>
        public async Task<int> UpdateOnlyItemAttributeAsync(ItemAttribute updatedItemAttribute)
        {
            // need to detach the item that is the parent to prevent attachment conflicts
            if (await appUnitOfWork.itemRepository.DetachItemById(updatedItemAttribute.ItemId))
                return await appUnitOfWork.itemRepository.UpdateItemAttributeAsync(updatedItemAttribute);
            else
                return UnitOfWork.CONST_WASERROR;
        }

        #endregion

    }
}
