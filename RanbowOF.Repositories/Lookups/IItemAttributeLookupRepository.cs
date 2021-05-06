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
    public interface IItemAttributeLookupRepository : IAppRepository<ItemAttributeLookup>
    {
        //- not used any more Task<List<ItemAttributeLookup>> GetAllEagerLoadingAsync();
        Task<DataGridItems<ItemAttributeLookup>> GetPagedDataEagerWithFilterAndOrderByAsync(DataGridParameters currentDataGridParameters);  // (int startPage, int currentPageSize);
    }
}
