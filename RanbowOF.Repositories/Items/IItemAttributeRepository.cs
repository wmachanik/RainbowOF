
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
    public  interface IItemAttributeRepository : IRepository<ItemAttribute>
    {
        #region Retrieval
        List<ItemAttribute> GetEagerItemAttributeByItemIdAsync(Guid sourceItemId);

        #endregion  
        #region ItemAttribute related routines
        Task<bool> IsUniqueItemAttributeAsync(ItemAttribute sourceItemAttribute);
        List<ItemAttributeVariety> GetAssociatedVarients(Guid sourceAttributeLookupId);
        Task<List<ItemAttributeVariety>> GetAssociatedVarientsAsync(Guid sourceAttributeLookupId);
        Task<ItemAttributeLookup> GetItemAttributeLookupByIdAsync(Guid sourceItemAttributeLookupId);
        Task<ItemAttribute> GetItemAttributeByIdNoTrackingAsync(Guid sourceItemAttributeId);
        Task<int> UpdateItemAttributeAsync(ItemAttribute sourceItemAttribute);
        #endregion
    }
}
