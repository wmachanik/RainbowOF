using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using System;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public interface IItemAttributeGridRepository : IGridRepository<ItemAttribute>
    {
        // over writables are inherited
        #region Interface specific routines
        Task<ItemAttributeLookup> GetItemAttributeByIdAsync(Guid sourceItemAttributeLookupId);
        Task<ItemAttribute> GetOnlyItemAttributeAsync(Guid sourceItemAttributeId);
        Task<int> UpdateOnlyItemAttributeAsync(ItemAttribute updatedItemAttribute);

        #endregion
    }
}
