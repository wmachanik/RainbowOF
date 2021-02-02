using RainbowOF.Models.Items;
using RainbowOF.Repositories.Common;
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
        /// <summary>
        /// Adds an item to the database that is fully populated, making sure that the values set within it are set.
        /// </summary>
        /// <param name="newItem">New fully populateded Item</param>
        /// <returns>First Item complete with categories, attributes and variations</returns>
        Task<Item> FindFirstWholeItemAsync(Expression<Func<Item, bool>> predicate);
        /// <summary>
        /// Find the first Itemin the system by SKU
        /// </summary>
        /// <param name="SKU">SKU to search for</param>
        /// <returns>null for none and Item if found</returns>
        Task<Item> FindFirstItemBySKU(string pSKU);
        /// <summary>
        /// Add and Item to the datasechecking for SKU duplication first
        /// </summary>
        /// <param name="newItem">New item to add</param>
        /// <returns>Nuber of records added or Error (AppUnitOfWork.CONST_WASERROR)</returns>
        Task<int> AddItem(Item newItem);
    }
}
