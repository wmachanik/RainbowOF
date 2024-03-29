﻿using RainbowOF.Models.Items;
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
    public class ItemAttributeVarietyGridViewRepository : GridViewRepository<ItemAttributeVariety>, IItemAttributeVarietyGridViewRepository
    {
        public ItemAttributeVarietyGridViewRepository(ILoggerManager sourceLogger, IAppUnitOfWork sourceAppUnitOfWork) : base(sourceLogger, sourceAppUnitOfWork)
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
        public override ItemAttributeVariety NewViewEntityDefaultSetter(ItemAttributeVariety newEntity, Guid ParentId)
        {
            if (newEntity == null)
                newEntity = new ItemAttributeVariety();

            newEntity.ItemAttributeId = ParentId;
            newEntity.IsDefault = false;
            newEntity.UoMId = Guid.Empty;
            newEntity.UoMQtyPerItem = 1;
            newEntity.ItemAttributeVarietyLookupId = Guid.Empty;
            return newEntity;
        }
        /// <summary>
        /// See if entity a duplicate, this needs to be overwritten at a class specific level.
        /// Logic: return false, as we have no idea what will make the generic class a duplicate.
        /// #pragma warning disable used so we do not get the warning in the compiler
        /// </summary>
        /// <param name="checkEntity">Entity to be checked</param>
        /// <returns>Bool true if a duplicate false if not. </returns>
        public override async Task<bool> IsDuplicateAsync(ItemAttributeVariety checkEntity)
        {
            if (checkEntity.ItemAttributeVarietyLookupId == Guid.Empty)
                return false;
            // here we need to check if the AttributeVariety is the same, as we cannot haver duplicate names
            IAppRepository<ItemAttributeVariety> _itemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVariety>();
            var _result = await _itemAttributeVarietyRepository.FindFirstByAsync(ic => (ic.ItemAttributeVarietyId == checkEntity.ItemAttributeVarietyId) && (ic.ItemAttributeVarietyLookupId == checkEntity.ItemAttributeVarietyLookupId));
            return (_result != null);
        }
        /// <summary>
        /// See if entity valid, this needs to be overwritten at a class specific level.
        /// Logic: return true, as we have no idea what will make the generic class valid.
        /// </summary>
        /// <param name="checkEntity">TEntity sourceEntity: Entity to be checked</param>
        /// <returns>Bool true if a valid false if not. </returns>
        public override bool IsValid(ItemAttributeVariety checkEntity)
        {
            return (checkEntity.ItemAttributeVarietyLookupId != Guid.Empty);
        }
        #endregion
        #region Interface specific routines
        /// <summary>
        /// Get the Item AttributeVariety Lookup Referenced by its Id
        /// Logic: Use generic GetById to return the ItemAttributeVarietyLookup by Id
        /// </summary>
        /// <param name="sourceItemAttributeVarietyLookupId">Id to search for</param>
        /// <returns>ItemAttributeVarietyLookup item, if found or null</returns>
        public async Task<ItemAttributeVarietyLookup> GetItemAttributeVarietyByIdAsync(Guid sourceItemAttributeVarietyLookupId)
        {
            IAppRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyLookupRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            ItemAttributeVarietyLookup _itemAttributeVarietyLookup = await _itemAttributeVarietyLookupRepository.GetByIdAsync(sourceItemAttributeVarietyLookupId);
            return _itemAttributeVarietyLookup;
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
