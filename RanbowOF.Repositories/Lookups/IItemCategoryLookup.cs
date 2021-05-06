using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Lookups
{
    public interface IItemCategoryLookup : IAppRepository<ItemCategoryLookup>
    {
        Task<IEnumerable<ItemCategoryLookup>> GetPagedEagerWithOrderByAsync(int startPage, int currentPageSize);
    }
}
