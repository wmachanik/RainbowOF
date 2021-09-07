using RainbowOF.Models.Items;
using RainbowOF.Repositories.Common;
using RainbowOF.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public interface IItemRepository : IAppRepository<Item>
    {
        //Task<List<Item>> GetAllItemData(Expression<Func<Item, bool>> predicate);
        /// <summary>
        /// Get a List of DataGridItems using data grid data passed in
        /// </summary>
        /// <param name="currentDataGridParameters">the current data grid parameters</param>
        /// <returns></returns>
        Task<DataGridItems<Item>> GetPagedDataEagerWithFilterAndOrderByAsync(DataGridParameters currentDataGridParameters);  
        /// <summary>
        /// Adds an item to the database that is fully populated, making sure that the values set within it are set.
        /// </summary>
        /// <param name="newItem">New fully populated Item</param>
        /// <returns>First Item complete with categories, attributes and variations</returns>
        Task<Item> FindFirstEagerLoadingItemAsync(Expression<Func<Item, bool>> predicate);
        /// <summary>
        /// Find the first Item in the system by SKU
        /// </summary>
        /// <param name="SKU">SKU to search for</param>
        /// <returns>null for none and Item if found</returns>
        Task<Item> FindFirstItemBySKUAsync(string sourceSKU);
        /// <summary>
        /// Add and Item to the cross-checking for SKU duplication first
        /// </summary>
        /// <param name="newItem">New item to add</param>
        /// <returns>Number of records added or Error (AppUnitOfWork.CONST_WASERROR)</returns>
        Task<int> AddItemAsync(Item newItem);
    }
}
