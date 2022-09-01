using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using System;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public interface IItemAttributeVarietyGridRepository : IGridRepository<ItemAttributeVariety>
    {
        // over writables are inherited
        #region Interface specific routines
        Task<ItemAttributeVarietyLookup> GetItemAttributeVarietyByIdAsync(Guid sourceItemAttributeVarietyLookupId);
        Task<ItemUoMLookup> GetItemUoMByIdAsync(Guid sourceItemUoMLookupId);
        #endregion
    }
}
