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
    public class ItemAttributeGridViewRepository : GridViewRepository<ItemAttribute>, IItemAttributeGridViewRepository
    {
        public ItemAttributeGridViewRepository(ILoggerManager sourceLogger, IAppUnitOfWork sourceAppUnitOfWork) : base(sourceLogger, sourceAppUnitOfWork)
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
            // here we need to check if the Attribute is the same, as we cannot haver duplicate names
            IAppRepository<ItemAttribute> _itemAttributeRepository = _AppUnitOfWork.Repository<ItemAttribute>();
            var _result = await _itemAttributeRepository.FindFirstByAsync(ic => (ic.ItemAttributeId == checkEntity.ItemAttributeId) && (ic.ItemAttributeLookupId == checkEntity.ItemAttributeLookupId));
            return (_result != null);
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
            IAppRepository<ItemAttributeLookup> _itemAttributeLookupRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            ItemAttributeLookup _itemAttributeLookup = await _itemAttributeLookupRepository.GetByIdAsync(sourceItemAttributeLookupId);
            return _itemAttributeLookup;
        }
        /// <summary>
        /// Get the Item Unit of Measure (UoM) referenced by its Id
        /// </summary>
        /// <param name="sourceItemUoMLookupId">ItemUoMId to search for</param>
        /// <returns>UoM Lookup item, if found or null</returns>
        public async Task<ItemUoMLookup> GetItemUoMByIdAsync(Guid sourceItemUoMLookupId)
        {
            IAppRepository<ItemUoMLookup> _itemUoMLookupRepository = _AppUnitOfWork.Repository<ItemUoMLookup>();
            ItemUoMLookup _itemUoMLookup = await _itemUoMLookupRepository.GetByIdAsync(sourceItemUoMLookupId);
            return _itemUoMLookup;
        }
        #endregion

    }
}
