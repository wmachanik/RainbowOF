using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public interface IItemAttributeVarietyGridViewRepository : IGridViewRepository<ItemAttributeVariety>
    {
        // over writables are inherited
        #region Interface specific routines
        Task<ItemAttributeVarietyLookup> GetItemAttributeVarietyByIdAsync(Guid sourceItemAttributeVarietyLookupId);
        Task<ItemUoMLookup> GetItemUoMByIdAsync(Guid sourceItemUoMLookupId);
        #endregion
    }
}
