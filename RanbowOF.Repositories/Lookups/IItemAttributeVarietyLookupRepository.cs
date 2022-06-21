using RainbowOF.Models.Items;
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
    public interface IItemAttributeVarietyLookupRepository : IRepository<ItemAttributeVarietyLookup>
    {
        //- not used any more Task<List<ItemAttributeVarietyLookup>> GetAllEagerLoadingAsync();
        Task<DataGridItems<ItemAttributeVarietyLookup>> GetPagedDataEagerWithFilterAndOrderByAsync(DataGridParameters currentDataGridParameters, Guid sourceParentItemAttributeLookupId);  // (int startPage, int currentPageSize);
    }
}
