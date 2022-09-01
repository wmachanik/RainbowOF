using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    public interface IItemRepository : IRepository<Item>
    {
        #region Basic CRUD stuff
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
        Task<Item> AddItemAsync(Item newItem);
        Task<bool> DetachItemById(Guid sourceItemId);

        #endregion
        #region Lists and Lookup ups
        List<Item> GetSimilarItems(Guid sourceItemId, Guid? sourceItemPrimaryCategoryId);
        List<ItemCategoryLookup> GetEagerItemsCategoryLookupsByItemId(Guid sourceItemId);
        // These work with Attributes and Attribute varieties, they cold be in a separate interface but as they are relate to the Item, they are here
        /// <summary>
        /// Get all the Attributes for an Item by ItemId
        /// </summary>
        /// <param name="ItemId">The Item's Id</param>
        /// <returns>List of Item Attributes</returns>
        List<ItemAttribute> GetEagerItemVariableAttributeByItemId(Guid sourceItemId);
        List<ItemAttributeVariety> GetEagerItemAttributeVarietiesByItemIdAndAttributeLookupId(Guid sourceItemId, Guid sourceItemAttributeLookupId);
        Task<List<ItemAttributeVariety>> GetEagerItemAttributeVarietiesByItemIdAndAttributeLookupIdAsync(Guid sourceItemId, Guid sourceItemAttributeLookupId);
        Task<bool> DoesThisItemHaveVariableAttributesAsync(Guid currentItemId);
        Task<bool> SetItemTypeAsync(Guid currentItemId, ItemTypes currentItemType);
        #endregion
        //List<ItemAttribute> GetAllItemVariableAttributesByItemId(Guid sourceItemId);
        #region ItemVariants of an item and supporting routines
        /// <summary>
        /// Get all the Item Variants and all the associated lookup tables based on sourceItemId
        /// </summary>
        /// <param name="sourceItemId">The ItemId of the parent item</param>
        /// <returns></returns>
        Task<List<ItemVariant>> GetAllItemVariantsEagerByItemIdAsync(Guid sourceItemId);
        /// <summary>
        /// Get the Item variant and all the related tables / Lists for the Item Variant whose Id is passed in
        /// </summary>
        /// <param name="sourceItemVariant">Id of the ItemVariant to be returned</param>
        /// <returns>The Item variant with all related lists retrieved.</returns>
        Task<ItemVariant> GetItemVariantEagerByItemVariantIdAsync(Guid sourceItemVariantId);
        ///// <summary>
        ///// Get all the variants that the item has an attribute marked as variable 
        ///// </summary>
        ///// <param name="sourceItemId">The ItemId of the parent Item</param>
        ///// <returns></returns>
        //Task<List<ItemAttributeVariety>> GetAllPossibleVariants(Guid sourceItemId);
        Task<bool> DeleteItemVariantAndAssociatedData(ItemVariant sourceItemVariant);
        /// <summary>
        /// Delete all the variants of this item
        /// </summary>
        /// <param name="sourceItemId">The Id of the item whose variants we wont to delete</param>
        /// <returns>true of successful</returns>
        Task<bool> DeleteAllItemsVariantsAsync(Guid sourceItemId);


        #endregion
    }
}
