using Microsoft.EntityFrameworkCore.Storage;
using RainbowOF.Data.SQL;
using RainbowOF.Tools;
using RainbowOF.Repositories.Logs;
using RainbowOF.Repositories.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RainbowOF.Repositories.Items;
using RainbowOF.Repositories.Lookups;
using RainbowOF.Models.Items;
using System.Linq;
using RainbowOF.Models.Lookups;
using System.Linq.Expressions;

namespace RainbowOF.Repositories.Common
{
    /// <summary>
    /// TThis part of the partial class is all lists that are used for select lists combo boxes
    /// </summary>
    /// Other partial classes:
    /// AppUnitOfWork      - the general database and app related routines and variables
    /// AppUnitOfWorkRepos - Generic and Custom Repos
    /// 
    public partial class UnitOfWork : IUnitOfWork
    {
        #region Internal List vars
        private Dictionary<Type, object> _LookupLists = new Dictionary<Type, object>();
        private Dictionary<Guid, string> _ListOfUoMSymbols = null;
        private Dictionary<Guid, List<ItemCategoryLookup>> _ListOfAnItemsCategories = null;
        private Dictionary<Guid, List<Item>> _ListOfSimilarItems = null;
        private Dictionary<Guid, string> _ListOfAttributes = null;
        List<ItemAttributeVarietyLookup> _ListOfAttributeVarieties = null;        
        private Dictionary<Guid, List<ItemAttribute>> _ListOfAnItemsAttributes = null;
        private Dictionary<Guid, List<ItemAttributeVariety>> _ListOfAnItemsAttributeVarieties = null;
        #endregion
        #region lists and variables from database
        public bool DBTransactionIsStillRunning()
        {
            return appDbTransaction != null;
        }
        public Dictionary<Type, object> LookupLists
        {
            get { return _LookupLists; }
            set { _LookupLists = value; }
        }
        public List<TLookupItem> GetListOf<TLookupItem>(bool IsForceReload = false, Func<TLookupItem, object> orderByExpression = null) where TLookupItem : class
        {
            if (LookupLists.ContainsKey(typeof(TLookupItem)))
            {
                if (IsForceReload)
                {
                    LookupLists.Remove(typeof(TLookupItem)); // of we are forcing reload delete the old list
                }
                else
                    return LookupLists[typeof(TLookupItem)] as List<TLookupItem>;
            }
            IRepository<TLookupItem> appRepository = Repository<TLookupItem>();
            if (appRepository == null)
            {
                // problem so return null
                return null;
            }
            List<TLookupItem> lookupItems = (orderByExpression == null) 
                ? appRepository.GetAll().ToList() 
                : appRepository.GetAllOrderBy(orderByExpression, false).ToList();
            //List<TLookupItem> lookupItems = appRepository.GetAll().ToList(); // : appRepository.GetAllOrderBy(orderByExpression, false).ToList();
            LookupLists.Add(typeof(TLookupItem), lookupItems);
            return lookupItems;
        }

        public async Task<List<TLookupItem>> GetListOfAsync<TLookupItem>(bool IsForceReload = false, Expression<Func<TLookupItem, object>> orderByExpression = null) where TLookupItem : class
        {
            ///->> I am not sure this routine works seems to have an async error some where linked to order expression
            if (LookupLists.ContainsKey(typeof(TLookupItem)))
            {
                if (IsForceReload)
                {
                    LookupLists.Remove(typeof(TLookupItem)); // of we are forcing reload delete the old list
                }
                else
                    return LookupLists[typeof(TLookupItem)] as List<TLookupItem>;
            }
            IRepository<TLookupItem> appRepository = Repository<TLookupItem>();
            if (appRepository == null)
            {
                // problem so return null
                return null;
            }
            var lookupItems = (orderByExpression == null)
                 ? await appRepository.GetAllAsync()
                 : await appRepository.GetAllOrderByAsync(orderByExpression, false);
            LookupLists.Add(typeof(TLookupItem), lookupItems);
            return lookupItems.ToList();
        }

        //public Dictionary<Guid, string> GetListOfCategories(bool IsForceReload = false)
        //{
        //    if-`+ ((IsForceReload) || (_listOfCategories == null))
        //    {
        //        if (_listOfCategories != null) _listOfCategories.Clear();
        //        else _listOfCategories = new Dictionary<Guid, string>();

