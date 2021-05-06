using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Lookups
{
    public interface IItemCategoryLookupRepository : IAppRepository<ItemCategoryLookup>
    {
        Task<List<ItemCategoryLookup>> GetAllEagerLoadingAsync();
        Task<DataGridItems<ItemCategoryLookup>> GetPagedDataEagerWithFilterAndOrderByAsync(DataGridParameters currentDataGridParameters);  // (int startPage, int currentPageSize);
    }
}
