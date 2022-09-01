using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.ViewModels.Common;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Lookups
{
    public interface IItemAttributeLookupRepository : IRepository<ItemAttributeLookup>
    {
        //- not used any more Task<List<ItemAttributeLookup>> GetAllEagerLoadingAsync();
        Task<DataGridItems<ItemAttributeLookup>> GetPagedDataEagerWithFilterAndOrderByAsync(DataGridParameters currentDataGridParameters);  // (int startPage, int currentPageSize);
    }
}
