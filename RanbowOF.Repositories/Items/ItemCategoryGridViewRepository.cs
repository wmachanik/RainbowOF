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
    public class ItemCategoryGridViewRepository : GridViewRepository<ItemCategory>, IItemCategoryGridViewRepository
    {
        public ItemCategoryGridViewRepository(ILoggerManager sourceLogger, IAppUnitOfWork sourceAppUnitOfWork) : base(sourceLogger, sourceAppUnitOfWork)
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
        public override ItemCategory NewViewEntityDefaultSetter(ItemCategory newEntity, Guid ParentId)
        {
            if (newEntity == null)
                newEntity = new ItemCategory();

            newEntity.ItemId = ParentId;
            newEntity.UsedForPrediction = false;
            newEntity.ItemCategoryLookupId = Guid.Empty;
            return newEntity;
        }
        /// <summary>
        /// See if entity a duplicate, this needs to be overwritten at a class specific level.
        /// Logic: return false, as we have no idea what will make the generic class a duplicate.
        /// #pragma warning disable used so we do not get the warning in the compiler
        /// </summary>
        /// <param name="checkEntity">Entity to be checked</param>
        /// <returns>Bool true if a duplicate false if not. </returns>
        public override async Task<bool> IsDuplicateAsync(ItemCategory checkEntity)
        {
            if (checkEntity.ItemCategoryLookupId == Guid.Empty)
                return false;
            // here we need to check if the category is the same, as we cannot haver duplicate names
            IAppRepository<ItemCategory> _itemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();
            var _result = await _itemCategoryRepository.FindFirstByAsync(ic => (ic.ItemCategoryId == checkEntity.ItemCategoryId) && (ic.ItemCategoryLookupId == checkEntity.ItemCategoryLookupId));
            return (_result != null);
        }
        /// <summary>
        /// See if entity valid, this needs to be overwritten at a class specific level.
        /// Logic: return true, as we have no idea what will make the generic class valid.
        /// </summary>
        /// <param name="checkEntity">TEntity sourceEntity: Entity to be checked</param>
        /// <returns>Bool true if a valid false if not. </returns>
        public override bool IsValid(ItemCategory checkEntity)
        {
            return (checkEntity.ItemCategoryLookupId != Guid.Empty);
        }
        #endregion
        #region Interface specific routines
        /// <summary>
        /// Get the Item Category Lookup Referenced by its Id
        /// Logic: Use generic GetById to return the ItemCategoryLookup by Id
        /// </summary>
        /// <param name="sourceItemCategoryLookupId">Id to search for</param>
        /// <returns>ItemCategoryLookup item, if found or null</returns>
        public async Task<ItemCategoryLookup> GetItemCategoryByIdAsync(Guid sourceItemCategoryLookupId)
        {
            IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
            ItemCategoryLookup _itemCategoryLookup = await _itemCategoryLookupRepository.GetByIdAsync(sourceItemCategoryLookupId);
            return _itemCategoryLookup;
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
