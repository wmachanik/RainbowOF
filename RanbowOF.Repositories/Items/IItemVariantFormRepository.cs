using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using System;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public interface IItemVariantFormRepository : IFormRepository<ItemVariant>
    {
        // over writables are inherited
        #region Interface specific routines

        ItemVariant NewBasicViewEntityDefaultSetter(ItemVariant newEntity, Guid ParentId);

        Task<ItemAttributeVarietyLookup> GetItemVariantByIdAsync(Guid sourceItemVariantLookupId);

        #endregion
    }
}
