using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public class ItemVariantFormRepository : FormRepository<ItemVariant>, IItemVariantFormRepository
    {
        public ItemVariantFormRepository(ILoggerManager sourceLogger, IUnitOfWork sourceAppUnitOfWork) : base(sourceLogger, sourceAppUnitOfWork)
        {
            // base initialises
            sourceLogger.LogDebug("ItemVariantGridRepository initialised.");
            //formSettings.PopUpRef = new Components.Modals.PopUpAndLogNotification();
            //formSettings.DeleteConfirmation = new Components.Modals.ConfirmModal();
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
            // this should look for the number Attributes at are marked at variable attributes and then add a list per item
            newEntity = NewBasicViewEntityDefaultSetter(newEntity, ParentId);
            // now add the possible variants
            var possibleVariables = AppUnitOfWork.GetListOfAnItemsVariableAttributes(ParentId);
            foreach (var variable in possibleVariables)
            {
                newEntity.ItemVariantAssociatedLookups.Add(new ItemVariantAssociatedLookup
                {
                    AssociatedAttributeLookupId = variable.ItemAttributeLookupId,
                    AssociatedAttributeLookup = variable.ItemAttributeDetail,
                    AssociatedAttributeVarietyLookupId = null
                });
            }
            return newEntity;
        }
        public ItemVariant NewBasicViewEntityDefaultSetter(ItemVariant newEntity, Guid ParentId)
        {
            if (newEntity == null)
                newEntity = new ItemVariant();

            newEntity.ItemId = ParentId;
            newEntity.ItemVariantName = "new variant";
            newEntity.IsEnabled = true;
            newEntity.ItemVariantAbbreviation = "Abrv0";
            newEntity.ManageStock = false;
            newEntity.QtyInStock = 0;
            newEntity.SKU = "SKAnother";
            newEntity.ItemVariantAssociatedLookups = new();

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
            if (checkEntity.ItemVariantAssociatedLookups.Exists(ival => ival.AssociatedAttributeVarietyLookupId == Guid.Empty))
                return false;
            // here we need to check if the Variant is the same, as we cannot haver duplicate names
            IRepository<ItemVariant> _itemVariantRepository = AppUnitOfWork.Repository<ItemVariant>();

            // If ItemVariantIOd = Empty then it is new and check if there is not one with the same associated lookups. so need to use exists and any
            // sample??
            var _testresult = await _itemVariantRepository.GetByIdAsync(iav => iav.ItemVariantAssociatedLookups.Exists(iavl => checkEntity.ItemVariantAssociatedLookups.Any(ce => ce.AssociatedAttributeLookupId == iavl.AssociatedAttributeLookupId)));

            var _result = await _itemVariantRepository.GetByIdAsync(
                        checkEntity.ItemVariantId == Guid.Empty ?
                            (iav =>
                               (
                                      (iav.ItemId == checkEntity.ItemId)
                                   && (iav.ItemVariantAssociatedLookups.Exists(ival => checkEntity.ItemVariantAssociatedLookups.Any(ce => ce.AssociatedAttributeLookupId == ival.AssociatedAttributeLookupId)))
                                )
                            )
                            :
                            (iav =>
                                (
                                    (iav.ItemId == checkEntity.ItemId)
                                && (iav.ItemVariantId != checkEntity.ItemVariantId)
                                && (iav.ItemVariantAssociatedLookups.Exists(ival => checkEntity.ItemVariantAssociatedLookups.Any(ce => ce.AssociatedAttributeLookupId == ival.AssociatedAttributeLookupId)))
                                )
                            )
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
            return !(checkEntity.ItemVariantAssociatedLookups.Exists(ce => ce.AssociatedAttributeVarietyLookupId == Guid.Empty));
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
            IRepository<ItemAttributeVarietyLookup> _itemAttributeVariantLookupRepository = AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            ItemAttributeVarietyLookup _itemVariantLookup = await _itemAttributeVariantLookupRepository.GetByIdAsync(sourceItemVariantLookupId);
            return _itemVariantLookup;
        }
        #endregion

    }
}
