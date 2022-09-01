using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.ViewModels.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Lookups
{
    public interface IItemCategoryLookupRepository : IRepository<ItemCategoryLookup>
    {
        Task<List<ItemCategoryLookup>> GetAllEagerLoadingAsync();
        Task<DataGridItems<ItemCategoryLookup>> GetPagedDataEagerWithFilterAndOrderByAsync(DataGridParameters currentDataGridParameters);  // (int startPage, int currentPageSize);
    }
}