        //        IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = Repository<ItemCategoryLookup>();
        //        var _itemCategories = _itemCategoryLookupRepository.GetAll()
        //            .OrderBy(ic => ic.FullCategoryName)
        //            .ToList();  // cannot async as part of UI for
        //        foreach (var _itemCategory in _itemCategories)
        //        {
        //            _listOfCategories.Add(_itemCategory.ItemCategoryLookupId, _itemCategory.FullCategoryName);
        //        }
        //    }
        //    return _listOfCategories;
        //}
        public Dictionary<Guid, string> GetListOfUoMSymbols(bool IsForceReload = false)
        {
            if (IsForceReload)
            {
                _ListOfUoMSymbols.Clear();
                _ListOfUoMSymbols = null; // would prefer to dispose it but cannot see how
            }
            if (_ListOfUoMSymbols == null)
            {
                IRepository<ItemUoMLookup> appRepository = this.Repository<ItemUoMLookup>();
                var _itemUoMs = appRepository.GetAll(); // (await _UoMRepository.GetAllAsync()).ToList();
                _ListOfUoMSymbols = new();
                if (_itemUoMs != null)
                {
                    foreach (var item in _itemUoMs)
                    {
                        _ListOfUoMSymbols.Add(item.ItemUoMLookupId, item.UoMSymbol);
                    }
                }
            }
            return _ListOfUoMSymbols;
        }
        /// <summary>
        /// Get or return a list of Categories based on the sourceItemId
        /// </summary>
        /// <param name="parentAttributeLookupId">Id of parent</param>
        /// <param name="IsForceReload">must we force reload</param>
        /// <returns>List of Categories</returns>
        //public List<ItemAttributeVariety> GetListOfAttributeVarieties(Guid parentItemAttributeId, bool IsForceReload = false)
        public List<ItemCategoryLookup> GetListOfAnItemsCategories(Guid sourceItemId, bool IsForceReload = false)
        {
            if ((_ListOfAnItemsCategories == null) || (IsForceReload) || (!_ListOfAnItemsCategories.ContainsKey(sourceItemId)))
            {
                List<ItemCategoryLookup> _listOfCategories = itemRepository.GetEagerItemsCategoryLookupsByItemId(sourceItemId)
                                                                            .OrderBy(icl => icl.FullCategoryName)
                                                                            .ToList();   // get a list of only the category lookups this Item order by full category name
                if ((_ListOfAnItemsCategories != null) && (_ListOfAnItemsCategories.ContainsKey(sourceItemId)))
                {
                    _ListOfAnItemsCategories[sourceItemId].Clear();
                    _ListOfAnItemsCategories[sourceItemId] = _listOfCategories;
                }
                else
                {
                    if (_ListOfAnItemsCategories == null) _ListOfAnItemsCategories = new();
                    _ListOfAnItemsCategories.Add(sourceItemId, _listOfCategories);
                }
            }
            return _ListOfAnItemsCategories[sourceItemId];
        }
        public List<Item> GetListOfSimilarItems(Guid sourceItemId, Guid? sourceItemPrimaryCategoryId, bool IsForceReload = false)
        {
            if ((_ListOfSimilarItems == null) || (IsForceReload) || (!_ListOfSimilarItems.ContainsKey(sourceItemId)))
            {
                List<Item> _listOfSimilarItems = itemRepository.GetSimilarItems(sourceItemId, sourceItemPrimaryCategoryId);   // get a list of items with the same parent category or all with parent category null
                if ((_ListOfSimilarItems != null) && (_ListOfSimilarItems.ContainsKey(sourceItemId)))
                {
                    _ListOfSimilarItems[sourceItemId].Clear();
                    _ListOfSimilarItems[sourceItemId] = _listOfSimilarItems;
                }
                else
                {
                    if (_ListOfSimilarItems == null) _ListOfSimilarItems = new();
                    _ListOfSimilarItems.Add(sourceItemId, _listOfSimilarItems);
                }
            }
            return _ListOfSimilarItems[sourceItemId];
        }
        public Dictionary<Guid, string> GetListOfAttributes(bool IsForceReload = false)
        {
            if ((IsForceReload) || (_ListOfAttributes == null))
            {
                if (_ListOfAttributes != null) _ListOfAttributes.Clear();
                else _ListOfAttributes = new Dictionary<Guid, string>();

                IRepository<ItemAttributeLookup> _itemAttributeLookupRepository = Repository<ItemAttributeLookup>();
                var _itemAttributes = _itemAttributeLookupRepository.GetAll()
                    .OrderBy(ia => ia.AttributeName)
                    .ToList();  // cannot async as part of UI for
                foreach (var _itemAttribute in _itemAttributes)
                {
                    _ListOfAttributes.Add(_itemAttribute.ItemAttributeLookupId, _itemAttribute.AttributeName);
                }
            }
            return _ListOfAttributes;
        }
        public List<ItemAttributeVarietyLookup> GetListOfAttributeVarieties(Guid parentAttributeLookupId, bool IsForceReload = false)
        {
            if ((IsForceReload) || (_ListOfAttributeVarieties == null))
            {
                if (_ListOfAttributeVarieties != null) _ListOfAttributeVarieties.Clear();
                else _ListOfAttributeVarieties = new List<ItemAttributeVarietyLookup>();

                IRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyLookupRepository = Repository<ItemAttributeVarietyLookup>();
                _ListOfAttributeVarieties = _itemAttributeVarietyLookupRepository.GetBy(avl => avl.ItemAttributeLookupId == parentAttributeLookupId)
                    .OrderBy(iav => iav.VarietyName)
                    .ToList();  // cannot async as part of UI for

            }
            return _ListOfAttributeVarieties;
        }
        /// <summary>
        /// Get or return a list of Attributes based on the sourceItemId
        /// </summary>
        /// <param name="parentAttributeLookupId">Id of parent</param>
        /// <param name="IsForceReload">must we force reload</param>
        /// <returns>List of attributes</returns>
        //public List<ItemAttributeVariety> GetListOfAttributeVarieties(Guid parentItemAttributeId, bool IsForceReload = false)
        public List<ItemAttribute> GetListOfAnItemsVariableAttributes(Guid sourceItemId, bool IsForceReload = false)
        {
            if ((_ListOfAnItemsAttributes == null) || (IsForceReload) || (!_ListOfAnItemsAttributes.ContainsKey(sourceItemId)))
            {
                List<ItemAttribute> _listOfAttributes = itemRepository.GetEagerItemVariableAttributeByItemId(sourceItemId);
                if ((_ListOfAnItemsAttributes != null) && (_ListOfAnItemsAttributes.ContainsKey(sourceItemId)))
                {
                    _ListOfAnItemsAttributes[sourceItemId].Clear();
                    _ListOfAnItemsAttributes[sourceItemId] = _listOfAttributes;
                }
                else
                {
                    if (_ListOfAnItemsAttributes == null) _ListOfAnItemsAttributes = new();
                    _ListOfAnItemsAttributes.Add(sourceItemId, _listOfAttributes);
                }
            }
            return _ListOfAnItemsAttributes[sourceItemId];
        }
        /// <summary>
        /// Get or return a list of Attribute Varieties based on the sourceItemId and associatedAttributeId
        /// </summary>
        /// <param name="sourceItemId"></param>
        /// <param name="sourceAssoicatedAttributeId"></param>
        /// <param name="IsForceReload"></param>
        /// <returns></returns>
        public List<ItemAttributeVariety> GetListOfAnItemsAttributeVarieties(Guid sourceItemId, Guid sourceAttributeLookupId, bool IsForceReload = false)
        {
            if ((_ListOfAnItemsAttributeVarieties == null) || (IsForceReload) || (!_ListOfAnItemsAttributeVarieties.ContainsKey(sourceAttributeLookupId)))
            {
                List<ItemAttributeVariety> _listOfItemsAttributeVarieties = itemRepository.GetEagerItemAttributeVarietiesByItemIdAndAttributeLookupId(sourceItemId, sourceAttributeLookupId);
                if ((_ListOfAnItemsAttributeVarieties != null) && (_ListOfAnItemsAttributeVarieties.ContainsKey(sourceAttributeLookupId)))
                {
                    _ListOfAnItemsAttributeVarieties[sourceAttributeLookupId].Clear();
                    _ListOfAnItemsAttributeVarieties[sourceAttributeLookupId] = _listOfItemsAttributeVarieties;
                }
                else
                {
                    if (_ListOfAnItemsAttributeVarieties == null) _ListOfAnItemsAttributeVarieties = new();
                    _ListOfAnItemsAttributeVarieties.Add(sourceAttributeLookupId, _listOfItemsAttributeVarieties);
                }
            }
            return _ListOfAnItemsAttributeVarieties[sourceAttributeLookupId];
        }
        public async Task<List<ItemAttributeVariety>> GetListOfItemAttributeVarietiesAsync(Guid sourceItemId, Guid sourceAttributeLookupId, bool IsForceReload = false)
        {
            if ((IsForceReload) || (!_ListOfAnItemsAttributeVarieties.ContainsKey(sourceAttributeLookupId)))
            {
                List<ItemAttributeVariety> _listOfItemsAttributeVarieties = await itemRepository.GetEagerItemAttributeVarietiesByItemIdAndAttributeLookupIdAsync(sourceItemId, sourceAttributeLookupId);
                if (_ListOfAnItemsAttributeVarieties.ContainsKey(sourceItemId))
                {
                    _ListOfAnItemsAttributeVarieties[sourceAttributeLookupId].Clear();
                    _ListOfAnItemsAttributeVarieties[sourceAttributeLookupId] = _listOfItemsAttributeVarieties;
                }
                else
                    _ListOfAnItemsAttributeVarieties.Add(sourceAttributeLookupId, _listOfItemsAttributeVarieties);
            }
            return _ListOfAnItemsAttributeVarieties[sourceAttributeLookupId];
        }

        #endregion
    }
}
