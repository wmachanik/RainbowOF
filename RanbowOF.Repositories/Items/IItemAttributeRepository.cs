
using RainbowOF.Models.Items;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public  interface IItemAttributeRepository : IAppRepository<ItemAttribute>
    {
        List<ItemAttributeVariety> GetAssociatedVarients(Guid sourceAttributeLookupId);
        Task<List<ItemAttributeVariety>> GetAssociatedVarientsAsync(Guid sourceAttributeLookupId);
    }
}
