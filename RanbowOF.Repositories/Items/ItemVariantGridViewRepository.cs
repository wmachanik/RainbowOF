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
    public class ItemVariantGridViewRepository : GridViewRepository<ItemVariant>, IItemVariantGridViewRepository
    {
        public ItemVariantGridViewRepository(ILoggerManager sourceLogger, IAppUnitOfWork sourceAppUnitOfWork) : base(sourceLogger, sourceAppUnitOfWork)
        {
            // base initialises
            sourceLogger.LogDebug("ItemVariantGridViewRepository initialised.");
        }
        #region Recommended Over writable Grid Classes
        /// <summary>
        /// Used to create a new entity when the add item button is pressed, this needs to be overwritten at a class specific level.
        /// Logic: return null, as we have no idea what will make the generic class valid.
        /// </summary>
        /// <param name="newEntity">TEntity newEntity: blank entity to be initialised</param>
        /// <returns>null.</returns>
        public override ItemVariant NewViewEntityDefaultSetter(ItemVariant newEntity, Guid ParentId)
        {
            if (newEntity == null)
                newEntity = new ItemVariant();

            newEntity.ItemId = ParentId;
            newEntity.ItemVariantName = "";
            newEntity.ManageStock = false;
            return newEntity;
        }

        /// <summary>
        /// See if entity a duplicate, this needs to be overwritten at a class specific level.
        /// Logic: return false, as we have no idea what will make the generic class a duplicate.
        /// #pragma warning disable used so we do not get the warning in the compiler
        /// </summary>
        /// <param name="checkEntity">Entity to be checked</param>
        /// <returns>Bool true if a duplicate false if not. </returns>
        public override async Task<bool> IsDuplicateAsync(ItemVariant checkEntity)
        {
            if (checkEntity.AssociatedAttributeVarietyLookupId == Guid.Empty)
                return false;
            // here we need to check if the Variant is the same, as we cannot haver duplicate names
            IAppRepository<ItemVariant> _itemVariantRepository = _AppUnitOfWork.Repository<ItemVariant>();
            var _result = await _itemVariantRepository.FindFirstByAsync(
                                    checkEntity.ItemVariantId == Guid.Empty ?
                                        (iav => (iav.ItemId == checkEntity.ItemId)
                                            && (iav.AssociatedAttributeVarietyLookupId == checkEntity.AssociatedAttributeVarietyLookupId)) :
                                        (iav => (iav.ItemId == checkEntity.ItemId)
                                            && (iav.ItemVariantId != checkEntity.ItemVariantId)
                                            && (iav.AssociatedAttributeVarietyLookupId == checkEntity.AssociatedAttributeVarietyLookupId))
                                      );
            return (_result != null);
        }
        /// <summary>
        /// See if entity valid, this needs to be overwritten at a class specific level.
        /// Logic: return true, as we have no idea what will make the generic class valid.
        /// </summary>
        /// <param name="checkEntity">TEntity sourceEntity: Entity to be checked</param>
        /// <returns>Bool true if a valid false if not. </returns>
        public override bool IsValid(ItemVariant checkEntity)
        {
            return (checkEntity.AssociatedAttributeVarietyLookupId != Guid.Empty);
        }
        #endregion
        #region Interface specific routines
        /// <summary>
        /// Get the Item Variant Lookup Referenced by its Id
        /// Logic: Use generic GetById to return the ItemVariantLookup by Id
        /// </summary>
        /// <param name="sourceItemVariantLookupId">Id to search for</param>
        /// <returns>ItemVariantLookup item, if found or null</returns>
        public async Task<ItemAttributeVarietyLookup> GetItemVariantByIdAsync(Guid sourceItemVariantLookupId)
        {
            IAppRepository<ItemAttributeVarietyLookup> _itemAttributeVariantLookupRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            ItemAttributeVarietyLookup _itemVariantLookup = await _itemAttributeVariantLookupRepository.GetByIdAsync(sourceItemVariantLookupId);
            return _itemVariantLookup;
        }
        #endregion

    }
}
